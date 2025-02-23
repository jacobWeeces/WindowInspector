using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;

namespace WindowInspector.App.Helpers;

/// <summary>
/// Provides secure access to Windows API functions
/// </summary>
[SuppressUnmanagedCodeSecurity] // Optimization for internal trusted calls
public static class Win32Helpers
{
    #region Constants and Enums

    private const int ERROR_SUCCESS = 0;

    [Flags]
    private enum WindowStyles : uint
    {
        WS_VISIBLE = 0x10000000,
        WS_DISABLED = 0x08000000
    }

    [Flags]
    private enum WindowStylesEx : uint
    {
        WS_EX_TOOLWINDOW = 0x00000080,
        WS_EX_TOPMOST = 0x00000008
    }

    #endregion

    #region Structures

    /// <summary>
    /// Win32 POINT structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Win32Point
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Win32 RECT structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    #region Win32 API Declarations

    /// <summary>
    /// Gets the cursor position in screen coordinates
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode,
               EntryPoint = "GetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out Win32Point lpPoint);

    /// <summary>
    /// Gets the window at the specified point
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode,
               EntryPoint = "WindowFromPoint")]
    private static extern IntPtr WindowFromPoint(Win32Point point);

    /// <summary>
    /// Gets information about the specified window's styles
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode,
               EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    /// <summary>
    /// Gets the window's rectangle in screen coordinates
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode,
               EntryPoint = "GetWindowRect")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    /// <summary>
    /// Gets the class name of a window
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode,
               EntryPoint = "GetClassNameW")]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the current cursor position in screen coordinates
    /// </summary>
    /// <returns>A Point containing the X,Y coordinates of the cursor, or (-1,-1) if an error occurs</returns>
    /// <exception cref="Win32Exception">Thrown when the Windows API call fails</exception>
    public static Point GetCursorPosition()
    {
        // Clear last error
        SetLastError(ERROR_SUCCESS);

        if (!GetCursorPos(out Win32Point point))
        {
            var error = Marshal.GetLastWin32Error();
            if (error != ERROR_SUCCESS)
            {
                throw new Win32Exception(error, "Failed to get cursor position");
            }
            return new Point(-1, -1);
        }

        return new Point(point.X, point.Y);
    }

    /// <summary>
    /// Gets information about the window at the current cursor position
    /// </summary>
    /// <returns>A tuple containing window info, or null values if no window found</returns>
    public static (string ClassName, bool IsVisible, bool IsDisabled, Rect Bounds) GetWindowInfo(Point position)
    {
        try
        {
            // Convert to Win32 point
            var point = new Win32Point { X = (int)position.X, Y = (int)position.Y };

            // Get window handle
            var hwnd = WindowFromPoint(point);
            if (hwnd == IntPtr.Zero)
            {
                return (string.Empty, false, false, Rect.Empty);
            }

            // Get window styles
            var style = (WindowStyles)GetWindowLong(hwnd, -16); // GWL_STYLE
            var exStyle = (WindowStylesEx)GetWindowLong(hwnd, -20); // GWL_EXSTYLE

            // Get window rect
            if (!GetWindowRect(hwnd, out RECT rect))
            {
                rect = new RECT();
            }

            // Get class name
            var className = new StringBuilder(256);
            GetClassName(hwnd, className, className.Capacity);

            return (
                className.ToString(),
                style.HasFlag(WindowStyles.WS_VISIBLE),
                style.HasFlag(WindowStyles.WS_DISABLED),
                new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top)
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting window info: {ex.Message}");
            return (string.Empty, false, false, Rect.Empty);
        }
    }

    #endregion

    #region Private Helper Methods

    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern void SetLastError(int dwErrorCode);

    /// <summary>
    /// Safely gets a window style value
    /// </summary>
    private static int GetWindowStyleSafely(IntPtr hwnd, int index, int defaultValue = 0)
    {
        try
        {
            var value = GetWindowLong(hwnd, index);
            return Marshal.GetLastWin32Error() == ERROR_SUCCESS ? value : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    #endregion
} 