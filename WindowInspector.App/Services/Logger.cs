using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WindowInspector.App.Services;

/// <summary>
/// Handles application-wide logging with file output
/// </summary>
public sealed class Logger : IDisposable
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    private readonly string _logDirectory;
    private string _currentLogFile;
    private readonly BlockingCollection<LogEntry> _logQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processQueueTask;
    private const int MAX_LOG_SIZE_MB = 10;
    private const int MAX_LOG_FILES = 5;
    private readonly StringBuilder _logBuffer;
    private const int FLUSH_THRESHOLD = 50;
    private int _bufferedLineCount;
    private readonly object _bufferLock = new();
    private DateTime _lastFlushTime = DateTime.Now;
    private readonly TimeSpan _maxBufferAge = TimeSpan.FromSeconds(1);

    public static Logger Instance => _instance.Value;

    private Logger()
    {
        // Set up log directory in user's AppData
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowInspector",
            "Logs"
        );
        Directory.CreateDirectory(_logDirectory);

        // Set up current log file with timestamp
        _currentLogFile = Path.Combine(_logDirectory, $"WindowInspector_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        // Initialize buffer and queue
        _logBuffer = new StringBuilder(4096);
        _logQueue = new BlockingCollection<LogEntry>();
        _cancellationTokenSource = new CancellationTokenSource();
        _processQueueTask = Task.Run(ProcessLogQueue);

        // Log startup
        Info("Logger initialized");
        Info($"Log file: {_currentLogFile}");
    }

    public void Info(string message) => EnqueueLog(LogLevel.Info, message);
    public void Warning(string message) => EnqueueLog(LogLevel.Warning, message);
    public void Error(string message) => EnqueueLog(LogLevel.Error, message);
    
    public void Error(Exception ex)
    {
        var message = new StringBuilder();
        message.AppendLine("Exception occurred:");
        message.AppendLine($"Message: {ex.Message}");
        message.AppendLine($"Source: {ex.Source}");
        message.AppendLine($"Target site: {ex.TargetSite}");
        message.AppendLine("Stack trace:");
        message.AppendLine(ex.StackTrace);

        if (ex.InnerException != null)
        {
            message.AppendLine("Inner exception:");
            message.AppendLine(ex.InnerException.ToString());
        }

        EnqueueLog(LogLevel.Error, message.ToString());
    }

    private void EnqueueLog(LogLevel level, string message)
    {
        if (_logQueue.IsAddingCompleted) return;

        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            ThreadId = Environment.CurrentManagedThreadId,
            ProcessId = Process.GetCurrentProcess().Id
        };

        _logQueue.Add(entry);
    }

    private void BufferLogEntry(LogEntry entry, bool forceFlush = false)
    {
        var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] [PID:{entry.ProcessId}] [TID:{entry.ThreadId}] {entry.Message}";
        
        lock (_bufferLock)
        {
            _logBuffer.AppendLine(line);
            _bufferedLineCount++;

            var shouldFlush = forceFlush ||
                            _bufferedLineCount >= FLUSH_THRESHOLD ||
                            (DateTime.Now - _lastFlushTime) >= _maxBufferAge;

            if (shouldFlush)
            {
                _ = FlushBufferAsync();
            }
        }
    }

    private async Task ProcessLogQueue()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var entry = _logQueue.Take(_cancellationTokenSource.Token);
                BufferLogEntry(entry);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // If logging fails, write to debug output as last resort
                Debug.WriteLine($"Error in log processing: {ex}");
            }
        }

        // Process remaining entries and flush buffer on shutdown
        while (_logQueue.TryTake(out var entry))
        {
            try
            {
                BufferLogEntry(entry, true);
            }
            catch
            {
                // Ignore errors during shutdown
            }
        }
    }

    private async Task FlushBufferAsync()
    {
        string contentToWrite;
        lock (_bufferLock)
        {
            if (_logBuffer.Length == 0) return;
            contentToWrite = _logBuffer.ToString();
            _logBuffer.Clear();
            _bufferedLineCount = 0;
            _lastFlushTime = DateTime.Now;
        }

        try
        {
            await File.AppendAllTextAsync(_currentLogFile, contentToWrite);
            await CheckLogSizeAndRotateAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write log entry: {ex.Message}");
        }
    }

    private async Task CheckLogSizeAndRotateAsync()
    {
        try
        {
            var fileInfo = new FileInfo(_currentLogFile);
            if (fileInfo.Exists && fileInfo.Length > MAX_LOG_SIZE_MB * 1024 * 1024)
            {
                // Ensure buffer is flushed before rotation
                await FlushBufferAsync();

                // Rotate files
                var files = Directory.GetFiles(_logDirectory, "WindowInspector_*.log")
                    .OrderByDescending(f => f)
                    .ToList();

                // Remove old files
                while (files.Count >= MAX_LOG_FILES)
                {
                    var oldFile = files.Last();
                    try
                    {
                        File.Delete(oldFile);
                        files.RemoveAt(files.Count - 1);
                    }
                    catch
                    {
                        // Ignore deletion errors
                    }
                }

                // Create new file
                _currentLogFile = Path.Combine(_logDirectory, $"WindowInspector_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                Info("Log file rotated");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to rotate log file: {ex.Message}");
        }
    }

    public async Task FlushAsync()
    {
        await FlushBufferAsync();
    }

    public void Dispose()
    {
        if (!_logQueue.IsAddingCompleted)
        {
            Info("Logger shutting down...");
            _logQueue.CompleteAdding();
            _cancellationTokenSource.Cancel();
            
            try
            {
                // Ensure final flush
                FlushBufferAsync().Wait(TimeSpan.FromSeconds(2));
                _processQueueTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Ignore shutdown errors
            }

            _logQueue.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }

    private enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    private class LogEntry
    {
        public DateTime Timestamp { get; init; }
        public LogLevel Level { get; init; }
        public string Message { get; init; } = string.Empty;
        public int ThreadId { get; init; }
        public int ProcessId { get; init; }
    }
} 