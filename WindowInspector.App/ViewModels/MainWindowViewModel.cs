using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using WindowInspector.App.Commands;
using WindowInspector.App.Helpers;
using WindowInspector.App.Services;

namespace WindowInspector.App.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IHoverWatcher _hoverWatcher;
    private string _statusText = "Ready to inspect elements...";
    private string _hierarchyText = "Hover over a UI element to inspect it...";
    private Point _cursorPosition;
    private DateTime _lastConsoleUpdate = DateTime.MinValue;
    private AutomationElement? _currentElement;
    private RelayCommand? _copyToClipboardCommand;
    private RelayCommand? _togglePauseCommand;
    private bool _isPaused;

    public MainWindowViewModel()
    {
        _hoverWatcher = new HoverWatcher();
        _hoverWatcher.ElementHovered += OnElementHovered;
        _hoverWatcher.Start();

        Console.WriteLine("Window Inspector started. Tracking cursor position...");
    }

    private void OnElementHovered(object? sender, HoverWatcherEventArgs e)
    {
        if (!IsPaused)
        {
            _cursorPosition = e.CursorPosition;
            _currentElement = e.Element;
            
            UpdateStatusText();
            UpdateHierarchyText();
            UpdateConsoleOutput();
        }
    }

    public bool IsPaused
    {
        get => _isPaused;
        private set
        {
            if (_isPaused != value)
            {
                _isPaused = value;
                OnPropertyChanged();
                
                if (_isPaused)
                {
                    _hoverWatcher.Stop();
                    StatusText = "Tracking paused. Click Resume or press Space to continue tracking.";
                }
                else
                {
                    _hoverWatcher.Start();
                    // Element will update automatically when tracking resumes
                }
            }
        }
    }

    public IRelayCommand TogglePauseCommand => _togglePauseCommand ??= new RelayCommand(
        execute: _ =>
        {
            IsPaused = !IsPaused;
        }
    );

    public IRelayCommand CopyToClipboardCommand => _copyToClipboardCommand ??= new RelayCommand(
        execute: _ =>
        {
            try
            {
                Clipboard.SetText(HierarchyText);
                var originalStatus = StatusText;
                StatusText = "Copied to clipboard!";
                
                // Reset status after 2 seconds
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, e) =>
                {
                    StatusText = originalStatus;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                StatusText = "Failed to copy to clipboard: " + ex.Message;
            }
        },
        canExecute: _ => !string.IsNullOrWhiteSpace(HierarchyText) && HierarchyText != "No element found under cursor"
    );

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
    }

    public string HierarchyText
    {
        get => _hierarchyText;
        set
        {
            if (_hierarchyText != value)
            {
                _hierarchyText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Update copy button state
            }
        }
    }

    private void UpdateStatusText()
    {
        if (IsPaused)
        {
            return; // Keep the "paused" message
        }

        var elementDesc = ElementTracker.GetElementDescription(_currentElement);
        StatusText = $"Cursor at: X={_cursorPosition.X}, Y={_cursorPosition.Y} | {elementDesc}";
    }

    private void UpdateHierarchyText()
    {
        if (_currentElement == null)
        {
            HierarchyText = "No element found under cursor";
            return;
        }

        HierarchyText = ElementTracker.BuildDetailedElementString(_currentElement);
    }

    private void UpdateConsoleOutput()
    {
        // Only update console once per second to avoid flooding
        var now = DateTime.Now;
        if ((now - _lastConsoleUpdate).TotalSeconds >= 1)
        {
            var elementDesc = ElementTracker.GetElementDescription(_currentElement);
            Console.WriteLine($"[{now:HH:mm:ss}] Cursor at: X={_cursorPosition.X}, Y={_cursorPosition.Y}");
            Console.WriteLine($"Element: {elementDesc}");
            _lastConsoleUpdate = now;
        }
    }

    public void Dispose()
    {
        Console.WriteLine("Window Inspector stopping...");
        _hoverWatcher.ElementHovered -= OnElementHovered;
        _hoverWatcher.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 