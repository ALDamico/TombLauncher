using System.Diagnostics;

namespace TombLauncher.Core.PlatformSpecific;

public class LinuxPlatformSpecificFeatures : IPlatformSpecificFeatures
{
    public void OpenGameFolder(string gameFolder)
    {
        Process.Start("xdg-open", gameFolder);
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
            MatchCasing = MatchCasing.CaseInsensitive,
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false
        };
    }

    public ProcessStartInfo GetGameLaunchStartInfo(string executableFileNameOnly, string arguments, string compatibilityExecutable,
        string workingDirectory)
    {
        arguments = executableFileNameOnly + " " + (arguments ?? "");
        return new ProcessStartInfo(compatibilityExecutable)
        {
            Arguments = arguments ?? "",
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
        };
    }
}