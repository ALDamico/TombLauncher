using System;
using System.IO;
using System.Reflection;
using Ionic.Zip;
using TombLauncher.Dto;
using TombLauncher.Progress;

namespace TombLauncher.Installers;

public class TombRaiderLevelInstaller
{
    public void Install(string containingFolder, GameMetadataDto gameDto,
        IProgress<CopyProgressInfo> copyProgress = null)
    {
        if (!Directory.Exists(containingFolder) && !File.Exists(containingFolder))
        {
            throw new ArgumentException("The source folder does not exist!", nameof(containingFolder));
        }

        var gameTitleNormalized = string.Empty;
        if (gameDto.Author != null)
        {
            gameTitleNormalized = $"{gameDto.Author} - ";
        }

        gameTitleNormalized += gameDto.Title;

        gameTitleNormalized = MakeValidFileName(gameTitleNormalized);
        var installFolder =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "Games", gameTitleNormalized);
        
        if (Directory.Exists(containingFolder))
        {

            CopyDirectory(containingFolder, installFolder, true);
        }
        else if (File.Exists(containingFolder) && ZipFile.IsZipFile(containingFolder))
        {
            ExtractZip(containingFolder, installFolder, copyProgress);
        }
    }

    private void ExtractZip(string zipPath, string targetPath, IProgress<CopyProgressInfo> progress = null)
    {
        var zipFile = new ZipFile(zipPath);
        if (progress != null)
        {
            zipFile.ExtractProgress += (sender, args) =>
            {
                var copyProgress = new CopyProgressInfo()
                {
                    CurrentFileName = args.CurrentEntry?.FileName,
                    TotalFiles = args.EntriesTotal,
                    CurrentFile = args.EntriesExtracted
                };
                progress?.Report(copyProgress);
            };
        }
        zipFile.ExtractAll(targetPath, ExtractExistingFileAction.OverwriteSilently);
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars =
            System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, IProgress<CopyProgressInfo> progress = null)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);
        
        if (progress != null)
        {
            var copyProgressInfo = new CopyProgressInfo()
            {
                CurrentFileName = dir.Name
            };
            progress.Report(copyProgressInfo);
        }

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            if (progress != null)
            {
                var copyProgressInfo = new CopyProgressInfo()
                {
                    CurrentFileName = Path.Combine(dir.FullName.Replace(destinationDir, string.Empty), file.Name)
                };
                progress.Report(copyProgressInfo);
            }
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (!recursive) return;
        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}