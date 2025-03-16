using System.Diagnostics;

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
}