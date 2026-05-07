using System.Security.Cryptography;

namespace TombLauncher.Core.Utils;

public static class CryptoUtils
{
    public static async Task<string> ComputeMd5Hash(Stream stream)
    {
        var hash = await MD5.HashDataAsync(stream);
        return Convert.ToHexStringLower(hash);
    }

    public static string ComputeMd5Hash(byte[] bytes)
    {
        var hash = MD5.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    public static async Task<string> ComputeSha256Hash(Stream stream, CancellationToken cancellationToken = default)
    {
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hash);
    }
}
