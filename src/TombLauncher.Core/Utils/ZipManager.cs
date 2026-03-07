using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Core.Utils;

public class ZipManager : IDisposable
{
    public ZipManager(string path, StringCodec? stringCodec = null)
    {
        stringCodec ??= StringCodec.Default;
        _zipFile = new ZipFile(path, stringCodec);
    }

    private readonly ZipFile _zipFile;

    public IEnumerable<ZipEntry> GetEntries()
    {
        var enumerator = _zipFile.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                yield return (ZipEntry)enumerator.Current;
            }
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    public Stream GetInputStream(ZipEntry file)
    {
        return _zipFile.GetInputStream(file);
    }

    public async Task ExtractAll(string targetPath, CancellationToken cancellationToken = default, IProgress<CopyProgressInfo>? progress = null)
    {
        Directory.CreateDirectory(targetPath);
        var enumerator = _zipFile.GetEnumerator();
        using var enumeratorDisposable = enumerator as IDisposable;
        var currentEntry = 0L;
        var buffer = new byte[4096 * 16];

        while (enumerator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = (ZipEntry)enumerator.Current;
            currentEntry++;

            if (current.IsDirectory)
            {
                Directory.CreateDirectory(Path.Combine(targetPath, current.Name));
                continue;
            }

            var relativePath = Path.GetDirectoryName(current.Name);
            if (relativePath.IsNotNullOrWhiteSpace())
            {
                Directory.CreateDirectory(Path.Combine(targetPath, relativePath!));
            }

            if (progress != null)
            {
                progress.Report(new CopyProgressInfo
                {
                    CurrentFileName = current.Name,
                    TotalFiles = _zipFile.Count,
                    CurrentFile = currentEntry
                });
            }

            await using var inputStream = _zipFile.GetInputStream(current);
            var targetFileName = Path.Combine(targetPath, current.Name);
            await using var outputStream = File.Create(targetFileName);
            int bytesRead;

            while ((bytesRead = await inputStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await outputStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        ((IDisposable)_zipFile)?.Dispose();
    }
}