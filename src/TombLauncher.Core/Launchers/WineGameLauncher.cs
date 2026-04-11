using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Launches a Windows executable via Wine on Linux.
/// Wraps the command with bash and appends "wineserver -w" so that the monitored
/// process stays alive until the game truly exits (Wine itself exits immediately
/// after handing off to wineserver).
/// </summary>
public class WineGameLauncher : IGameLauncher
{
    private readonly string _winePath;

    public WineGameLauncher(string winePath)
    {
        _winePath = winePath;
    }

    public ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context)
    {
        var gameArgs = string.IsNullOrWhiteSpace(context.Arguments) ? string.Empty : " " + context.Arguments;
        var wineCommand = $"\"{_winePath}\" \"{context.ExecutableFileName}\"{gameArgs}";

        var psi = new ProcessStartInfo("bash")
        {
            WorkingDirectory = context.WorkingDirectory,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add($"{wineCommand}; wineserver -w");

        if (!string.IsNullOrWhiteSpace(context.PrefixPath))
            psi.Environment["WINEPREFIX"] = context.PrefixPath;

        // Per-game env var overrides applied last — they can override WINEPREFIX and any of the above.
        if (context.ExtraEnvVars != null)
            foreach (var (k, v) in context.ExtraEnvVars)
                psi.Environment[k] = v;

        return psi;
    }
}
