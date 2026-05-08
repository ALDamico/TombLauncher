using System.Reflection;

namespace TombLauncher.Core.Utils;

public class VersionUtils
{
    public static Version? GetApplicationVersion() => Assembly.GetEntryAssembly()?.GetName().Version;
    public static Version GetDotNetVersion() => Environment.Version;
}