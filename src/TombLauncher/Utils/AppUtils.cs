using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView;
using TombLauncher.Core.PlatformSpecific;
using AngleSharpConfig = AngleSharp.Configuration;

namespace TombLauncher.Utils;

public static class AppUtils
{
    public static string[] LogFileNamePatterns => ["LAST_CRASH*", "TENLog*.txt", "TR1X.log", "TR2X.log"];
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

    public static string[] GetLogFiles(string directory, DateTime startDate)
    {
        return LogFileNamePatterns.SelectMany(p => Directory.GetFiles(directory, p,
                new EnumerationOptions()
                {
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true,
                    AttributesToSkip = FileAttributes.ReparsePoint,
                    ReturnSpecialDirectories = false
                }).Where(f => File.GetLastWriteTime(f) > startDate))
            .ToArray();
    }
}