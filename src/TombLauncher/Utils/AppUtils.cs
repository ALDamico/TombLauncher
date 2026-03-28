using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.XPath;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using IconPacks.Avalonia.RemixIcon;
using LiveChartsCore;
using Microsoft.Extensions.DependencyInjection;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Services;
using AngleSharpConfig = AngleSharp.Configuration;

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

    public static async Task<IDocument> OpenDocumentFromContent(string content, CancellationToken cancellationToken)
    {
        var config = AngleSharpConfig.Default.WithXPath().WithDefaultLoader();
        var browsingContext = BrowsingContext.New(config);
        return await browsingContext.OpenAsync(req => req.Content(content), cancellationToken);
    }

    public static async Task<IDocument> OpenDocument(string url, CancellationToken cancellationToken)
    {
        var config = AngleSharpConfig.Default.WithXPath().WithDefaultLoader();
        var browsingContext = BrowsingContext.New(config);
        return await browsingContext.OpenAsync(url, cancellationToken);
    }

    public static INode? SelectSingleNodeFromElement(this INode? node, string xpath, bool ignoreNamespaces = true)
    {
        return (node as IElement)?.SelectSingleNode(xpath, ignoreNamespaces);
    }

    public static List<INode> SelectNodesFromElement(this INode? node, string xpath, bool ignoreNamespaces = true)
    {
        return (node as IElement)?.SelectNodes(xpath, ignoreNamespaces) ?? [];
    }

    public static INamedNodeMap? GetAttributes(this INode? node)
    {
        return (node as IElement)?.Attributes;
    }

    public static bool HasClass(this INode? node, string className)
    {
        return (node as IElement)?.ClassList.Contains(className) ?? false;
    }

    public static string? GetAttributeValue(this INode? node, string attrName)
    {
        return node.GetAttributes()?[attrName]?.Value;
    }

    public static string? GetInnerHtml(this INode? node)
    {
        return (node as IElement)?.InnerHtml;
    }

    public static string GetInnerHtmlOrEmpty(this INode? node)
    {
        return node.GetInnerHtml() ?? "";
    }

    public static async Task CheckCompatibilityToolAsync()
    {
        var platform = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        if (!platform.IsWineSupported) return;

        var appConfig = Ioc.Default.GetRequiredService<ILayeredAppConfiguration>();
        var notifications = Ioc.Default.GetRequiredService<NotificationService>();

        var tool = appConfig.Compatibility.CompatibilityTool;

        if (tool == CompatibilityTool.Proton)
        {
            var protonInstallations = platform.FindAvailableProtonInstallations();
            if (protonInstallations.Count == 0 && string.IsNullOrWhiteSpace(appConfig.Compatibility.ProtonPath))
            {
                await notifications.AddWarningNotificationAsync(
                    "PROTON_NOT_FOUND", "PROTON_NOT_FOUND_DESCRIPTION",
                    PackIconRemixIconKind.GobletBrokenLine);
            }
        }
        else
        {
            // Wine (default)
            var mergedWinePath = appConfig.Compatibility.WinePath;
            var wineExe = platform.FindWineExecutable();
            if (wineExe != null)
            {
                if (mergedWinePath != wineExe)
                {
                    appConfig.User.Compatibility.WinePath = wineExe;
                    await PersistAsync();
                }
            }
            else
            {
                await notifications.AddWarningNotificationAsync(
                    "WINE_NOT_FOUND", "WINE_NOT_FOUND_DESCRIPTION",
                    PackIconRemixIconKind.GobletBrokenLine);
            }
        }

        static async Task PersistAsync()
        {
            using var scope = Ioc.Default.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<SettingsPageService>();
            await settingsService.PersistCurrentConfigAsync();
        }
    }
    
    public static void ApplyInitialSettings()
    {
        var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        var themeManager = Ioc.Default.GetRequiredService<ThemeManager>();

        var applicationLanguage = settingsProvider.GetApplicationSettings().ApplicationLanguage;
        localizationManager.ChangeLanguage(applicationLanguage);

        var applicationTheme = settingsProvider.GetAppearanceSettings().ApplicationTheme;
        themeManager.ApplyTheme(applicationTheme);

        var baseVariant = ThemeVariant.Dark;
        if (!string.IsNullOrEmpty(applicationTheme) && applicationTheme.Contains("Light"))
        {
            baseVariant = ThemeVariant.Light;
        }
        ChangeTheme(baseVariant);
    }
}