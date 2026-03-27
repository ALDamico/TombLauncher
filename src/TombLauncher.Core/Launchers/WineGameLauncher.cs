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

    public ProcessStartInfo GetLaunchStartInfo(
        string executableFileNameOnly,
        string arguments,
        string workingDirectory,
        string? prefixPath = null)
    {
        var gameArgs = string.IsNullOrWhiteSpace(arguments) ? string.Empty : " " + arguments;
        var wineCommand = $"\"{_winePath}\" \"{executableFileNameOnly}\"{gameArgs}";

        var psi = new ProcessStartInfo("bash")
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add($"{wineCommand}; wineserver -w");

        if (!string.IsNullOrWhiteSpace(prefixPath))
            psi.Environment["WINEPREFIX"] = prefixPath;

        return psi;
    }
}
