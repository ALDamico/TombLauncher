using System.Diagnostics;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.PlatformSpecific;

public class WindowsPlatformSpecificFeatures : IPlatformSpecificFeatures
{
    public void OpenGameFolder(string gameFolder)
    {
        Process.Start("explorer", gameFolder);
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

    public ProcessStartInfo GetGameLaunchStartInfo(string executableFileNameOnly, string arguments,
        string compatibilityExecutable, string workingDirectory)
    {
        return new ProcessStartInfo(executableFileNameOnly)
        {
            Arguments = arguments ?? "",
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
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
}