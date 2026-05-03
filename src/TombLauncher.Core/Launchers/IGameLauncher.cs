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
    /// <param name="context">All launch parameters encapsulated in a single object.</param>
    ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context);
}
