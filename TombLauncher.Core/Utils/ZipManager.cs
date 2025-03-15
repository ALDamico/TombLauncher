using System.Collections;
using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Core.Utils;

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

    public async Task ExtractAll(string targetPath, CancellationToken cancellationToken = default, IProgress<CopyProgressInfo> progress = null)
    {
        PathUtils.EnsureFolderExists(targetPath);
        var zipFileEnumerator = _zipFile.GetEnumerator();
        using var zipFileEnumerator1 = zipFileEnumerator as IDisposable;
        var runningSize = 0L;

        while (zipFileEnumerator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = (ZipEntry)zipFileEnumerator.Current;
            runningSize++;

            if (current.IsDirectory)
            {
                PathUtils.EnsureFolderExists(Path.Combine(targetPath, current.Name));
                continue;
            }

            var relativePath = Path.GetDirectoryName((string)current.Name);
            if (relativePath.IsNotNullOrWhiteSpace())
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

            while ((size = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await streamWriter.WriteAsync(buffer, 0, size, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        ((IDisposable)_zipFile)?.Dispose();
        _zipFileEnumerator = null;
    }
}