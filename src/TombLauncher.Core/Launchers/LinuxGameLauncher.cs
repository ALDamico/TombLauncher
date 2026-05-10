using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Launches a Linux executable directly (no compatibility layer needed - Linux host OS).
/// </summary>
public class LinuxGameLauncher : IGameLauncher
{
    public ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context)
    {
        return new ProcessStartInfo(Path.Combine(context.WorkingDirectory, context.ExecutableFileName))
        {
            Arguments = context.Arguments,
            WorkingDirectory = context.WorkingDirectory,
            UseShellExecute = false,
        };
    }
}