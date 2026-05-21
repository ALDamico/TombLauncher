using System.Diagnostics;
using System.Reflection;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Settings;
using TombLauncher.Contracts.SupportMatrix;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific.SupportMatrix;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.PlatformSpecific;

public class WindowsPlatformSpecificFeatures : IPlatformSpecificFeatures
{
    public Platform Platform => Platform.Windows;

    public void OpenFolder(string folder)
    {
        Process.Start("explorer", folder);
    }

    public void OpenUrl(string link)
    {
        link = link.Replace("&", "^&");
        Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
    }

    public EnumerationOptions GetEnumerationOptions()
    {
        return new EnumerationOptions()
        {
            MatchCasing = MatchCasing.PlatformDefault,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false
        };
    }

    public NotifyFilters GetSavegameWatcherNotifyFilters()
    {
        return NotifyFilters.LastWrite;
    }

    public List<UnzipBackendDto> GetPlatformSpecificZipFallbackPrograms()
    {
        return
        [
            new UnzipBackendDto() { Name = "tar", Command = "tar", CommandLineArguments = @"-xf ""{0}"" -C ""{1}""" },
            new UnzipBackendDto() { Name = "7-zip", Command = "7z", CommandLineArguments = @"x ""{0}"" -o""{1}""" }
        ];
    }

    public string GetAppDataDirectory()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }

    public string ExpandPath(string path) => path;

    public bool IsWineSupported => false;
    public string? FindWineExecutable() => null;
    public string? GetWineVersion(string winePath) => null;
    public List<ProtonInstallationDto> FindAvailableProtonInstallations() => [];
    public string? GetProtonVersion(string protonPath) => null;
    public ISupportMatrix SupportMatrix { get; } = new WindowsSupportMatrix();
    public bool IsExecutable(string path)
    {
        return PathUtils.GetFullPath(path)?.ToLowerInvariant().EndsWith(".exe") == true;
    }
}