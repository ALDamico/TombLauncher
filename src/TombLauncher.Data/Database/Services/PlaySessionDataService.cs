using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
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

    public async Task<PlaySessionCrashDto> GetCrashInfo(int gameId, CancellationToken cancellationToken)
    {
        var lastPlaySession = await _dbContext.PlaySession.Where(ps => ps.GameId == gameId)
            .OrderByDescending(ps => ps.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        var crashDto = new PlaySessionCrashDto()
        {
            ExitCode = lastPlaySession?.ExitCode,
            StdOut = lastPlaySession?.StdOut,
            StdErr = lastPlaySession?.StdErr,
            CrashFiles = { new CrashFileDto("retrieved", lastPlaySession?.CrashFileContent ?? "") }
        };

        if (lastPlaySession?.CrashFileContent.IsNotNullOrWhiteSpace() == true)
        {
            crashDto.CrashFiles.Add(new CrashFileDto("retrieved", lastPlaySession.CrashFileContent!));
        }

        return crashDto;
    }

    public async Task<PlaySessionCrashDto> GetCrashInfo(int gameId, CancellationToken cancellationToken)
    {
        var lastPlaySession = await _dbContext.PlaySession.Where(ps => ps.GameId == gameId)
            .OrderByDescending(ps => ps.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        var crashDto = new PlaySessionCrashDto()
        {
            ExitCode = lastPlaySession?.ExitCode,
            StdOut = lastPlaySession?.StdOut,
            StdErr = lastPlaySession?.StdErr,
            CrashFiles = { new CrashFileDto("retrieved", lastPlaySession?.CrashFileContent ?? "") }
        };

        if (lastPlaySession?.CrashFileContent.IsNotNullOrWhiteSpace() == true)
        {
            crashDto.CrashFiles.Add(new CrashFileDto("retrieved", lastPlaySession.CrashFileContent!));
        }

        return crashDto;
    }
}
