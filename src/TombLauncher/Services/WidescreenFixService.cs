using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TombLauncher.Data.Database.Services;

namespace TombLauncher.Services;

public class WidescreenFixService : IWidescreenFixService
{
    private readonly ILogger<WidescreenFixService> _logger;
    private readonly GameDataService _gameDataService;

    public WidescreenFixService(ILogger<WidescreenFixService> logger, GameDataService gameDataService)
    {
        _logger = logger;
        _gameDataService = gameDataService;
    }

    public async Task ApplyWidescreenFix(int gameId)
    {
        _logger.LogInformation("Applying widescreen fix for game {GameId}", gameId);
        await _gameDataService.UpdateWidescreenFix(gameId, true);
    }

    public async Task RemoveWidescreenFix(int gameId)
    {
        _logger.LogInformation("Removing widescreen fix for game {GameId}", gameId);
        await _gameDataService.UpdateWidescreenFix(gameId, false);
    }

    public async Task<bool> IsWidescreenFixApplied(int gameId)
    {
        var game = await _gameDataService.GetGameById(gameId);
        return game?.IsWidescreenFixEnabled ?? false;
    }

    public List<string> GetAvailableWidescreenFixes()
    {
        // Return list of available widescreen fixes (could be based on game engine, etc.)
        return new List<string>() { "16:9", "16:10", "21:9" };
    }
}