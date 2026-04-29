using System.Diagnostics;
using TombLauncher.Core.Launchers;

namespace TombLauncher.Tests;

public class GameLauncherTests
{
    // ─── WineGameLauncher ────────────────────────────────────────────────────

    [Fact]
    public void Wine_Uses_Bash_As_Executable()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame"
        });
        Assert.Equal("bash", psi.FileName);
    }

    [Fact]
    public void Wine_Command_Includes_WineServer_Wait()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame"
        });
        Assert.Contains("wineserver -w", psi.ArgumentList[1]);
    }

    [Fact]
    public void Wine_Command_Includes_Exe()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame"
        });
        Assert.Contains("game.exe", psi.ArgumentList[1]);
    }

    [Fact]
    public void Wine_Sets_WinePrefix_When_Provided()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame",
            PrefixPath = "/home/user/.wine-mygame"
        });
        Assert.True(psi.Environment.ContainsKey("WINEPREFIX"));
        Assert.Equal("/home/user/.wine-mygame", psi.Environment["WINEPREFIX"]);
    }

    [Fact]
    public void Wine_Does_Not_Set_WinePrefix_When_Empty()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame"
        });
        Assert.False(psi.Environment.ContainsKey("WINEPREFIX"));
    }

    [Fact]
    public void Wine_Uses_WorkingDirectory()
    {
        var launcher = new WineGameLauncher("/usr/bin/wine");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = "/games/mygame"
        });
        Assert.Equal("/games/mygame", psi.WorkingDirectory);
    }

    // ─── ProtonGameLauncher ─────────────────────────────────────────────────
    // ProtonGameLauncher calls Directory.CreateDirectory internally, so tests
    // must use a working directory that actually exists (e.g. GetTempPath()).

    [Fact]
    public void Proton_Uses_Bash_As_Executable()
    {
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = Path.GetTempPath()
        });
        Assert.Equal("bash", psi.FileName);
    }

    [Fact]
    public void Proton_Command_Includes_Run_Subcommand()
    {
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = Path.GetTempPath()
        });
        Assert.Contains("run", psi.ArgumentList[1]);
    }

    [Fact]
    public void Proton_Command_Does_Not_Include_WineServer_Wait()
    {
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = Path.GetTempPath()
        });
        Assert.DoesNotContain("wineserver -w", psi.ArgumentList[1]);
    }

    [Fact]
    public void Proton_Sets_SteamCompatDataPath_When_Provided()
    {
        var prefixPath = Path.Combine(Path.GetTempPath(), "proton-data-mygame");
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = Path.GetTempPath(),
            PrefixPath = prefixPath
        });
        Assert.True(psi.Environment.ContainsKey("STEAM_COMPAT_DATA_PATH"));
        Assert.Equal(prefixPath, psi.Environment["STEAM_COMPAT_DATA_PATH"]);
    }

    [Fact]
    public void Proton_Uses_Default_SteamCompatDataPath_When_Not_Provided()
    {
        // When no PrefixPath is set, Proton falls back to {WorkingDirectory}/proton_pfx.
        var workDir = Path.GetTempPath();
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = workDir
        });
        Assert.True(psi.Environment.ContainsKey("STEAM_COMPAT_DATA_PATH"));
        Assert.Equal(Path.Combine(workDir, "proton_pfx"), psi.Environment["STEAM_COMPAT_DATA_PATH"]);
    }

    [Fact]
    public void Proton_Does_Not_Set_WinePrefix()
    {
        var prefixPath = Path.Combine(Path.GetTempPath(), "some-prefix");
        var launcher = new ProtonGameLauncher("/path/to/proton");
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = Path.GetTempPath(),
            PrefixPath = prefixPath
        });
        Assert.False(psi.Environment.ContainsKey("WINEPREFIX"));
    }

    // ─── WindowsGameLauncher ────────────────────────────────────────────────

    [Fact]
    public void Windows_FileName_Is_Executable()
    {
        var launcher = new WindowsGameLauncher();
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = @"C:\Games\MyGame"
        });
        Assert.Equal("game.exe", psi.FileName);
    }

    [Fact]
    public void Windows_Uses_ShellExecute()
    {
        var launcher = new WindowsGameLauncher();
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = @"C:\Games\MyGame"
        });
        Assert.True(psi.UseShellExecute);
    }

    [Fact]
    public void Windows_Sets_WorkingDirectory()
    {
        var launcher = new WindowsGameLauncher();
        var psi = launcher.GetLaunchStartInfo(new GameLaunchContext
        {
            ExecutableFileName = "game.exe",
            WorkingDirectory = @"C:\Games\MyGame"
        });
        Assert.Equal(@"C:\Games\MyGame", psi.WorkingDirectory);
    }
}
