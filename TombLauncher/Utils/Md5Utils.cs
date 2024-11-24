using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TombLauncher.Utils;

public static class Md5Utils
{
    public async static Task<string> ComputeMd5Hash(Stream stream)
    {
        var md5 = MD5.Create();
        var md5Hash = await md5.ComputeHashAsync(stream);
        return BitConverter.ToString(md5Hash).Replace("-", string.Empty).ToLowerInvariant();
    }
}