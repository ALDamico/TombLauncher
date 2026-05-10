using TombLauncher.Core.Utils;

namespace TombLauncher.Patchers.Trx.Patchers;

public class TrxNativeExecutablePatcher
{
    public string BuildTag(string internalName, Version version)
    {
        var tag = $"{internalName}-{version.Major}.{version.Minor}";
        if (version.Build > -1)
        {
            tag += $".{version.Build}";
        }

        return tag;
    }

    public async Task<byte[]> ExtractExecutable(string fullFilePath, CancellationToken ct)
    {
        using var zipManager = new ZipManager(fullFilePath);
        var entry = zipManager.GetEntries()
            .Where(e => e?.IsDirectory == false)
            .Where(e => e != null)
            .FirstOrDefault(e => e!.Name.Contains("TRX") || e.Name.Contains("TR1X") || e.Name.Contains("TR2X"));
        if (entry == null)
            return [];
        await using var inputStream = zipManager.GetInputStream(entry);
        await using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream, ct);
        return memoryStream.ToArray();
    }
}