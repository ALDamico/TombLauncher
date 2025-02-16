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
        _backups = GetRepository<FileBackup>();
        _savegameMetadata = GetRepository<SavegameMetadata>();
    }

    private readonly IMapper _mapper;

    private readonly Lazy<EfRepository<Game>> _games;
    private readonly Lazy<EfRepository<PlaySession>> _playSessions;
    private readonly Lazy<EfRepository<GameHashes>> _hashes;
    private readonly Lazy<EfRepository<GameLink>> _links;
    private readonly Lazy<EfRepository<FileBackup>> _backups;
    private readonly Lazy<EfRepository<SavegameMetadata>> _savegameMetadata;

    internal EfRepository<Game> Games => _games.Value;

    internal EfRepository<PlaySession> PlaySessions => _playSessions.Value;
    internal EfRepository<GameHashes> Hashes => _hashes.Value;
    internal EfRepository<GameLink> Links => _links.Value;
    internal EfRepository<FileBackup> Backups => _backups.Value;

    internal EfRepository<SavegameMetadata> SavegameMetadata => _savegameMetadata.Value;

    public GameMetadataDto GetGameById(int id)
    {
        var entity = Games.GetEntityById(id);
        return _mapper.Map<GameMetadataDto>(entity);
    }


    public async Task UpsertGame(IGameMetadata game)
    {
        Game entity;
        if (game.Id == 0)
        {
            entity = _mapper.Map<Game>(game);
            Games.Insert(entity);
        }
        else
        {
            entity = Games.GetEntityById(game.Id);
            entity.Author = game.Author;
            entity.Description = game.Description;
            entity.Difficulty = game.Difficulty;
            entity.Guid = game.Guid;
            entity.Id = game.Id;
            entity.Length = game.Length;
            entity.Setting = game.Setting;
            entity.Title = game.Title;
            entity.GameEngine = game.GameEngine;
            entity.InstallDate = game.InstallDate;
            entity.InstallDirectory = game.InstallDirectory;
            entity.IsInstalled = game.IsInstalled;
            entity.ReleaseDate = game.ReleaseDate;
            entity.TitlePic = game.TitlePic;
            entity.AuthorFullName = game.AuthorFullName;
            Games.Update(entity);
        }
        
        // TODO Save launch options
        /*var launchOptionsDto = new LaunchOptionsDto()
        {
            GameEngine = game.GameEngine,
            GameExecutable = new FileBackupDto(){FileType = FileType.GameExecutable, FileName = game.ExecutablePath},
        }*/

        await Save();
        game.Id = entity.Id;
    }

    public void MarkGameAsUninstalled(int id)
    {
        var game = Games.GetEntityById(id);
        game.IsInstalled = false;
        Games.Update(game);
    }

    private GameHashes ToGameHashes(GameHashDto dto)
    {
        return _mapper.Map<GameHashes>(dto);
    }

    public List<GameMetadataDto> GetGames()
    {
        return _mapper.Map<List<GameMetadataDto>>(Games.GetAll());
    }

    public async Task<List<GameWithStatsDto>> GetGamesWithStats(bool installedOnly = false)
    {
        var outputList = new List<GameWithStatsDto>();
        var playSessions = PlaySessions.GetAll().ToLookup(ps => ps.GameId);
        
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        IQueryable<Game> games = Games.GetAll().Include(g => g.FileBackups.Where(b => targetFileTypes.Contains(b.FileType)));
        if (installedOnly)
        {
            games = games.Where(g => g.IsInstalled);
        }
        foreach (var game in games)
        {
            var gameMetadata = _mapper.Map<GameMetadataDto>(game);
            var thisGamePlaySessions = playSessions[game.Id].ToList();
            var gameWithStatsDto = new GameWithStatsDto()
            {
                GameMetadata = gameMetadata,
                TotalPlayedTime = TimeSpan.Zero
            };

            if (thisGamePlaySessions.Count != 0)
            {
                gameWithStatsDto.LastPlayed = thisGamePlaySessions.Max(ps => ps.StartDate);
                gameWithStatsDto.TotalPlayedTime =
                    TimeSpan.FromTicks(thisGamePlaySessions.Select(ps => (ps.EndDate - ps.StartDate).Ticks).Sum());
            }

            outputList.Add(gameWithStatsDto);
        }

        return await Task.FromResult(outputList);
    }

    private async Task<GameMetadataDto> GetGameWithExecutables(int id)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };
        
        var game = await Games.Get().Include(g => g.FileBackups.Where(f => targetFileTypes.Contains(f.FileType)))
            .SingleAsync(g => g.Id == id);

        return _mapper.Map<GameMetadataDto>(game);
    }

    public async Task<GameWithStatsDto> GetGameWithStats(int id)
    {
        var playSessions = PlaySessions.Get(ps => ps.GameId == id).ToList();
        var gameWithStatsDto = new GameWithStatsDto()
        {
            GameMetadata = await GetGameWithExecutables(id),
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
        var queryResult = Hashes.Get(h => h.GameId == dto.Id);
        return _mapper.Map<List<GameHashDto>>(queryResult);
    }

    public bool ExistsHashes(List<GameHashDto> computedHashes, out int? foundId)
    {
        foundId = null;
        var hashesRepo = Hashes.GetAll();
        var tempQueryable = computedHashes.Select(ToGameHashes).AsQueryable();
        var joined = hashesRepo.AsEnumerable().Join(tempQueryable, gh => gh.FileName + "#" + gh.Md5Hash,
            tmp => tmp.FileName + "#" + tmp.Md5Hash,
            (gh, _) => gh);
        var matches = joined.GroupBy(m => m.GameId).Select(g => new { Id = g.Key, Count = g.Count() }).ToList();
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
            var idToReturn = matches.FirstOrDefault()?.Id;
            var game = Games.GetEntityById(idToReturn.GetValueOrDefault());
            if (!game.IsInstalled)
                return false;
            foundId = idToReturn;
            return true;
        }

        return false;
    }

    public async Task SaveHashes(List<GameHashDto> hashes)
    {
        foreach (var hash in hashes)
        {
            Hashes.Insert(ToGameHashes(hash));
        }

        await Save();
    }

    public List<GameLinkDto> GetLinks(int gameId, LinkType? linkType = null)
    {
        var queryResult = Links.Get(l => l.GameId == gameId);
        if (linkType != null)
            queryResult = queryResult.Where(g => g.LinkType == linkType);
        return _mapper.Map<List<GameLinkDto>>(queryResult);
    }

    public async Task SaveLink(GameLinkDto dto)
    {
        if (dto.Id != 0) return;

        var entity = await Links.Get(l => l.Link == dto.Link && l.LinkType == dto.LinkType && l.BaseUrl == dto.BaseUrl).FirstOrDefaultAsync() ??
                     _mapper.Map<GameLink>(dto);
        Links.Upsert(entity);
    }

    public async Task<GameMetadataDto> GetGameByLinks(LinkType linkType, List<string> links)
    {
        var gameIds = Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType).Select(l => l.GameId).Distinct()
            .ToList();
        if (gameIds.Count == 1)
        {
            var gameId = gameIds.First();
            return await GetGameWithExecutables(gameId);
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

    public async Task<Dictionary<string, GameWithStatsDto>> GetGamesByLinksDictionary(LinkType linkType, List<string> links)
    {
        var stats = await GetGamesWithStats();
        return (await Links.Get(l => links.Contains(l.Link) && l.LinkType == linkType).ToListAsync())
            .Join(stats, l => l.GameId, game => game.GameMetadata.Id, (i, game) => new { i.Link, Game = game })
            .ToDictionary(k => k.Link, g => g.Game);
    }

    public StatisticsDto GetStatistics()
    {
        var output = new StatisticsDto();

        var playSessionsRepo = PlaySessions.GetAll().Include(ps => ps.Game);

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

    private GameStatisticsDto GetLatestPlayedGame(IIncludableQueryable<PlaySession, Game> repo)
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

    public void BackupSavegames(int gameId, List<SavegameBackupDto> dtos, int? numberOfVersionsToKeep)
    {
        dtos.ForEach(f => f.GameId = gameId);
        var entitiesToPersist = _mapper.Map<List<FileBackup>>(dtos);
        foreach (var entity in entitiesToPersist)
        {
            Backups.Insert(entity);
            SavegameMetadata.Upsert(entity.SavegameMetadata);
        }

        if (numberOfVersionsToKeep.HasValue)
        {
            var groups = Backups.GetAll().Include(b => b.SavegameMetadata).Where(b => b.GameId == gameId)
                .OrderByDescending(b => b.BackedUpOn)
                .GroupBy(b => b.SavegameMetadata.SlotNumber);
            foreach (var group in groups)
            {
                var lastDate = group.Select(g => g).Take(numberOfVersionsToKeep.Value).LastOrDefault()?.BackedUpOn;
                Backups.GetAll().Where(b => b.GameId == gameId).Include(b => b.SavegameMetadata)
                    .Where(b => b.BackedUpOn < lastDate).ExecuteDelete();
            }
        }
    }

    public async Task<List<FileBackupDto>> GetSavegamesByGameId(int gameId)
    {
        var backups = Backups.GetAll().Where(f => f.FileType == FileType.Savegame || f.FileType == FileType.SavegameStartOfLevel)
            .Where(f => f.GameId == gameId)
            .OrderByDescending(f => f.BackedUpOn);

        return await _mapper.ProjectTo<FileBackupDto>(backups).ToListAsync();
    }

    public async Task<List<string>> GetSavegameMd5sByGameId(int gameId)
    {
        return await Backups.GetAll().Where(f => f.FileType == FileType.Savegame || f.FileType == FileType.SavegameStartOfLevel)
            .Where(f => f.GameId == gameId).Select(sg => sg.Md5).ToListAsync();
    }

    public async Task UpdateSavegameStartOfLevel(FileBackupDto targetSaveGame)
    {
        var entityToUpdate = Backups.GetEntityById(targetSaveGame.Id);
        if (entityToUpdate == null)
            throw new InvalidOperationException();
        entityToUpdate.FileType = targetSaveGame.FileType;
        Backups.Update(entityToUpdate);
        await Backups.Commit();
    }

    public async Task DeleteFileBackupById(int id)
    {
        if (Backups.Delete(id))
            await Backups.Commit();
    }

    public void DeleteFileBackupsByGameId(int gameId, IEnumerable<FileType> fileTypes = null)
    {
        var byGameId = Backups.Get(b => b.GameId == gameId);
        if (fileTypes != null)
        {
            byGameId = byGameId.Where(b => fileTypes.Contains(b.FileType));
        }

        byGameId.ExecuteDelete();
    }

    public SavegameBackupDto GetSavegameById(int id)
    {
        var entity = Backups.GetEntityById(id);
        return _mapper.Map<SavegameBackupDto>(entity);
    }

    public async Task<List<SavegameBackupDto>> GetSavegameBackups()
    {
        var entities = Backups.Get().Include(b => b.SavegameMetadata)
            .Include(b => b.Game)
            .Where(b => b.FileType == FileType.Savegame || b.FileType == FileType.SavegameStartOfLevel);
        return await _mapper.ProjectTo<SavegameBackupDto>(entities).ToListAsync();
    }

    public async Task SyncSavegameMetadata(IEnumerable<SavegameBackupDto> dtos)
    {
        var savegameBackupDtos = dtos as SavegameBackupDto[] ?? dtos.ToArray();
        var idsToFind = savegameBackupDtos.Select(dto => dto.Id);
        var mappedEntities = Backups.GetAll().Include(b => b.SavegameMetadata)
            .Join(idsToFind, b => b.Id, i => i, (backup, i) => backup).ToList();
        var lookup = savegameBackupDtos.ToDictionary(dto => dto.Id);
        
        foreach (var entity in mappedEntities)
        {
            var backup = lookup[entity.Id];
            entity.SavegameMetadata.LevelName = backup.LevelName;
            entity.SavegameMetadata.SlotNumber = backup.SlotNumber;
            entity.SavegameMetadata.SaveNumber = backup.SaveNumber;
            entity.Md5 = backup.Md5;
            entity.BackedUpOn = DateTime.Now;
            
            Backups.Update(entity);
        }
        await Backups.Commit();
    }

    public async Task UpdateLaunchOptions(LaunchOptionsDto launchOptionsDto)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };
        var fileBackupQueryable = Backups.Get(b => b.GameId == launchOptionsDto.GameId)
            .Where(b => targetFileTypes.Contains(b.FileType));
        var gameToUpdate = Games.GetEntityById(launchOptionsDto.GameId);
        gameToUpdate.GameEngine = launchOptionsDto.GameEngine;
        Games.Upsert(gameToUpdate);

        // Update game executable
        var gameExecutable =
            fileBackupQueryable.Where(fb => fb.FileType == FileType.GameExecutable);
        if (await gameExecutable.AnyAsync())
        {
            await gameExecutable.ExecuteUpdateAsync(exe =>
                exe.SetProperty(f => f.FileName, launchOptionsDto.GameExecutable.FileName));
        }
        else
        {
            var newEntity = _mapper.Map<FileBackup>(launchOptionsDto.GameExecutable);
            Backups.Insert(newEntity);
        }

        // Update setup executable
        if (launchOptionsDto.SetupExecutable == null)
        {
            await fileBackupQueryable.Where(b => b.FileType == FileType.SetupExecutable).ExecuteDeleteAsync();
        }
        else
        {
            var setupQueryable = fileBackupQueryable.Where(fb => fb.FileType == FileType.SetupExecutable);
            if (await setupQueryable.AnyAsync())
            {
                await setupQueryable.ExecuteUpdateAsync(exe =>
                    exe.SetProperty(f => f.Arguments, launchOptionsDto.SetupExecutable.Arguments)
                        .SetProperty(f => f.FileName, launchOptionsDto.SetupExecutable.FileName));
            }
            else
            {
                var newSetupExe = _mapper.Map<FileBackup>(launchOptionsDto.SetupExecutable);
                Backups.Insert(newSetupExe);
            }
        }

        if (launchOptionsDto.CommunitySetupExecutable == null)
        {
            await fileBackupQueryable.Where(b => b.FileType == FileType.CommunitySetupExecutable).ExecuteDeleteAsync();
        }
        else
        {
            var communitySetupQueryable =
                fileBackupQueryable.Where(fb => fb.FileType == FileType.CommunitySetupExecutable);
            if (await communitySetupQueryable.AnyAsync())
            {
                await communitySetupQueryable.ExecuteUpdateAsync(exe =>
                    exe.SetProperty(f => f.FileName, launchOptionsDto.CommunitySetupExecutable.FileName));
            }
            else
            {
                var newCommunitySetupExe = _mapper.Map<FileBackup>(launchOptionsDto.CommunitySetupExecutable);
                Backups.Insert(newCommunitySetupExe);
            }
        }

        await Save();
    }
}