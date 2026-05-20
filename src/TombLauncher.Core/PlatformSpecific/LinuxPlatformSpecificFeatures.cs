using System.Diagnostics;
using System.Text.RegularExpressions;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Settings;
using TombLauncher.Contracts.SupportMatrix;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific.SupportMatrix;
using TombLauncher.Core.Utils;

namespace TombLauncher.Core.PlatformSpecific;

public class LinuxPlatformSpecificFeatures : IPlatformSpecificFeatures
{
    public Platform Platform => Platform.Linux;

    public void OpenFolder(string folder)
    {
        Process.Start("xdg-open", folder);
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
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
    }

    public NotifyFilters GetSavegameWatcherNotifyFilters()
    {
        return NotifyFilters.LastWrite | NotifyFilters.FileName;
    }

    public List<UnzipBackendDto> GetPlatformSpecificZipFallbackPrograms()
    {
        return
        [
            new UnzipBackendDto() { Name = "tar", Command = "tar", CommandLineArguments = @"-xf ""{0}"" -C ""{1}""" },
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

    public string ExpandPath(string path) =>
        path.StartsWith('~')
            ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + path[1..]
            : path;

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

    public string? GetWineVersion(string winePath)
    {
        if (string.IsNullOrWhiteSpace(winePath))
            return null;
        try
        {
            var psi = new ProcessStartInfo(winePath, "--version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return null;
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrWhiteSpace(output) ? null : output;
        }
        catch
        {
            return null;
        }
    }

    public List<ProtonInstallationDto> FindAvailableProtonInstallations()
    {
        var steamRoots = CollectSteamRoots();
        var results = new List<ProtonInstallationDto>();

        foreach (var steamRoot in steamRoots)
        {
            var commonDir = Path.Combine(steamRoot, "steamapps", "common");
            if (!Directory.Exists(commonDir)) continue;

            foreach (var dir in Directory.EnumerateDirectories(commonDir, "Proton*"))
            {
                var protonBin = Path.Combine(dir, "proton");
                if (!File.Exists(protonBin)) continue;

                var displayName = Path.GetFileName(dir); // e.g. "Proton 9.0"
                results.Add(new ProtonInstallationDto(displayName, protonBin));
            }
        }

        // Sort descending so newest version appears first
        results.Sort((a, b) => string.Compare(b.DisplayName, a.DisplayName, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    /// <summary>
    /// Collects all Steam library roots, starting with the default one then
    /// parsing libraryfolders.vdf for additional libraries.
    /// </summary>
    private static List<string> CollectSteamRoots()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var defaultRoot = Path.Combine(home, ".steam", "steam");
        var roots = new List<string> { defaultRoot };

        var vdf = Path.Combine(defaultRoot, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdf)) return roots;

        // Simple regex-based parse: look for lines like: "path"  "/some/path"
        var pathPattern = new Regex(@"""path""	+""(.+?)""", RegexOptions.Compiled);
        foreach (var line in File.ReadLines(vdf))
        {
            var m = pathPattern.Match(line);
            if (m.Success)
            {
                var extra = m.Groups[1].Value;
                if (!roots.Contains(extra))
                    roots.Add(extra);
            }
        }

        // Let's filter entries that are symbolic links
        return roots
            .Where(Directory.Exists)
            .Select(dir => new DirectoryInfo(dir).ResolveLinkTarget(true)?.FullName ?? dir)
            .Distinct()
            .ToList();
    }

    public string? GetProtonVersion(string protonPath)
    {
        if (string.IsNullOrWhiteSpace(protonPath)) return null;

        // Proton stores its version in a file called "version" next to the binary
        var versionFile = Path.Combine(Path.GetDirectoryName(protonPath) ?? "", "version");
        if (!File.Exists(versionFile)) return null;

        try
        {
            return File.ReadAllText(versionFile).Trim();
        }
        catch
        {
            return null;
        }
    }

    public ISupportMatrix SupportMatrix { get; } = new LinuxSupportMatrix();

    public bool IsExecutable(string path)
    {
        var fullPath = PathUtils.GetFullPath(path);
        if (fullPath == null) return false;

#pragma warning disable CA1416
        var fileMode = File.GetUnixFileMode(path);
        return fileMode.HasFlag(UnixFileMode.GroupExecute) ||
               fileMode.HasFlag(UnixFileMode.UserExecute) ||
               fileMode.HasFlag(UnixFileMode.OtherExecute);
#pragma warning restore CA1416
    }
}