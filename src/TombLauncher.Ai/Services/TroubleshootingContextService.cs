using TombLauncher.Ai.Models;
using TombLauncher.Data.Database.Services;

namespace TombLauncher.Ai.Services;

public class TroubleshootingContextService
{
    public TroubleshootingContextService(PlaySessionDataService playSessionDataService, GameDataService gameDataService)
    {
        _playSessionDataService = playSessionDataService;
        _gameDataService = gameDataService;
    }
    private readonly PlaySessionDataService _playSessionDataService;
    private readonly GameDataService _gameDataService;
    
    public async Task<TroubleshootingContext> GetTroubleshootingContext(int gameId, CancellationToken cancellationToken)
    {
        var lastCrashInfo = await _playSessionDataService.GetCrashInfo(gameId, cancellationToken);
        var gameEngine = await _gameDataService.GetGameEngineById(gameId, cancellationToken);
        return new TroubleshootingContext()
        {
            GameId = gameId,
            LastCrashLog = lastCrashInfo.CrashFiles.FirstOrDefault(cf => cf.FileName == "retrieved")?.Content,
            LastExitCode = lastCrashInfo.ExitCode,
            LastStdErr = lastCrashInfo.StdErr,
            LastStdOut = lastCrashInfo.StdOut,
            GameEngine = gameEngine
        };
    }
}