using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

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

    public static void ChangeTheme(ThemeVariant themeVariant)
    {
        Application.Current.RequestedThemeVariant = themeVariant;
        if (themeVariant == ThemeVariant.Dark)
        {
            LiveCharts.Configure(config => config.AddDarkTheme());
        }
        else
        {
            LiveCharts.Configure(config => config.AddLightTheme());
        }
    }

    public static Version GetApplicationVersion() => Assembly.GetEntryAssembly()?.GetName().Version;

    public static Version GetDotNetVersion() => Environment.Version;
}