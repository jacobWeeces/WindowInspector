using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using WindowInspector.App.Commands;
using WindowInspector.App.Helpers;

namespace WindowInspector.App.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherTimer _cursorTrackingTimer;
    private string _statusText = "Ready to inspect elements...";
    private string _hierarchyText = "Hover over a UI element to inspect it...";
    private Point _cursorPosition;
    private DateTime _lastConsoleUpdate = DateTime.MinValue;
    private AutomationElement? _currentElement;
    private RelayCommand? _copyToClipboardCommand;

    public MainWindowViewModel()
    {
        _cursorTrackingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // Update 10 times per second
        };
        _cursorTrackingTimer.Tick += CursorTrackingTimer_Tick;
        _cursorTrackingTimer.Start();

        Console.WriteLine("Window Inspector started. Tracking cursor position...");
    }

    public IRelayCommand CopyToClipboardCommand => _copyToClipboardCommand ??= new RelayCommand(
        execute: _ =>
        {
            try
            {
                Clipboard.SetText(HierarchyText);
                var originalStatus = StatusText;
                StatusText = "Copied to clipboard!";
                
                // Reset status after 2 seconds
                var timer = new DispatcherTimer
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

    public Point CursorPosition
    {
        get => _cursorPosition;
        private set
        {
            if (_cursorPosition != value)
            {
                _cursorPosition = value;
                OnPropertyChanged();
                UpdateElement();
            }
        }
    }

    private void CursorTrackingTimer_Tick(object? sender, EventArgs e)
    {
        CursorPosition = Win32Helpers.GetCursorPosition();
    }

    private void UpdateElement()
    {
        _currentElement = ElementTracker.GetElementAt(CursorPosition);
        UpdateStatusText();
        UpdateHierarchyText();
        UpdateConsoleOutput();
    }

    private void UpdateStatusText()
    {
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

        var sb = new StringBuilder();
        sb.AppendLine("Element Details:");
        sb.AppendLine("----------------");
        
        // Add basic properties
        sb.AppendLine($"Type: {_currentElement.Current.ControlType.ProgrammaticName.Replace("ControlType.", "")}");
        sb.AppendLine($"Name: {_currentElement.Current.Name}");
        sb.AppendLine($"Class: {_currentElement.Current.ClassName}");
        sb.AppendLine($"AutomationId: {_currentElement.Current.AutomationId}");
        
        // Add bounds information
        var rect = _currentElement.Current.BoundingRectangle;
        sb.AppendLine($"Bounds: X={rect.X}, Y={rect.Y}, Width={rect.Width}, Height={rect.Height}");
        
        // Add process info if available
        try
        {
            var processId = _currentElement.Current.ProcessId;
            var process = System.Diagnostics.Process.GetProcessById(processId);
            sb.AppendLine($"Process: {process.ProcessName} (ID: {processId})");
        }
        catch
        {
            sb.AppendLine("Process: Unknown");
        }

        HierarchyText = sb.ToString();
    }

    private void UpdateConsoleOutput()
    {
        // Only update console once per second to avoid flooding
        var now = DateTime.Now;
        if ((now - _lastConsoleUpdate).TotalSeconds >= 1)
        {
            var elementDesc = ElementTracker.GetElementDescription(_currentElement);
            Console.WriteLine($"[{now:HH:mm:ss}] Cursor at: X={CursorPosition.X}, Y={CursorPosition.Y}");
            Console.WriteLine($"Element: {elementDesc}");
            _lastConsoleUpdate = now;
        }
    }

    public void Dispose()
    {
        Console.WriteLine("Window Inspector stopping...");
        _cursorTrackingTimer.Stop();
        _cursorTrackingTimer.Tick -= CursorTrackingTimer_Tick;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
} 