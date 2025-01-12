using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.UnitOfWork;

public class GamesUnitOfWork : UnitOfWorkBase
{
    public GamesUnitOfWork(MapperConfiguration mapperConfiguration)
    {
        _mapper = mapperConfiguration.CreateMapper();
        _games = GetRepository<Game>();
        _playSessions = GetRepository<PlaySession>();
        _hashes = GetRepository<GameHashes>();
        _links = GetRepository<GameLink>();
    }

    private IMapper _mapper;

    private readonly Lazy<EfRepository<Game>> _games;
    private readonly Lazy<EfRepository<PlaySession>> _playSessions;
    private readonly Lazy<EfRepository<GameHashes>> _hashes;
    private readonly Lazy<EfRepository<GameLink>> _links;

    internal EfRepository<Game> Games => _games.Value;

    internal EfRepository<PlaySession> PlaySessions => _playSessions.Value;
    internal EfRepository<GameHashes> Hashes => _hashes.Value;
    internal EfRepository<GameLink> Links => _links.Value;

    public GameMetadataDto GetGameById(int id)
    {
        var entity = Games.GetEntityById(id);
        return _mapper.Map<GameMetadataDto>(entity);
    }


    public void UpsertGame(IGameMetadata game)
    {
        var entity = _mapper.Map<Game>(game);
        if (entity.Id == default)
        {
            Games.Insert(entity);
        }
        else
        {
            Games.Update(entity);
        }

        Save();
        game.Id = entity.Id;
    }

    public void DeleteGameById(int id)
    {
        Games.Delete(id);
    }

    private GameHashes ToGameHashes(GameHashDto dto)
    {
        return _mapper.Map<GameHashes>(dto);
    }

    public List<GameMetadataDto> GetGames()
    {
        return _mapper.Map<List<GameMetadataDto>>(Games.GetAll());
    }

    public List<GameWithStatsDto> GetGamesWithStats()
    {
        var outputList = new List<GameWithStatsDto>();
        var playSessions = PlaySessions.GetAll().ToLookup(ps => ps.GameId);

        var games = Games.GetAll().ToList();
        foreach (var game in games)
        {
            var thisGamePlaySessions = playSessions[game.Id].ToList();
            var gameWithStatsDto = new GameWithStatsDto()
            {
                GameMetadata = _mapper.Map<GameMetadataDto>(game),
                TotalPlayedTime = TimeSpan.Zero
            };

            if (thisGamePlaySessions.Any())
            {
                gameWithStatsDto.LastPlayed = thisGamePlaySessions.Max(ps => ps.StartDate);
                gameWithStatsDto.TotalPlayedTime =
                    TimeSpan.FromTicks(thisGamePlaySessions.Select(ps => (ps.EndDate - ps.StartDate).Ticks).Sum());
            }

            outputList.Add(gameWithStatsDto);
        }

        return outputList;
    }

    public GameWithStatsDto GetGameWithStats(int id)
    {
        var playSessions = PlaySessions.Get(ps => ps.GameId == id).ToList();
        var game = Games.GetEntityById(id);
        var gameWithStatsDto = new GameWithStatsDto()
        {
            GameMetadata = _mapper.Map<GameMetadataDto>(game),
            TotalPlayedTime = TimeSpan.Zero
        };
        if (playSessions.Any())
        {
            gameWithStatsDto.LastPlayed = playSessions.Max(ps => ps.StartDate);
            gameWithStatsDto.TotalPlayedTime =
                TimeSpan.FromTicks(playSessions.Select(ps => (ps.EndDate - ps.StartDate).Ticks).Sum());
        }

        return gameWithStatsDto;
    }

    public List<PlaySessionDto> GetPlaySessionsByGameId(int gameId)
    {
        var playSessions = PlaySessions.Get(ps => ps.Game.Id == gameId).ToList();
        return _mapper.Map<List<PlaySessionDto>>(playSessions);
    }

    public void AddPlaySessionToGame(GameMetadataDto dto, DateTime startDate, DateTime endDate)
    {
        var gameId = dto.Id;
        var playSession = new PlaySession()
        {
            GameId = gameId,
            StartDate = startDate,
            EndDate = endDate
        };
        PlaySessions.Insert(playSession);
    }

    public PlaySessionDto GetLastPlaySession(GameMetadataDto dto)
    {
        var gameId = dto.Id;
        var lastPlaysession = PlaySessions
            .Get(ps => ps.GameId == gameId, playSessions => playSessions.OrderByDescending(ps => ps.EndDate))
            .FirstOrDefault();
        return _mapper.Map<PlaySessionDto>(lastPlaysession);
    }

    public List<GameHashDto> GetHashes(GameMetadataDto dto)
    {
        var queryResult = Hashes.Get(h => h.GameId == dto.Id); //.AsEnumerable().ToDtos().ToList();
        return _mapper.Map<List<GameHashDto>>(queryResult);
    }

