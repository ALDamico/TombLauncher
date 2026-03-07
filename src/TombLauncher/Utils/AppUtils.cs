using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Utils;

public static class AppUtils
{
    public static IClipboard GetClipboard()
    {
        var applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        return applicationLifetime?.MainWindow?.Clipboard!;
    }

    public static Task SetClipboardTextAsync(string text)
    {
        var clipboard = GetClipboard();
        return clipboard.SetTextAsync(text);
    }

    public static void ChangeTheme(ThemeVariant themeVariant)
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = themeVariant;
        }
        var applyTheme = themeVariant == ThemeVariant.Dark
            ? (Action<LiveChartsSettings>)(config => config.AddDarkTheme())
            : config => config.AddLightTheme();
        LiveCharts.Configure(applyTheme);
    }

    public static Version? GetApplicationVersion() => Assembly.GetEntryAssembly()?.GetName().Version;

    public static Version GetDotNetVersion() => Environment.Version;

    public static IPlatformSpecificFeatures InitPlatformSpecificFeatures()
    {
        IPlatformSpecificFeatures platformSpecificFeatures;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            platformSpecificFeatures = new WindowsPlatformSpecificFeatures();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            platformSpecificFeatures = new LinuxPlatformSpecificFeatures();
        }
        else
        {
            throw new PlatformNotSupportedException("This platform is not supported.");
        }

        return platformSpecificFeatures;
    }
}