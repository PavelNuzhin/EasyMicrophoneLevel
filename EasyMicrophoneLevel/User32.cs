using System.Drawing;
using System.Runtime.InteropServices;

public static partial class User32
{
    private const string user32dllName = "user32.dll";

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;

        public Rectangle ToRectangle()
        {
            return new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }
    }

    [DllImport(user32dllName, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, String className, String windowTitle);

    [DllImport(user32dllName)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport(user32dllName)]
    private static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref RECT rect, [MarshalAs(UnmanagedType.U4)] int cPoints);

    public static Rectangle GetClientRect(IntPtr hWnd)
    {
        GetClientRect(hWnd, out RECT rectangle);
        MapWindowPoints(hWnd, IntPtr.Zero, ref rectangle, 2);

        var netRectangle = rectangle.ToRectangle();

        return netRectangle;
    }
}
