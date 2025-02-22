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
            var controlType = element.Current.ControlType.ProgrammaticName.Replace("ControlType.", "");
            var name = string.IsNullOrEmpty(element.Current.Name) ? "[No name]" : element.Current.Name;
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

            return $"{controlType}: {name} ({className})";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting element description: {ex.Message}");
            return "Error reading element properties";
        }
    }
} 