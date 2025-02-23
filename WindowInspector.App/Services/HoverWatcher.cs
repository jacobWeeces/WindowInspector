using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;
using WindowInspector.App.Helpers;

namespace WindowInspector.App.Services;

public class HoverWatcher : IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly Dispatcher _dispatcher;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(50);
    private bool _isPaused;
    private DateTime _lastElementUpdate = DateTime.MinValue;
    private Point _lastCursorPos;
    private AutomationElement? _lastElement;

    public event EventHandler<AutomationElement?>? ElementChanged;
    public event EventHandler<Point>? CursorMoved;
    public event EventHandler<Exception>? ErrorOccurred;

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (_isPaused != value)
            {
                _isPaused = value;
                if (_isPaused)
                {
                    _timer.Stop();
                    Logger.Instance.Info("Element tracking paused");
                }
                else
                {
                    _timer.Start();
                    Logger.Instance.Info("Element tracking resumed");
                    // Force an immediate update when resuming
                    CheckElementUnderCursor();
                }
            }
        }
    }

    public HoverWatcher(TimeSpan? updateInterval = null)
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        _timer = new DispatcherTimer
        {
            Interval = updateInterval ?? TimeSpan.FromMilliseconds(100) // Default to 10 times per second
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Logger.Instance.Info($"HoverWatcher started with {_timer.Interval.TotalMilliseconds}ms interval");
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (!IsPaused)
        {
            CheckElementUnderCursor();
        }
    }

    private void CheckElementUnderCursor()
    {
        try
        {
            var cursorPos = Win32Helpers.GetCursorPosition();
            var now = DateTime.Now;

            // Only update cursor position if it has changed
            if (cursorPos != _lastCursorPos)
            {
                _lastCursorPos = cursorPos;
                RaiseEventOnUIThread(() => CursorMoved?.Invoke(this, cursorPos));
            }

            // Debounce element updates to avoid flickering
            if ((now - _lastElementUpdate) >= _debounceInterval)
            {
                var element = ElementTracker.GetElementAt(cursorPos);
                
                // Only raise event if element has changed
                if (!ElementTracker.AreElementsEqual(element, _lastElement))
                {
                    _lastElement = element;
                    _lastElementUpdate = now;

                    var desc = ElementTracker.GetElementDescription(element);
                    Logger.Instance.Info($"Element changed: {desc} at ({cursorPos.X}, {cursorPos.Y})");

                    RaiseEventOnUIThread(() => ElementChanged?.Invoke(this, element));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(ex);
            RaiseEventOnUIThread(() => ErrorOccurred?.Invoke(this, ex));
        }
    }

    private void RaiseEventOnUIThread(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.BeginInvoke(action);
        }
    }

    public void Dispose()
    {
        Logger.Instance.Info("HoverWatcher stopping...");
        _timer.Stop();
        _timer.Tick -= Timer_Tick;
    }
} 