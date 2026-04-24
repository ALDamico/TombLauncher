using Avalonia.Controls;

namespace TombLauncher.Utils;

internal static class WindowUtils
{
    internal static WindowState ToggleWindowState(WindowState currentState)
    {
        return currentState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
    }
}