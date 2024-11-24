using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Progress;

namespace TombLauncher.Utils;

public static class ZipUtils
{
    public static void ExtractZip(string zipPath, string targetPath, IProgress<CopyProgressInfo> progress = null)
    {
        var zipFile = new ZipFile(zipPath);

        var zipFileEnumerator = zipFile.GetEnumerator();
        var runningSize = 0L;
        
        while (zipFileEnumerator.Current != null)
        {
            var current = (ZipEntry)zipFileEnumerator.Current;
            runningSize++;
            if (progress != null)
            {
                var copyProgress = new CopyProgressInfo()
                {
                    CurrentFileName = current.Name,
                    TotalFiles = zipFile.Count,
                    CurrentFile = runningSize
                };
                progress?.Report(copyProgress);
            }

            var inputStream = zipFile.GetInputStream(current);
            var targetFileName = Path.Combine(targetPath, current.Name);
            using (var streamWriter = File.Create(targetFileName))
            {
                int size = 4096;
                byte[] buffer = new byte[size];

                while ((size = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    streamWriter.Write(buffer, 0, size);
                }
            }

            zipFileEnumerator.MoveNext();
        }

        ((IDisposable)zipFileEnumerator).Dispose();
    }
}