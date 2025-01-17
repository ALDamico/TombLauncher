﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TombLauncher.Core.Utils;

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

    public static string GetGamesFolder()
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", "Games");
    }
}