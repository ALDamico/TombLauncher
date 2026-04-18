using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class PlaySessionDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public PlaySessionDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
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

    public PlaySessionDto GetLastPlaySession(GameMetadataDto dto)
    {
        var lastPlaySession = _dbContext.PlaySession
            .Where(ps => ps.GameId == dto.Id)
            .OrderByDescending(ps => ps.EndDate)
            .FirstOrDefault();
        return _mapper.Map<PlaySessionDto>(lastPlaySession);
    }

    public List<PlaySessionDto> GetPlaySessionsByGameId(int gameId)
    {
        var playSessions = _dbContext.PlaySession
            .Where(ps => ps.GameId == gameId)
            .ToList();
        return _mapper.Map<List<PlaySessionDto>>(playSessions);
    }
}
