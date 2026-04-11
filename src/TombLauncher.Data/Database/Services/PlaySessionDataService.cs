using AutoMapper;
using Newtonsoft.Json;
using TombLauncher.Core.Dtos;
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
