using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Launches a Windows executable directly (no compatibility layer needed — Windows host OS).
/// </summary>
public class WindowsGameLauncher : IGameLauncher
{
    public ProcessStartInfo GetLaunchStartInfo(
        string executableFileNameOnly,
        string arguments,
        string workingDirectory,
        string? prefixPath = null)
    {
        return new ProcessStartInfo(executableFileNameOnly)
        {
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true,
        };
    }
}
