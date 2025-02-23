using System.Text;
using System.Windows;
using System.Windows.Automation;

namespace WindowInspector.App.Helpers;

public static class ElementTracker
{
    // Known desktop-related window classes
    private static readonly HashSet<string> DesktopClasses = new()
    {
        "Progman",      // Main desktop window
        "WorkerW",      // Desktop worker window
        "FolderView",   // Desktop icons container
        "SysListView32" // Desktop icons list view
    };

    /// <summary>
    /// Gets the UI Automation element at the specified screen coordinates
    /// </summary>
    /// <param name="screenPoint">The screen coordinates to check</param>
    /// <returns>The AutomationElement at the specified point, or null if none found</returns>
    public static AutomationElement? GetElementAt(Point screenPoint)
    {
        try
        {
            return AutomationElement.FromPoint(new System.Windows.Point(screenPoint.X, screenPoint.Y));
        }
        catch (ElementNotAvailableException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting element at point: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets a basic description of the automation element
    /// </summary>
    /// <param name="element">The element to describe</param>
    /// <returns>A string describing the element's basic properties</returns>
    public static string GetElementDescription(AutomationElement? element)
    {
        if (element == null) return "No element found";

        try
        {
            var className = string.IsNullOrEmpty(element.Current.ClassName) ? "[No class]" : element.Current.ClassName;

            // Enhanced desktop detection
            if (DesktopClasses.Contains(className))
            {
                // If it's a desktop icon, show that specifically
                if (className == "SysListView32")
                {
                    return "Desktop (Icons View)";
                }
                return "Desktop";
            }

            var controlType = element.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
            var name = string.IsNullOrEmpty(element.Current.Name) ? "[No name]" : element.Current.Name;

            return $"{controlType}: {name} ({className})";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting element description: {ex.Message}");
            return "Error reading element properties";
        }
    }

    /// <summary>
    /// Gets a list of parent elements from the current element up to the root
    /// </summary>
    /// <param name="element">The starting element</param>
    /// <returns>List of elements from child to root</returns>
    private static List<AutomationElement> GetParentHierarchy(AutomationElement element)
    {
        var hierarchy = new List<AutomationElement> { element };
        try
        {
            var current = element;
            while (true)
            {
                var parent = TreeWalker.RawViewWalker.GetParent(current);
                if (parent == null || parent == AutomationElement.RootElement)
                {
                    break;
                }
                hierarchy.Add(parent);
                current = parent;
            }
        }
        catch
        {
            // Stop hierarchy collection on any error
        }
        return hierarchy;
    }

    /// <summary>
    /// Builds a detailed string containing all relevant element properties
    /// </summary>
    /// <param name="element">The element to describe</param>
    /// <returns>A formatted string with detailed element information</returns>
    public static string BuildDetailedElementString(AutomationElement? element)
    {
        if (element == null) return "No element found under cursor";

        try
        {
            var sb = new StringBuilder();
            var current = element.Current;

            // Section: Basic Information
            sb.AppendLine("Basic Information");
            sb.AppendLine("----------------");
            sb.AppendLine($"Control Type: {current.ControlType.ProgrammaticName.Replace("ControlType.", "")}");
            sb.AppendLine($"Name: {current.Name}");
            sb.AppendLine($"Class Name: {current.ClassName}");
            sb.AppendLine($"Automation ID: {current.AutomationId}");
            sb.AppendLine();

            // Section: Location & Size
            sb.AppendLine("Location & Size");
            sb.AppendLine("--------------");
            var rect = current.BoundingRectangle;
            sb.AppendLine($"Position: ({rect.X:F0}, {rect.Y:F0})");
            sb.AppendLine($"Size: {rect.Width:F0} × {rect.Height:F0}");
            sb.AppendLine($"Is Offscreen: {current.IsOffscreen}");
            sb.AppendLine();

            // Section: Element Hierarchy
            sb.AppendLine("Element Hierarchy");
            sb.AppendLine("-----------------");
            var hierarchy = GetParentHierarchy(element);
            for (int i = 0; i < hierarchy.Count; i++)
            {
                var indent = new string(' ', i * 2);
                var elem = hierarchy[i];
                var type = elem.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
                var name = string.IsNullOrEmpty(elem.Current.Name) ? "[No name]" : elem.Current.Name;
                var className = string.IsNullOrEmpty(elem.Current.ClassName) ? "[No class]" : elem.Current.ClassName;
                
                sb.AppendLine($"{indent}↳ {type}: {name} ({className})");
            }
            sb.AppendLine();

            // Section: Element State
            sb.AppendLine("Element State");
            sb.AppendLine("-------------");
            sb.AppendLine($"Is Enabled: {current.IsEnabled}");
            sb.AppendLine($"Is Keyboard Focusable: {current.IsKeyboardFocusable}");
            sb.AppendLine($"Has Keyboard Focus: {current.HasKeyboardFocus}");
            sb.AppendLine($"Framework: {current.FrameworkId}");
            
            // Section: Patterns
            var supportedPatterns = GetSupportedPatterns(element);
            if (supportedPatterns.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Supported Patterns");
                sb.AppendLine("-----------------");
                foreach (var pattern in supportedPatterns.OrderBy(p => p))
                {
                    sb.AppendLine($"• {pattern}");
                }
            }

            // Section: Process Information
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(current.ProcessId);
                sb.AppendLine();
                sb.AppendLine("Process Information");
                sb.AppendLine("------------------");
                sb.AppendLine($"Process Name: {process.ProcessName}");
                sb.AppendLine($"Process ID: {current.ProcessId}");
                sb.AppendLine($"Window Handle: 0x{current.NativeWindowHandle:X8}");
            }
            catch
            {
                // Process info not available
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error building element details: {ex.Message}";
        }
    }

    private static List<string> GetSupportedPatterns(AutomationElement element)
    {
        var patterns = new List<string>();
        try
        {
            var patternIds = new[]
            {
                InvokePattern.Pattern,
                SelectionPattern.Pattern,
                ValuePattern.Pattern,
                RangeValuePattern.Pattern,
                ScrollPattern.Pattern,
                ExpandCollapsePattern.Pattern,
                GridPattern.Pattern,
                GridItemPattern.Pattern,
                MultipleViewPattern.Pattern,
                WindowPattern.Pattern,
                SelectionItemPattern.Pattern,
                DockPattern.Pattern,
                TextPattern.Pattern,
                TablePattern.Pattern,
                TableItemPattern.Pattern,
                TogglePattern.Pattern,
                TransformPattern.Pattern,
                ScrollItemPattern.Pattern,
                ItemContainerPattern.Pattern,
                VirtualizedItemPattern.Pattern,
                SynchronizedInputPattern.Pattern
            };

            foreach (var patternId in patternIds)
            {
                if (element.GetSupportedPatterns().Contains(patternId))
                {
                    patterns.Add(patternId.ProgrammaticName.Replace("Pattern", ""));
                }
            }
        }
        catch
        {
            // Pattern detection failed
        }
        return patterns;
    }
} 