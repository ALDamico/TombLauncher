using Microsoft.EntityFrameworkCore;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Services;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Tests;

public class UpdateLaunchOptionsTests : IDisposable
{
    private readonly TombLauncherDbContext _context;
    private readonly GameDataService _service;
    private readonly int _gameId;

    public UpdateLaunchOptionsTests()
    {
        var options = new DbContextOptionsBuilder<TombLauncherDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new TombLauncherDbContext(options);

        var envMapper = new EnvironmentVariableMapper();
        _service = new GameDataService(_context, new FileBackupMapper(), new GameMapper(envMapper), envMapper);

        var game = new Game { Title = "Test Game", InstallDirectory = "/games/test" };
        _context.Games.Add(game);
        _context.FileBackups.Add(new FileBackup
        {
            Game = game, FileName = "game.exe", FileType = FileType.GameExecutable
        });
        _context.SaveChanges();
        _gameId = game.Id;
    }

    // ── compatibility fields ──────────────────────────────────────────────────

    [Fact]
    public async Task PersistsCompatibilityTool()
    {
        await _service.UpdateLaunchOptions(MakeDto(tool: CompatibilityTool.Proton));
        Assert.Equal(CompatibilityTool.Proton, _context.Games.Find(_gameId)!.CompatibilityTool);
    }

    [Fact]
    public async Task PersistsCompatibilityToolPath()
    {
        await _service.UpdateLaunchOptions(MakeDto(toolPath: "/opt/proton/proton"));
        Assert.Equal("/opt/proton/proton", _context.Games.Find(_gameId)!.CompatibilityToolPath);
    }

    [Fact]
    public async Task PersistsCompatibilityPrefixPath()
    {
        await _service.UpdateLaunchOptions(MakeDto(prefixPath: "/home/user/.wine-tr1"));
        Assert.Equal("/home/user/.wine-tr1", _context.Games.Find(_gameId)!.CompatibilityPrefixPath);
    }

    // ── env vars ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddsEnvVars()
    {
        var dto = MakeDto(envVars:
        [
            new() { GameId = _gameId, VariableName = "DXVK_HUD", VariableValue = "fps" }
        ]);

        await _service.UpdateLaunchOptions(dto);

        var saved = _context.GameEnvironmentVariables.Where(e => e.GameId == _gameId).ToList();
        Assert.Single(saved);
        Assert.Equal("DXVK_HUD", saved[0].VariableName);
    }

    [Fact]
    public async Task ReplacesEnvVars_OnSecondSave()
    {
        await _service.UpdateLaunchOptions(MakeDto(envVars:
        [
            new() { GameId = _gameId, VariableName = "OLD_VAR", VariableValue = "old" }
        ]));

        await _service.UpdateLaunchOptions(MakeDto(envVars:
        [
            new() { GameId = _gameId, VariableName = "NEW_VAR", VariableValue = "new" }
        ]));

        var saved = _context.GameEnvironmentVariables.Where(e => e.GameId == _gameId).ToList();
        Assert.Single(saved);
        Assert.Equal("NEW_VAR", saved[0].VariableName);
    }

    [Fact]
    public async Task ClearsEnvVars_WhenListEmpty()
    {
        await _service.UpdateLaunchOptions(MakeDto(envVars:
        [
            new() { GameId = _gameId, VariableName = "SOME_VAR", VariableValue = "val" }
        ]));

        await _service.UpdateLaunchOptions(MakeDto());

        Assert.Empty(_context.GameEnvironmentVariables.Where(e => e.GameId == _gameId));
    }

    // ── setup executable ─────────────────────────────────────────────────────

    [Fact]
    public async Task AddsSetupExecutable_WhenNew()
    {
        var dto = MakeDto(setupExe: new FileBackupDto
            { GameId = _gameId, FileName = "setup.exe", FileType = FileType.SetupExecutable });

        await _service.UpdateLaunchOptions(dto);

        var saved = _context.FileBackups
            .FirstOrDefault(b => b.GameId == _gameId && b.FileType == FileType.SetupExecutable);
        Assert.NotNull(saved);
        Assert.Equal("setup.exe", saved.FileName);
    }

    [Fact]
    public async Task UpdatesSetupExecutable_WhenExists()
    {
        _context.FileBackups.Add(new FileBackup
        {
            GameId = _gameId, FileName = "old_setup.exe", FileType = FileType.SetupExecutable
        });
        _context.SaveChanges();

        await _service.UpdateLaunchOptions(MakeDto(setupExe: new FileBackupDto
            { GameId = _gameId, FileName = "new_setup.exe", FileType = FileType.SetupExecutable }));

        var backups = _context.FileBackups
            .Where(b => b.GameId == _gameId && b.FileType == FileType.SetupExecutable).ToList();
        Assert.Single(backups);
        Assert.Equal("new_setup.exe", backups[0].FileName);
    }

    [Fact]
    public async Task RemovesSetupExecutable_WhenNull()
    {
        _context.FileBackups.Add(new FileBackup
        {
            GameId = _gameId, FileName = "setup.exe", FileType = FileType.SetupExecutable
        });
        _context.SaveChanges();

        await _service.UpdateLaunchOptions(MakeDto(setupExe: null));

        Assert.Empty(_context.FileBackups
            .Where(b => b.GameId == _gameId && b.FileType == FileType.SetupExecutable));
    }

    // ── helper ────────────────────────────────────────────────────────────────

    private LaunchOptionsDto MakeDto(
        CompatibilityTool tool = CompatibilityTool.Automatic,
        string? toolPath = null,
        string? prefixPath = null,
        List<EnvironmentVariableDto>? envVars = null,
        FileBackupDto? setupExe = null) => new()
    {
        GameId = _gameId,
        GameEngine = GameEngine.Unknown,
        GameExecutable = new FileBackupDto
            { GameId = _gameId, FileName = "game.exe", FileType = FileType.GameExecutable },
        CompatibilityTool = tool,
        CompatibilityToolPath = toolPath,
        CompatibilityPrefixPath = prefixPath,
        ExtraEnvVars = envVars ?? [],
        SetupExecutable = setupExe
    };

    public void Dispose() => _context.Dispose();
}
