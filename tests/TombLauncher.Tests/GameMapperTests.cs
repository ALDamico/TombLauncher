using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Tests;

public class GameMapperTests
{
    private readonly GameMapper _mapper = new(new EnvironmentVariableMapper());

    // ── ToDto ────────────────────────────────────────────────────────────────

    [Fact]
    public void ToDto_MapsCompatibilityTool()
    {
        var game = MakeGame(tool: CompatibilityTool.Proton);
        Assert.Equal(CompatibilityTool.Proton, _mapper.ToDto(game).CompatibilityTool);
    }

    [Fact]
    public void ToDto_MapsCompatibilityToolPath()
    {
        var game = MakeGame(toolPath: "/opt/proton/proton");
        Assert.Equal("/opt/proton/proton", _mapper.ToDto(game).CompatibilityToolPath);
    }

    [Fact]
    public void ToDto_MapsCompatibilityPrefixPath()
    {
        var game = MakeGame(prefixPath: "/home/user/.wine-tr1");
        Assert.Equal("/home/user/.wine-tr1", _mapper.ToDto(game).CompatibilityPrefixPath);
    }

    [Fact]
    public void ToDto_MapsEnvironmentVariables()
    {
        var game = MakeGame();
        game.EnvironmentVariables.Add(new GameEnvironmentVariable
            { GameId = 1, VariableName = "DXVK_HUD", VariableValue = "fps" });

        var dto = _mapper.ToDto(game);

        Assert.Single(dto.ExtraEnvVars);
        Assert.Equal("DXVK_HUD", dto.ExtraEnvVars[0].VariableName);
        Assert.Equal("fps", dto.ExtraEnvVars[0].VariableValue);
    }

    [Fact]
    public void ToDto_ExtraEnvVars_EmptyWhenNoneConfigured()
    {
        var dto = _mapper.ToDto(MakeGame());
        Assert.Empty(dto.ExtraEnvVars);
    }

    // ── ToGame ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToGame_MapsCompatibilityTool()
    {
        var dto = MakeDto(tool: CompatibilityTool.Wine);
        Assert.Equal(CompatibilityTool.Wine, _mapper.ToGame(dto).CompatibilityTool);
    }

    [Fact]
    public void ToGame_MapsCompatibilityToolPath()
    {
        var dto = MakeDto(toolPath: "/usr/bin/wine");
        Assert.Equal("/usr/bin/wine", _mapper.ToGame(dto).CompatibilityToolPath);
    }

    [Fact]
    public void ToGame_MapsCompatibilityPrefixPath()
    {
        var dto = MakeDto(prefixPath: "/home/user/.wine-prefix");
        Assert.Equal("/home/user/.wine-prefix", _mapper.ToGame(dto).CompatibilityPrefixPath);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static Game MakeGame(
        CompatibilityTool tool = CompatibilityTool.Automatic,
        string? toolPath = null,
        string? prefixPath = null) => new()
    {
        Title = "Test Game",
        InstallDirectory = "/games/test",
        CompatibilityTool = tool,
        CompatibilityToolPath = toolPath,
        CompatibilityPrefixPath = prefixPath
    };

    private static GameMetadataDto MakeDto(
        CompatibilityTool tool = CompatibilityTool.Automatic,
        string? toolPath = null,
        string? prefixPath = null) => new()
    {
        Title = "Test Game",
        InstallDirectory = "/games/test",
        CompatibilityTool = tool,
        CompatibilityToolPath = toolPath,
        CompatibilityPrefixPath = prefixPath,
        TitlePic = []
    };
}
