using System.Diagnostics;
using TombLauncher.Core.Dtos;

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

    public ProcessStartInfo GetGameLaunchStartInfo(string executableFileNameOnly, string arguments,
        string compatibilityExecutable, string workingDirectory, string? winePrefix = null)
    {
        arguments = executableFileNameOnly + " " + (arguments ?? "");
        var psi = new ProcessStartInfo(compatibilityExecutable)
        {
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };
        if (!string.IsNullOrWhiteSpace(winePrefix))
            psi.Environment["WINEPREFIX"] = winePrefix;
        return psi;
    }

    public NotifyFilters GetSavegameWatcherNotifyFilters()
    {
        return NotifyFilters.LastWrite | NotifyFilters.FileName;
    }

    public List<UnzipBackendDto> GetPlatformSpecificZipFallbackPrograms()
    {
        return
        [
            new UnzipBackendDto() { Name = "tar", Command  = "tar", CommandLineArguments = @"-xf ""{0}"" -C ""{1}""" },
            new UnzipBackendDto() { Name = "unzip", Command = "unzip", CommandLineArguments = @"""{0}"" -d ""{1}""" }
        ];
    }

    public string GetAppDataDirectory()
    {
        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        var appDir = Path.Combine(configHome, "TombLauncher");
        if (!Directory.Exists(appDir))
        {
            Directory.CreateDirectory(appDir);
        }
        return appDir;
    }

    public bool IsWineSupported => true;

    public string? FindWineExecutable()
    {
        // 1. Check WINE env var (points directly to the wine binary)
        var wineEnv = Environment.GetEnvironmentVariable("WINE");
        if (!string.IsNullOrWhiteSpace(wineEnv) && File.Exists(wineEnv))
            return wineEnv;

        // 2. Check WINEPATH env var (explicitly set on some systems)
        var winePathEnv = Environment.GetEnvironmentVariable("WINEPATH");
        if (!string.IsNullOrWhiteSpace(winePathEnv) && File.Exists(winePathEnv))
            return winePathEnv;

        // 3. Fall back to PATH scan
        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? [];
        return pathDirs
            .Select(dir => Path.Combine(dir, "wine"))
            .FirstOrDefault(File.Exists);
    }
}