using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TombLauncher.Utils;

public class PathUtils
{
    public static string GetRandomTempDirectory()
    {
        var tempPath = Path.GetTempPath();
        var dirname = Path.GetRandomFileName();
        var fullPath = Path.Combine(tempPath, "TombLauncher", dirname);
        EnsureFolderExists(fullPath);
        return fullPath;
    }

    public static void EnsureFolderExists([NotNull]string path)
    {
        if (!Path.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }
}