    public bool ExistsHashes(List<GameHashDto> computedHashes, out int? foundId)
    {
        foundId = null;
        var hashesRepo = Hashes.GetAll();
        var tempQueryable = computedHashes.Select(ToGameHashes).AsQueryable();
        var joined = hashesRepo.AsEnumerable().Join(tempQueryable, gh => gh.FileName + "#" + gh.Md5Hash,
            tmp => tmp.FileName + "#" + tmp.Md5Hash,
            (gh, tmp) => gh);
        var matches = joined.GroupBy(m => m.GameId).Select(g => new { Id = g.Key, Count = g.Count() }).ToList();
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
            var idToReturn = matches.FirstOrDefault()?.Id;
            foundId = idToReturn;
            return true;
        }

        return false;
    }

    public void SaveHashes(List<GameHashDto> hashes)
    {
        foreach (var hash in hashes)
        {
            Hashes.Insert(ToGameHashes(hash));
        }

        Save();
    }

    public List<GameLinkDto> GetLinks(int gameId, LinkType? linkType = null)
    {
        var queryResult = Links.Get(l => l.GameId == gameId);
        if (linkType != null)
            queryResult = queryResult.Where(g => g.LinkType == linkType);
        return _mapper.Map<List<GameLinkDto>>(queryResult);
    }

    public void SaveLink(GameLinkDto dto)
    {
        if (dto.Id != default) return;

        var entity = _mapper.Map<GameLink>(dto);
        Links.Upsert(entity);
    }

    public GameMetadataDto GetGameByLinks(LinkType linkType, List<string> links)
    {
        var gameIds = Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType).Select(l => l.GameId).Distinct()
            .ToList();
        if (gameIds.Count == 1)
        {
            return _mapper.Map<GameMetadataDto>(Games.GetEntityById(gameIds.First()));
        }

        return null;
    }

    public List<GameMetadataDto> GetGamesByLinks(LinkType linkType, List<string> links)
    {
        var games = Games.GetAll();
        var queryResult = Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType)
            .Select(l => l.GameId)
            .Join(games, i => i, game => game.Id, (i, game) => game);

        return _mapper.Map<List<GameMetadataDto>>(queryResult);
    }

    public Dictionary<string, GameWithStatsDto> GetGamesByLinksDictionary(LinkType linkType, List<string> links)
    {
        var stats = GetGamesWithStats();
        return Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType).ToList()
            .Join(stats, l => l.GameId, game => game.GameMetadata.Id, (i, game) => new { Link = i.Link, Game = game })
            .ToDictionary(k => k.Link, g => g.Game);
    }

    public StatisticsDto GetStatistics()
    {
        var output = new StatisticsDto();

        var playSessionsRepo = PlaySessions.GetAll().Include(ps => ps.Game);
        var gamesRepo = Games.GetAll();

        var groupedByGame = playSessionsRepo.ToLookup(ps => ps.GameId);
        var latestPayedGame = GetLatestPlayedGame(playSessionsRepo);
        output.LatestPlayedGame = latestPayedGame;
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
            var dto = new DailyStatisticsDto()
            {
                Date = dateTimeIterator.ToDateTime(new TimeOnly()),
                AverageGameDuration = averagePlayTime,
                DifferentGamesPlayed = distinctGamesPlayed,
                TotalPlayTime = totalPlayTime
            };
            
            dailyStats.Add(dto);

            dateTimeIterator = dateTimeIterator.AddDays(1);
        }

        output.DailyStatistics = dailyStats;

        return output;
    }

    private static GameStatisticsDto GetMostPlayedByLaunches(ILookup<int, PlaySession> groupedByGame)
    {
        var mostPlaySessions = groupedByGame.MaxBy(g => g.Count());
        var mostPlayed = new GameStatisticsDto()
        {
            Title = mostPlaySessions.First().Game.Title,
            TotalSessions = (uint)mostPlaySessions.Count(),
            Id = mostPlaySessions.First().GameId
        };
        return mostPlayed;
    }

    private GameStatisticsDto GetLongestPlaySession(IIncludableQueryable<PlaySession, Game> repo)
    {
        var listified = repo.ToList();
        var maxDuration = listified.Max(s => s.EndDate - s.StartDate);

        var longest = listified.FirstOrDefault(r => r.EndDate - r.StartDate == maxDuration);
        return new GameStatisticsDto()
        {
            Title = longest.Game.Title,
            LastPlayed = longest.StartDate,
            LastPlayedEnd = longest.EndDate,
            Id = longest.GameId
        };
    }

    private GameStatisticsDto GetLatestPlayedGame(IIncludableQueryable<PlaySession, Game> repo)
    {
        var latestSession = repo.FirstOrDefault(ps => ps.StartDate == repo.Max(p => p.StartDate));

        return new GameStatisticsDto()
        {
            Title = latestSession.Game.Title,
            LastPlayed = latestSession.StartDate,
            Id = latestSession.GameId
        };
    }
}