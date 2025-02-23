using System.Text;
using System.Windows;
using System.Windows.Automation;
using WindowInspector.App.Services;

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
            Logger.Instance.Warning($"No element available at ({screenPoint.X}, {screenPoint.Y})");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error getting element at point: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if an element is still valid and accessible
    /// </summary>
    /// <param name="element">The element to check</param>
    /// <returns>True if the element is still valid, false if it's stale or inaccessible</returns>
    public static bool IsElementValid(AutomationElement? element)
    {
        if (element == null) return false;

        try
        {
            // Try to access some basic properties that should always be available
            // This will throw if the element is stale
            var dummy = element.Current.BoundingRectangle;
            var dummy2 = element.Current.ProcessId;
            return true;
        }
        catch (ElementNotAvailableException)
        {
            Logger.Instance.Warning("Element is no longer available");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error validating element: {ex.Message}");
            return false;
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
        if (!IsElementValid(element)) return "Element no longer available";

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
        catch (ElementNotAvailableException)
        {
            Logger.Instance.Warning("Element became unavailable while getting description");
            return "Element no longer available";
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error getting element description: {ex.Message}");
            return "Error reading element properties";
        }
    }

    /// <summary>
    /// Gets a list of parent elements from the current element up to the root, handling stale elements
    /// </summary>
    /// <param name="element">The starting element</param>
    /// <returns>List of elements from child to root</returns>
    private static List<AutomationElement> GetParentHierarchy(AutomationElement element)
    {
        var hierarchy = new List<AutomationElement>();
        try
        {
            if (!IsElementValid(element))
            {
                Logger.Instance.Warning("Cannot get hierarchy for invalid element");
                return hierarchy;
            }

            hierarchy.Add(element);
            var current = element;

            while (true)
            {
                AutomationElement? parent;
                try
                {
                    parent = TreeWalker.RawViewWalker.GetParent(current);
                }
                catch (ElementNotAvailableException)
                {
                    Logger.Instance.Warning("Parent element became unavailable");
                    break;
                }

                if (parent == null || parent == AutomationElement.RootElement || !IsElementValid(parent))
                {
                    break;
                }

                hierarchy.Add(parent);
                current = parent;
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error getting parent hierarchy: {ex.Message}");
        }
        return hierarchy;
    }

    /// <summary>
    /// Safely tries to get an element's property with stale element handling
    /// </summary>
    private static T GetElementProperty<T>(AutomationElement element, Func<AutomationElement.AutomationElementInformation, T> propertyAccessor, T defaultValue)
    {
        try
        {
            if (!IsElementValid(element))
            {
                return defaultValue;
            }
            return propertyAccessor(element.Current);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Builds a detailed string containing all relevant element properties
    /// </summary>
    /// <param name="element">The element to describe</param>
    /// <returns>A formatted string with detailed element information</returns>
    public static string BuildDetailedElementString(AutomationElement? element)
    {
        if (element == null) return "No element found under cursor";
        if (!IsElementValid(element)) return "Element is no longer available (the window or control may have been closed or changed)";

        try
        {
            var sb = new StringBuilder();
            var current = element.Current;

            // Section: Basic Information
            sb.AppendLine("Basic Information");
            sb.AppendLine("----------------");
            sb.AppendLine($"Control Type: {GetElementProperty(element, e => e.ControlType.ProgrammaticName.Replace("ControlType.", ""), "[Unknown]")}");
            sb.AppendLine($"Name: {GetElementProperty(element, e => e.Name, "[No name]")}");
            sb.AppendLine($"Class Name: {GetElementProperty(element, e => e.ClassName, "[No class]")}");
            sb.AppendLine($"Automation ID: {GetElementProperty(element, e => e.AutomationId, "[No ID]")}");
            sb.AppendLine();

            // Section: Location & Size
            sb.AppendLine("Location & Size");
            sb.AppendLine("--------------");
            var rect = GetElementProperty(element, e => e.BoundingRectangle, Rect.Empty);
            if (rect != Rect.Empty)
            {
                sb.AppendLine($"Position: ({rect.X:F0}, {rect.Y:F0})");
                sb.AppendLine($"Size: {rect.Width:F0} × {rect.Height:F0}");
            }
            else
            {
                sb.AppendLine("Position: Not available");
                sb.AppendLine("Size: Not available");
            }
            sb.AppendLine($"Is Offscreen: {GetElementProperty(element, e => e.IsOffscreen, false)}");
            sb.AppendLine();

            // Section: Element Hierarchy
            sb.AppendLine("Element Hierarchy");
            sb.AppendLine("-----------------");
            var hierarchy = GetParentHierarchy(element);
            if (hierarchy.Count > 0)
            {
                for (int i = 0; i < hierarchy.Count; i++)
                {
                    var indent = new string(' ', i * 2);
                    var elem = hierarchy[i];
                    if (IsElementValid(elem))
                    {
                        var type = GetElementProperty(elem, e => e.ControlType.ProgrammaticName.Replace("ControlType.", ""), "[Unknown]");
                        var name = GetElementProperty(elem, e => e.Name, "[No name]");
                        var className = GetElementProperty(elem, e => e.ClassName, "[No class]");
                        sb.AppendLine($"{indent}↳ {type}: {name} ({className})");
                    }
                    else
                    {
                        sb.AppendLine($"{indent}↳ [Element no longer available]");
                    }
                }
            }
            else
            {
                sb.AppendLine("[Hierarchy unavailable]");
            }
            sb.AppendLine();

            // Section: Element State
            sb.AppendLine("Element State");
            sb.AppendLine("-------------");
            sb.AppendLine($"Is Enabled: {GetElementProperty(element, e => e.IsEnabled, false)}");
            sb.AppendLine($"Is Keyboard Focusable: {GetElementProperty(element, e => e.IsKeyboardFocusable, false)}");
            sb.AppendLine($"Has Keyboard Focus: {GetElementProperty(element, e => e.HasKeyboardFocus, false)}");
            sb.AppendLine($"Framework: {GetElementProperty(element, e => e.FrameworkId, "[Unknown]")}");
            
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
                var processId = GetElementProperty(element, e => e.ProcessId, -1);
                if (processId > 0)
                {
                    var process = System.Diagnostics.Process.GetProcessById(processId);
                    sb.AppendLine();
                    sb.AppendLine("Process Information");
                    sb.AppendLine("------------------");
                    sb.AppendLine($"Process Name: {process.ProcessName}");
                    sb.AppendLine($"Process ID: {processId}");
                    sb.AppendLine($"Window Handle: 0x{GetElementProperty(element, e => e.NativeWindowHandle, 0):X8}");
                }
            }
            catch
            {
                // Process info not available
            }

            return sb.ToString();
        }
        catch (ElementNotAvailableException)
        {
            return "Element is no longer available (the window or control may have been closed or changed)";
        }
        catch (Exception ex)
        {
            return $"Error building element details: {ex.Message}";
        }
    }

    public static bool AreElementsEqual(AutomationElement? a, AutomationElement? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        try
        {
            // Compare key properties that identify an element
            return a.Current.ProcessId == b.Current.ProcessId &&
                   a.Current.BoundingRectangle == b.Current.BoundingRectangle &&
                   a.Current.Name == b.Current.Name &&
                   a.Current.ClassName == b.Current.ClassName &&
                   a.Current.ControlType == b.Current.ControlType;
        }
        catch (Exception ex)
        {
            Logger.Instance.Warning($"Error comparing elements: {ex.Message}");
            return false; // If we can't compare properties (e.g., element became stale), consider them different
        }
    }

    private static List<string> GetSupportedPatterns(AutomationElement element)
    {
        var patterns = new List<string>();
        try
        {
            if (!IsElementValid(element))
            {
                return patterns;
            }

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
                try
                {
                    if (element.GetSupportedPatterns().Contains(patternId))
                    {
                        patterns.Add(patternId.ProgrammaticName.Replace("Pattern", ""));
                    }
                }
                catch (ElementNotAvailableException)
                {
                    Logger.Instance.Warning("Element became unavailable during pattern check");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Warning($"Error checking pattern {patternId.ProgrammaticName}: {ex.Message}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error($"Error getting supported patterns: {ex.Message}");
        }
        return patterns;
    }
} 