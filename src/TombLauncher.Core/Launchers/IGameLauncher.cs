using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Abstracts the OS/tool-specific logic needed to launch a Windows game executable.
/// Implementations: <see cref="WindowsGameLauncher"/>, <see cref="WineGameLauncher"/>, <see cref="ProtonGameLauncher"/>.
/// </summary>
public interface IGameLauncher
{
    /// <summary>
    /// Builds the <see cref="ProcessStartInfo"/> required to launch the game.
    /// </summary>
    /// <param name="executableFileNameOnly">Just the file name of the exe (no directory).</param>
    /// <param name="arguments">Extra command-line arguments to pass to the exe.</param>
    /// <param name="workingDirectory">The directory the process will run from.</param>
    /// <param name="prefixPath">
    /// Optional per-game isolation path. For Wine this is WINEPREFIX;
    /// for Proton this is STEAM_COMPAT_DATA_PATH.
    /// </param>
    ProcessStartInfo GetLaunchStartInfo(
        string executableFileNameOnly,
        string arguments,
        string workingDirectory,
        string? prefixPath = null);
}
