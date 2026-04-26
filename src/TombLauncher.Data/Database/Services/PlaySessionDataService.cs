using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class PlaySessionDataService
{
    private readonly TombLauncherDbContext _dbContext;

    public PlaySessionDataService(TombLauncherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddPlaySessionToGame(GameMetadataDto dto, DateTime startDate, DateTime endDate)
    {
        var playSession = new PlaySession()
        {
            GameId = dto.Id,
            StartDate = startDate,
            EndDate = endDate
        };
        _dbContext.PlaySession.Add(playSession);
        await _dbContext.SaveChangesAsync();
    }
}
