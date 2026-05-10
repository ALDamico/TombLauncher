using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Core.Utils;

public class VersionUtils
{
    public static Version? GetApplicationVersion() => Assembly.GetEntryAssembly()?.GetName().Version;
    public static Version GetDotNetVersion() => Environment.Version;

    public static string? ReadFileVersion(string filePath)
    {
        var data = File.ReadAllBytes(filePath);
        // Search for the UTF-16LE key "FileVersion\0" in the PE StringFileInfo
        var key = Encoding.Unicode.GetBytes("FileVersion\0");
        var idx = IndexOf(data, key);
        if (idx < 0)
            return null;

        // Skip key + padding to DWORD boundary
        var valueStart = idx + key.Length;
        while (valueStart % 4 != 0)
            valueStart++;

        // Read the UTF-16LE value until null terminator
        var sb = new StringBuilder();
        for (var i = valueStart; i < data.Length - 1; i += 2)
        {
            var c = (char)(data[i] | (data[i + 1] << 8));
            if (c == '\0') break;
            sb.Append(c);
        }

        var result = sb.ToString();
        return result.Length > 0 ? result : null;
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] == needle[j]) continue;
                match = false;
                break;
            }
            if (match) return i;
        }
        return -1;
    }

    public static TrxVersionInfo ReadTrxVersionInfo(string filePath)
    {
        var fileVersion = ReadFileVersion(filePath) ?? "";
        var regex = new Regex(@"^(?<InternalName>TR[12]?X)\s(?<Version>\d+\.\d+\.\d+)");

        var matches = regex.Match(fileVersion);
        if (!Version.TryParse(matches.Groups["Version"].Value, out var version))
        {
            throw new InvalidOperationException("Can't parse input string!");
        }

        var internalName = matches.Groups["InternalName"].Value.ToLowerInvariant();
        if (internalName.Equals("tr1x", StringComparison.InvariantCultureIgnoreCase) ||
            internalName.Equals("tr2x", StringComparison.InvariantCultureIgnoreCase))
        {
            internalName = internalName[..^1];
        }

        var executableName = internalName switch
        {
            "tr1" => "TR1X",
            "tr2" => "TR2X",
            _ => "TRX"
        };

        return new TrxVersionInfo()
        {
            InternalName = internalName,
            ExecutableName = executableName,
            Version = version
        };
    }
}