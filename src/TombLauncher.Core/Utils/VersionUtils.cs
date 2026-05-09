using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.Core.Utils;

public class VersionUtils
{
    public static Version? GetApplicationVersion() => Assembly.GetEntryAssembly()?.GetName().Version;
    public static Version GetDotNetVersion() => Environment.Version;

    public static string? ReadFileVersion(string filePath) => FileVersionInfo.GetVersionInfo(filePath).FileVersion;

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