using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using WindowInspector.App.Commands;
using WindowInspector.App.Helpers;
using WindowInspector.App.Services;

namespace WindowInspector.App.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly HoverWatcher _hoverWatcher;
    private readonly DispatcherTimer _statusResetTimer;
    private string _statusText = "Ready to inspect elements...";
    private string _hierarchyText = "Hover over a UI element to inspect it...";
    private Point _cursorPosition;
    private AutomationElement? _currentElement;
    private RelayCommand? _copyToClipboardCommand;
    private RelayCommand? _togglePauseCommand;
    private bool _isUpdating;

    public MainWindowViewModel()
    {
        _hoverWatcher = new HoverWatcher();
        _hoverWatcher.CursorMoved += HoverWatcher_CursorMoved;
        _hoverWatcher.ElementChanged += HoverWatcher_ElementChanged;
        _hoverWatcher.ErrorOccurred += HoverWatcher_ErrorOccurred;

        _statusResetTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _statusResetTimer.Tick += (s, e) =>
        {
            _statusResetTimer.Stop();
            UpdateStatusText(); // Revert to normal status
        };

        Console.WriteLine("Window Inspector started. Tracking cursor position...");
    }

    public bool IsUpdating
    {
        get => _isUpdating;
        private set
        {
            if (_isUpdating != value)
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsPaused
    {
        get => _hoverWatcher.IsPaused;
        private set
        {
            if (_hoverWatcher.IsPaused != value)
            {
                _hoverWatcher.IsPaused = value;
                OnPropertyChanged();
                
                if (value)
                {
                    StatusText = "Tracking paused. Click Resume to continue tracking.";
                }
                else
                {
                    StatusText = "Resuming element tracking...";
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
            if (IsUpdating)
            {
                ShowTemporaryStatus("Please wait for element information to finish updating...", isError: true);
                return;
            }

            var textToCopy = HierarchyText;
            if (string.IsNullOrWhiteSpace(textToCopy))
            {
                ShowTemporaryStatus("Nothing to copy - text is empty", isError: true);
                return;
            }

            try
            {
                // Ensure we're on the UI thread
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() => Clipboard.SetText(textToCopy));
                }
                else
                {
                    Clipboard.SetText(textToCopy);
                }
                ShowTemporaryStatus("Copied to clipboard!");
            }
            catch (Exception ex)
            {
                ShowTemporaryStatus($"Failed to copy to clipboard: {ex.Message}", isError: true);
            }
        },
        canExecute: _ => !string.IsNullOrWhiteSpace(HierarchyText) && 
                        HierarchyText != "No element found under cursor" &&
                        !IsUpdating
    );

    public string StatusText
    {
        get => _statusText;
        private set
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
        private set
        {
            if (_hierarchyText != value)
            {
                _hierarchyText = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested(); // Update copy button state
            }
        }
    }

    public Point CursorPosition
    {
        get => _cursorPosition;
        private set
        {
            if (_cursorPosition != value)
            {
                _cursorPosition = value;
                OnPropertyChanged();
            }
        }
    }

    private void HoverWatcher_CursorMoved(object? sender, Point cursorPos)
    {
        CursorPosition = cursorPos;
    }

    private void HoverWatcher_ElementChanged(object? sender, AutomationElement? element)
    {
        IsUpdating = true;
        _currentElement = element;
        UpdateStatusText();
        UpdateHierarchyText();
        IsUpdating = false;
    }

    private void HoverWatcher_ErrorOccurred(object? sender, Exception ex)
    {
        ShowTemporaryStatus($"Error: {ex.Message}", isError: true);
    }

    private void ShowTemporaryStatus(string message, bool isError = false)
    {
        _statusResetTimer.Stop();
        StatusText = isError ? $"⚠️ {message}" : message;
        _statusResetTimer.Start();
    }

    private void UpdateStatusText()
    {
        if (IsPaused)
        {
            return; // Keep the "paused" message
        }

        var elementDesc = ElementTracker.GetElementDescription(_currentElement);
        StatusText = $"Cursor at: X={CursorPosition.X}, Y={CursorPosition.Y} | {elementDesc}";
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

    public void Dispose()
    {
        Console.WriteLine("Window Inspector stopping...");
        _statusResetTimer.Stop();
        _hoverWatcher.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 