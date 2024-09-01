using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace TombLauncher.Utils;

public static class AppUtils
{
    public static IClipboard GetClipboard()
    {
        var applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return applicationLifetime?.MainWindow?.Clipboard;
    }

    public static Task SetClipboardTextAsync(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard == null) return Task.CompletedTask;
        return clipboard.SetTextAsync(text);
    }
}