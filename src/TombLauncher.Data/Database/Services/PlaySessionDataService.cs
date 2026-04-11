using Newtonsoft.Json;
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

    public async Task AddPlaySessionToGame(GameMetadataDto dto, DateTime startDate, DateTime endDate, PlaySessionCrashDto? playSessionCrashDto)
    {
        var playSession = new PlaySession()
        {
            GameId = dto.Id,
            StartDate = startDate,
            EndDate = endDate,
            ExitCode = 0
        };

        if (playSessionCrashDto != null)
        {
            playSession.ExitCode = playSessionCrashDto.ExitCode;
            playSession.StdOut = playSessionCrashDto.StdOut;
            playSession.StdErr = playSessionCrashDto.StdErr;
            playSession.CrashFileContent = JsonConvert.SerializeObject(playSessionCrashDto.CrashFiles, Formatting.None);
        }
        
        _dbContext.PlaySession.Add(playSession);
        await _dbContext.SaveChangesAsync();
    }
}
