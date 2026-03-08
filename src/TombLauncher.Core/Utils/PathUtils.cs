namespace TombLauncher.Core.Utils;

public class PathUtils
{
    public static string GetRandomTempDirectory()
    {
        var tempPath = GetTombLauncherTempDirectory();
        var dirname = Path.GetRandomFileName();
        var fullPath = Path.Combine(tempPath, dirname);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    public static string GetTombLauncherTempDirectory()
    {
        var tempPath = Path.GetTempPath();
        var fullPath = Path.Combine(tempPath, "TombLauncher");
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }

    public static string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }

    public static string GetGamesFolder(string appDataDirectory)
    {
        return Path.Combine(appDataDirectory, "Data", "Games");
    }

    public static string GetGameInstallFolder(string appDataDirectory, Guid gameGuid)
    {
        return Path.Combine(GetGamesFolder(appDataDirectory), gameGuid.ToString());
    }

    public static bool DirectoryContainsFile(string path, string file)
    {
        return Directory.GetFiles(path, file, SearchOption.AllDirectories).Length > 0;
    }

    public static string? GetRelativePath(string containingFolder, string fileName)
    {
        var file = Directory.GetFiles(containingFolder, fileName, SearchOption.AllDirectories);
        if (file.Length == 0)
            return null;
        return Path.GetRelativePath(containingFolder, file[0]);
    }

    public static char[] GetWindowsInvalidFileNameChars(bool includeDirectorySeparators = false)
    {
        char[] baseChars =
        [
            '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t',
            '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015',
            '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':',
            '*', '?'
        ];

        return includeDirectorySeparators
            ? [..baseChars, '\\', '/']
            : baseChars;
    }

    public static long GetDirectorySize(string directory)
    {
        return new DirectoryInfo(directory)
            .EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Sum(f => f.Length);
    }
}