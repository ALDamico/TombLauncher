using System.Security.Cryptography;

namespace TombLauncher.Core.Utils;

public static class Md5Utils
{
    public static async Task<string> ComputeMd5Hash(Stream stream)
    {
        var md5Hash = await MD5.HashDataAsync(stream);
        return Convert.ToHexStringLower(md5Hash);
    }

    public static string ComputeMd5Hash(byte[] bytes)
    {
        var md5Hash = MD5.HashData(bytes);
        return Convert.ToHexStringLower(md5Hash);
    }
}