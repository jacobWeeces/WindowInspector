using System.Windows;
using System.Windows.Automation;
using WindowInspector.App.Helpers;

namespace WindowInspector.App.Services;

public class HoverWatcherEventArgs : EventArgs
{
    public Point CursorPosition { get; }
    public AutomationElement? Element { get; }

    public HoverWatcherEventArgs(Point cursorPosition, AutomationElement? element)
    {
        CursorPosition = cursorPosition;
        Element = element;
    }
}

public interface IHoverWatcher : IDisposable
{
    bool IsWatching { get; }
    void Start();
    void Stop();
    event EventHandler<HoverWatcherEventArgs>? ElementHovered;
}

public class HoverWatcher : IHoverWatcher
{
    private readonly System.Timers.Timer _timer;
    private Point _lastPosition;
    private AutomationElement? _lastElement;
    private bool _isWatching;

    public bool IsWatching => _isWatching;

    public event EventHandler<HoverWatcherEventArgs>? ElementHovered;

    public HoverWatcher()
    {
        _timer = new System.Timers.Timer
        {
            Interval = 100 // 100ms interval as specified
        };
        _timer.Elapsed += Timer_Elapsed;
        _lastPosition = new Point(-1, -1);
    }

    public void Start()
    {
        if (!_isWatching)
        {
            _isWatching = true;
            _timer.Start();
        }
    }

    public void Stop()
    {
        if (_isWatching)
        {
            _isWatching = false;
            _timer.Stop();
        }
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            var currentPosition = Win32Helpers.GetCursorPosition();

            // Only update if the cursor has moved
            if (currentPosition != _lastPosition)
            {
                _lastPosition = currentPosition;
                
                var element = ElementTracker.GetElementAt(currentPosition);
                
                // Only raise event if element has changed or position has changed
                if (element != _lastElement || currentPosition != _lastPosition)
                {
                    _lastElement = element;
                    
                    // Raise event on UI thread
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ElementHovered?.Invoke(this, new HoverWatcherEventArgs(currentPosition, element));
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            Console.WriteLine($"Error in HoverWatcher: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
        _timer.Elapsed -= Timer_Elapsed;
        _timer.Dispose();
    }
} 