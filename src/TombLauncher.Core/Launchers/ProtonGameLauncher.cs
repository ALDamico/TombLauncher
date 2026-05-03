using System.Diagnostics;

namespace TombLauncher.Core.Launchers;

/// <summary>
/// Launches a Windows executable via Proton (Valve's Wine fork) on Linux.
/// Unlike Wine, Proton stays alive until the game exits — no "wineserver -w" needed.
/// Uses STEAM_COMPAT_DATA_PATH instead of WINEPREFIX.
/// </summary>
public class ProtonGameLauncher : IGameLauncher
{
    private readonly string _protonPath;

    public ProtonGameLauncher(string protonPath)
    {
        _protonPath = protonPath;
    }

    public ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context)
    {
        var gameArgs = string.IsNullOrWhiteSpace(context.Arguments) ? string.Empty : " " + context.Arguments;
        var protonCommand = $"\"{_protonPath}\" run \"{context.ExecutableFileName}\"{gameArgs}";

        var psi = new ProcessStartInfo("bash")
        {
            WorkingDirectory = context.WorkingDirectory,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add("-lc");
        psi.ArgumentList.Add(protonCommand);

        // Clear env vars that the .NET runtime may set and that can interfere with Wine's loader.
        psi.Environment.Remove("LD_PRELOAD");

        // TombLauncher may run with NVIDIA PRIME offload vars set in its .desktop launcher.
        // Wine/Proton cannot render correctly when these are inherited — strip them.
        psi.Environment.Remove("__NV_PRIME_RENDER_OFFLOAD");
        psi.Environment.Remove("__GLX_VENDOR_LIBRARY_NAME");
        psi.Environment.Remove("__VK_LAYER_NV_optimus");

        // STEAM_COMPAT_DATA_PATH must always be set and the directory must exist before Proton starts.
        var compatDataPath = string.IsNullOrWhiteSpace(context.PrefixPath)
            ? Path.Combine(context.WorkingDirectory, "proton_pfx")
            : context.PrefixPath;

        Directory.CreateDirectory(compatDataPath);
        psi.Environment["STEAM_COMPAT_DATA_PATH"] = compatDataPath;

        // Required by Proton 10+ for non-Steam games.
        psi.Environment["STEAM_APPID"] = "0";

        // Diagnostics
        psi.Environment["PROTON_LOG"] = "1";
        psi.Environment["DXVK_LOG_LEVEL"] = "info";

        // Tells Proton where the game files are; needed by some Proton versions.
        psi.Environment["STEAM_COMPAT_INSTALL_PATH"] = context.WorkingDirectory;

        // Derive the Steam client root from the Proton binary path:
        // {steam_root}/steamapps/common/Proton X.Y/proton  →  3 levels up = {steam_root}
        var steamClientPath = Path.GetFullPath(
            Path.Combine(Path.GetDirectoryName(_protonPath)!, "..", "..", ".."));
        psi.Environment["STEAM_COMPAT_CLIENT_INSTALL_PATH"] = steamClientPath;

        // Per-game env var overrides applied last — they can override any of the above.
        if (context.ExtraEnvVars != null)
            foreach (var (k, v) in context.ExtraEnvVars)
                psi.Environment[k] = v;

        return psi;
    }
}
