using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

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

    public static Bitmap NullBitmap
    {
        get
        {
            if (App.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                return new Bitmap(
                    AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm-light.png")));
            }

            return new Bitmap(AssetLoader.Open(new Uri("avares://TombLauncher/Assets/unknown-title-pic-sm.png")));
        }
    }
}