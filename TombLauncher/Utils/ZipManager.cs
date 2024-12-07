using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Progress;

namespace TombLauncher.Utils;

public class ZipManager : IDisposable
{
    public ZipManager(string path, StringCodec stringCodec = null)
    {
        stringCodec ??= StringCodec.Default;
        _zipFile = new ZipFile(path, stringCodec);
    }

    private ZipFile _zipFile;
    private IEnumerator _zipFileEnumerator;

    public IEnumerable<ZipEntry> GetEntries()
    {
        if (_zipFileEnumerator == null)
            _zipFileEnumerator = _zipFile.GetEnumerator();
        while (_zipFileEnumerator.MoveNext())
        {
            yield return (ZipEntry)_zipFileEnumerator.Current;
        }
    }

    public Stream GetInputStream(ZipEntry file)
    {
        return _zipFile.GetInputStream(file);
    }

    public async Task ExtractAll(string targetPath, IProgress<CopyProgressInfo> progress = null)
    {
        PathUtils.EnsureFolderExists(targetPath);
        var zipFileEnumerator = _zipFile.GetEnumerator();
        using var zipFileEnumerator1 = zipFileEnumerator as IDisposable;
        var runningSize = 0L;

        while (zipFileEnumerator.MoveNext())
        {
            var current = (ZipEntry)zipFileEnumerator.Current;
            runningSize++;

            if (current.IsDirectory)
            {
                PathUtils.EnsureFolderExists(Path.Combine(targetPath, current.Name));
                continue;
            }

            var relativePath = Path.GetDirectoryName(current.Name);
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                PathUtils.EnsureFolderExists(Path.Combine(targetPath, relativePath));
            }

            if (progress != null)
            {
                var copyProgress = new CopyProgressInfo()
                {
                    CurrentFileName = current.Name,
                    TotalFiles = _zipFile.Count,
                    CurrentFile = runningSize
                };
                progress.Report(copyProgress);
            }

            var inputStream = _zipFile.GetInputStream(current);
            var targetFileName = Path.Combine(targetPath, current.Name);
            await using var streamWriter = File.Create(targetFileName);
            var size = 4096;
            var buffer = new byte[size];

            while ((size = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                streamWriter.Write(buffer, 0, size);
            }
        }
    }

    public void Dispose()
    {
        ((IDisposable)_zipFile)?.Dispose();
        _zipFileEnumerator = null;
    }
}