using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class GameDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public GameDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public GameMetadataDto GetGameById(int id)
    {
        var entity = _dbContext.Games.Find(id);
        return _mapper.Map<GameMetadataDto>(entity);
    }

    public async Task UpsertGame(IGameMetadata game)
    {
        Game? entity;
        if (game.Id == 0)
        {
            entity = _mapper.Map<Game>(game);
            _dbContext.Games.Add(entity);
        }
        else
        {
            if (game.InstallDirectory.IsNullOrWhiteSpace())
                throw new InvalidOperationException("Attempting to insert a game with an unknown install directory!");
            entity = await _dbContext.Games.FindAsync(game.Id);
            if (entity == null)
                throw new InvalidOperationException($"Game with id {game.Id} not found!");
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
            entity.InstallDirectory = game.InstallDirectory!; // If we've reached this point, InstallDirectory HAS to be 
            entity.IsInstalled = game.IsInstalled;
            entity.ReleaseDate = game.ReleaseDate;
            entity.TitlePic = game.TitlePic;
            entity.AuthorFullName = game.AuthorFullName;
            entity.IsFavourite = game.IsFavourite;
            entity.IsCompleted = game.IsCompleted;
            _dbContext.Games.Update(entity);
        }

        await _dbContext.SaveChangesAsync();
        game.Id = entity.Id;
    }

    public async Task MarkGameAsUninstalled(int id)
    {
        var game = _dbContext.Games.Find(id)!;
        game.IsInstalled = false;
        _dbContext.Games.Update(game);
        await _dbContext.SaveChangesAsync();
    }

    public List<GameMetadataDto> GetGames()
    {
        return _mapper.Map<List<GameMetadataDto>>(_dbContext.Games.ToList());
    }

    public async Task<List<GameWithStatsDto>> GetGamesWithStats(bool installedOnly = false)
    {
        var outputList = new List<GameWithStatsDto>();
        var playSessions = _dbContext.PlaySession.ToLookup(ps => ps.GameId);

        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        IQueryable<Game> games = _dbContext.Games
            .Include(g => g.FileBackups.Where(b => targetFileTypes.Contains(b.FileType)));
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

    public async Task<GameWithStatsDto> GetGameWithStats(int id)
    {
        var playSessions = _dbContext.PlaySession.Where(ps => ps.GameId == id).ToList();
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

    public GameWithStatsDto? GetLatestPlayedGame()
    {
        var playSessionsQuery = _dbContext.PlaySession
            .Include(ps => ps.Game).ThenInclude(g => g!.FileBackups);
        var entity = playSessionsQuery
            .FirstOrDefault(ps => ps.StartDate == playSessionsQuery.Max(p => p.StartDate));

        if (entity == null)
            return null;

        var metadataDto = _mapper.Map<GameMetadataDto>(entity.Game);
        var allPlaySessions = playSessionsQuery
            .Where(ps => ps.GameId == entity.GameId).ToList()
            .Select(ps => ps.EndDate - ps.StartDate).Sum();

        return new GameWithStatsDto()
        {
            GameMetadata = metadataDto,
            LastPlayed = entity.StartDate,
            TotalPlayedTime = allPlaySessions
        };
    }

    public List<GameWithStatsDto> GetRecentlyPlayedGames(int count)
    {
        var playSessions = _dbContext.PlaySession
            .Include(ps => ps.Game).ThenInclude(g => g!.FileBackups)
            .AsEnumerable()
            .GroupBy(ps => ps.GameId)
            .Select(g => new
            {
                GameId = g.Key,
                Game = g.First().Game,
                LastPlayed = g.Max(ps => ps.StartDate),
                TotalPlayedTime = TimeSpan.FromTicks(g.Sum(ps => (ps.EndDate - ps.StartDate).Ticks))
            })
            .OrderByDescending(g => g.LastPlayed)
            .Skip(1) // Skip the latest (already shown in "Resume Last Game")
            .Take(count)
            .ToList();

        return playSessions.Select(g => new GameWithStatsDto
        {
            GameMetadata = _mapper.Map<GameMetadataDto>(g.Game),
            LastPlayed = g.LastPlayed,
            TotalPlayedTime = g.TotalPlayedTime
        }).ToList();
    }

    public List<GameWithStatsDto> GetFavouriteGames(int count)
    {
        var favouriteGames = _dbContext.Games
            .Include(g => g.FileBackups)
            .Where(g => g.IsFavourite)
            .ToList();

        var gameIds = favouriteGames.Select(g => g.Id).ToHashSet();

        var playSessionStats = _dbContext.PlaySession
            .Where(ps => gameIds.Contains(ps.GameId))
            .AsEnumerable()
            .GroupBy(ps => ps.GameId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    LastPlayed = g.Max(ps => ps.StartDate),
                    TotalPlayedTime = TimeSpan.FromTicks(g.Sum(ps => (ps.EndDate - ps.StartDate).Ticks))
                });

        return favouriteGames
            .OrderByDescending(g => playSessionStats.TryGetValue(g.Id, out var stat) ? stat.LastPlayed : DateTime.MinValue)
            .Take(count)
            .Select(g => new GameWithStatsDto
            {
                GameMetadata = _mapper.Map<GameMetadataDto>(g),
                LastPlayed = playSessionStats.TryGetValue(g.Id, out var sessionStat) ? sessionStat.LastPlayed : null,
                TotalPlayedTime = playSessionStats.TryGetValue(g.Id, out var playSessionStat) ? playSessionStat.TotalPlayedTime : TimeSpan.Zero
            }).ToList();
    }

    public async Task UpdateLaunchOptions(LaunchOptionsDto launchOptionsDto)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        var fileBackupQueryable = _dbContext.FileBackups
            .Where(b => b.GameId == launchOptionsDto.GameId)
            .Where(b => targetFileTypes.Contains(b.FileType));

        var gameToUpdate = (await _dbContext.Games.FindAsync(launchOptionsDto.GameId))!;
        gameToUpdate.GameEngine = launchOptionsDto.GameEngine;
        _dbContext.Games.Update(gameToUpdate);

        // Update game executable
        var gameExecutable = fileBackupQueryable.Where(fb => fb.FileType == FileType.GameExecutable);
        if (await gameExecutable.AnyAsync())
        {
            await gameExecutable.ExecuteUpdateAsync(exe =>
                exe.SetProperty(f => f.FileName, launchOptionsDto.GameExecutable.FileName));
        }
        else
        {
            var newEntity = _mapper.Map<FileBackup>(launchOptionsDto.GameExecutable);
            _dbContext.FileBackups.Add(newEntity);
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
                _dbContext.FileBackups.Add(newSetupExe);
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
                _dbContext.FileBackups.Add(newCommunitySetupExe);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<QuickStatsDto> GetQuickStatsAsync()
    {
        var games = await _dbContext.Games.ToListAsync();
        var playSessions = await _dbContext.PlaySession.ToListAsync();
        var totalPlayTime = playSessions.Aggregate(TimeSpan.Zero,
            (sum, ps) => sum + (ps.EndDate - ps.StartDate));

        return new QuickStatsDto
        {
            InstalledGamesCount = games.Count(g => g.IsInstalled),
            CompletedGamesCount = games.Count(g => g.IsCompleted),
            FavouriteGamesCount = games.Count(g => g.IsFavourite),
            TotalPlayTime = totalPlayTime
        };
    }

    private async Task<GameMetadataDto> GetGameWithExecutables(int id)
    {
        var targetFileTypes = new List<FileType>()
        {
            FileType.GameExecutable,
            FileType.SetupExecutable,
            FileType.CommunitySetupExecutable
        };

        var game = await _dbContext.Games
            .Include(g => g.FileBackups.Where(f => targetFileTypes.Contains(f.FileType)))
            .Include(g => g.InstalledFromLink)
            .SingleAsync(g => g.Id == id);

        return _mapper.Map<GameMetadataDto>(game);
    }

    public async Task SetInstalledFromLink(int gameId, int linkId)
    {
        var game = _dbContext.Games.Find(gameId);
        if (game == null) return;
        _dbContext.Entry(game).Property("InstalledFromLinkId").CurrentValue = linkId;
        await _dbContext.SaveChangesAsync();
    }
}
