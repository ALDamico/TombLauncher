using System.Globalization;
using System.IO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class StatisticsDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public StatisticsDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public async Task<StatisticsDto> GetStatistics()
    {
        var output = new StatisticsDto();

        var playSessionsRepo = _dbContext.PlaySessions.Include(ps => ps.Game);

        var groupedByGame = playSessionsRepo.ToLookup(ps => ps.GameId);
        var latestPlayedGame = GetLatestPlayedGame(playSessionsRepo);
        output.LatestPlayedGame = latestPlayedGame;
        var longestPlaySession = GetLongestPlaySession(playSessionsRepo);
        output.LongestPlaySession = longestPlaySession;

        var mostPlayed = GetMostPlayedByLaunches(groupedByGame);
        output.MostLaunches = mostPlayed;

        var byDayOfWeek = playSessionsRepo.ToLookup(ps => ps.StartDate.DayOfWeek);
        foreach (var dayOfWeek in Enum.GetValues<DayOfWeek>())
        {
            var averageTimePlayed = byDayOfWeek[dayOfWeek].Select(ps => ps.EndDate - ps.StartDate).Average();
            var playsByThatDay = byDayOfWeek[dayOfWeek].Count();
            var dto = new DayOfWeekStatisticsDto()
            {
                DayOfWeek = dayOfWeek,
                AverageTimePlayed = averageTimePlayed,
                PlaySessionsCount = playsByThatDay
            };
            output.DayOfWeekStatistics.Add(dto);
        }

        output.DayOfWeekStatistics = output.DayOfWeekStatistics.OrderBy(d => d.DayOfWeek,
            new DayOfWeekCultureSensitiveComparer(CultureInfo.CurrentUICulture)).ToList();

        var todayMinus30Days = DateTime.Now.GetOneSecondToMidnight() - TimeSpan.FromDays(29);

        var last30Days = playSessionsRepo.Where(s => s.StartDate > todayMinus30Days).OrderBy(s => s.StartDate)
            .ToLookup(d => DateOnly.FromDateTime(d.StartDate));

        var dailyStats = new List<DailyStatisticsDto>();

        var dateTimeIterator = DateOnly.FromDateTime(todayMinus30Days);
        while (dateTimeIterator <= DateOnly.FromDateTime(DateTime.Now))
        {
            var dailyStatistics = last30Days[dateTimeIterator].ToList();
            var distinctGamesPlayed = dailyStatistics.Select(ds => ds.GameId).Distinct().Count();

            var playTimes = dailyStatistics.Select(ds => ds.EndDate - ds.StartDate).ToList();
            var averagePlayTime = playTimes.Average();
            var totalPlayTime = playTimes.Sum();
            var dailyDto = new DailyStatisticsDto()
            {
                Date = dateTimeIterator.ToDateTime(new TimeOnly()),
                AverageGameDuration = averagePlayTime,
                DifferentGamesPlayed = distinctGamesPlayed,
                TotalPlayTime = totalPlayTime
            };

            dailyStats.Add(dailyDto);

            dateTimeIterator = dateTimeIterator.AddDays(1);
        }

        output.DailyStatistics = dailyStats;

        output.SpaceUsedStatistics = await GetGameSpaceUsedStatistics();

        return output;
    }

    private async Task<List<GameSpaceUsedDto>> GetGameSpaceUsedStatistics()
    {
        var spaceUsedStatistics = new List<GameSpaceUsedDto>();
        var games = await _dbContext.Games.ToListAsync();

        foreach (var game in games)
        {
            var installDirectory = game.InstallDirectory;
            var directorySize = 0L;
            if (Directory.Exists(installDirectory))
            {
                directorySize = PathUtils.GetDirectorySize(installDirectory);
            }
            else
            {
                continue;
            }

            var gameSpaceUsed = new GameSpaceUsedDto()
            {
                Id = game.Id,
                Title = game.Title,
                SpaceUsedBytes = directorySize
            };
            spaceUsedStatistics.Add(gameSpaceUsed);
        }

        return spaceUsedStatistics;
    }

    private static GameStatisticsDto? GetMostPlayedByLaunches(ILookup<int, PlaySession> groupedByGame)
    {
        if (groupedByGame.Count == 0)
            return null;
        var mostPlaySessions = groupedByGame.MaxBy(g => g.Count());
        var mostPlayed = new GameStatisticsDto()
        {
            Title = mostPlaySessions!.First().Game.Title,
            TotalSessions = (uint)mostPlaySessions.Count(),
            Id = mostPlaySessions.First().GameId
        };
        return mostPlayed;
    }

    private GameStatisticsDto? GetLongestPlaySession(IQueryable<PlaySession> repo)
    {
        var listified = repo.ToList();
        if (listified.Count == 0)
        {
            return null;
        }
        var maxDuration = listified.Max(s => s.EndDate - s.StartDate);

        var longest = listified.FirstOrDefault(r => r.EndDate - r.StartDate == maxDuration);
        if (longest == null)
            return null;
        return new GameStatisticsDto()
        {
            Title = longest.Game.Title,
            LastPlayed = longest.StartDate,
            LastPlayedEnd = longest.EndDate,
            Id = longest.GameId
        };
    }

    private GameStatisticsDto? GetLatestPlayedGame(IQueryable<PlaySession> repo)
    {
        var latestSession = repo.FirstOrDefault(ps => ps.StartDate == repo.Max(p => p.StartDate));
        if (latestSession == null)
            return null;

        return new GameStatisticsDto()
        {
            Title = latestSession.Game.Title,
            LastPlayed = latestSession.StartDate,
            Id = latestSession.GameId
        };
    }
}
