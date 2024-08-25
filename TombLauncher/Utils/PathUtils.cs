using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TombLauncher.Utils;

public class PathUtils
{
    public static string GetRandomTempDirectory()
    {
        var tempPath = Path.GetTempPath();
        var dirname = Path.GetRandomFileName();
        var fullPath = Path.Combine(tempPath, dirname);
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
}