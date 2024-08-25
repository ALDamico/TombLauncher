using System;
using Ionic.Zip;
using TombLauncher.Progress;

namespace TombLauncher.Utils;

public static class ZipUtils
{
    public static void ExtractZip(string zipPath, string targetPath, IProgress<CopyProgressInfo> progress = null)
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
}