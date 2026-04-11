using System.Collections.Generic;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Encapsulates all parameters needed to launch a game via any compatibility tool.
/// Passed to <see cref="IGameLauncher.GetLaunchStartInfo"/> instead of a long parameter list.
/// </summary>
public class GameLaunchContext
{
    /// <summary>Just the filename of the .exe (no directory path).</summary>
    public required string ExecutableFileName { get; init; }

    /// <summary>Extra command-line arguments to pass to the executable.</summary>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>The directory the process will run from.</summary>
    public required string WorkingDirectory { get; init; }

    /// <summary>
    /// Optional per-game isolation path.
    /// For Wine: WINEPREFIX. For Proton: STEAM_COMPAT_DATA_PATH.
    /// </summary>
    public string? PrefixPath { get; init; }

    /// <summary>
    /// Additional environment variables to set after the launcher's own defaults.
    /// These override any variable the launcher sets with the same key.
    /// </summary>
    public IReadOnlyDictionary<string, string>? ExtraEnvVars { get; init; }
}
