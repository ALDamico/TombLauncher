using System.Security.Cryptography;

namespace TombLauncher.Core.Utils;

public static class Md5Utils
{
    public static async Task<string> ComputeMd5Hash(Stream stream)
    {
        var md5 = MD5.Create();
        var md5Hash = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(md5Hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    public static async Task<string> ComputeMd5Hash(byte[] bytes)
    {
        using var memoryStream = new MemoryStream(bytes);
        return await ComputeMd5Hash(memoryStream);
    }
}