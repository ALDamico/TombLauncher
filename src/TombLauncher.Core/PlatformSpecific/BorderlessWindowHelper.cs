using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TombLauncher.Core.PlatformSpecific;

public static class BorderlessWindowHelper
{
    public static void Apply(Process process, int width, int height)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        Task.Run(() => PollWindow(process, width, height));
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
    
    private const int MsCheck = 100;
    private const int MaxIterations = 100;

    private const int GwlStyle = -16;

    private const nint WsCaptions   = 0x00C00000;
    private const nint WsThickframe = 0x00040000;
    private const nint WsMinimizebox = 0x00020000;
    private const nint WsMaximizebox = 0x00010000;
    private const nint WsSysmenu    = 0x00080000;

    private const uint SwpFramechanged = 0x0020;
    private const uint SwpShowwindow   = 0x0040;
    private const uint SwpNozorder     = 0x0004;
    private const uint SwpNoactivate   = 0x0010;
    private static async Task PollWindow(Process process, int width, int height)
    {
        try
        {
            for (var i = 0; i < MaxIterations; i++)
            {
                var windowHandle = process.MainWindowHandle;
                if (windowHandle != 0)
                {
                    var style = GetWindowLongPtr(windowHandle, GwlStyle);
                    if (style == 0 && Marshal.GetLastWin32Error() == 5) return; // ERROR_ACCESS_DENIED
                    SetWindowLongPtr(windowHandle, GwlStyle,
                        style & ~(WsCaptions | WsThickframe | WsMinimizebox | WsMaximizebox | WsSysmenu));
                    SetWindowPos(windowHandle, 0, 0, 0, width, height,
                        SwpFramechanged | SwpShowwindow | SwpNozorder | SwpNoactivate);
                    return;
                }

                await Task.Delay(MsCheck);
            }
        }
        catch (InvalidOperationException)
        {
            // Process already exited. Ignore
        }
    }
}