using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Launches a Windows executable directly (no compatibility layer needed — Windows host OS).
/// </summary>
public class WindowsGameLauncher : IGameLauncher
{
    public ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context)
    {
        return new ProcessStartInfo(context.ExecutableFileName)
        {
            Arguments = context.Arguments,
            WorkingDirectory = context.WorkingDirectory,
            UseShellExecute = true,
        };
    }
}
