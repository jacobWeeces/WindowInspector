using System.Runtime.InteropServices;
using System.Windows;

namespace WindowInspector.App.Helpers;

public static class Win32Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Win32Point
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out Win32Point lpPoint);

    /// <summary>
    /// Gets the current cursor position in screen coordinates
    /// </summary>
    /// <returns>A Point containing the X,Y coordinates of the cursor</returns>
    public static Point GetCursorPosition()
    {
        if (GetCursorPos(out Win32Point point))
        {
            return new Point(point.X, point.Y);
        }

        // In case of failure, return (-1, -1) to indicate an error
        return new Point(-1, -1);
    }
} 