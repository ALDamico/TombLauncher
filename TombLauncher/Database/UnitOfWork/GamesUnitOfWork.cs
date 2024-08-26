using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Database.Repositories;
using TombLauncher.Dto;
using TombLauncher.Dto.Extensions;
using TombLauncher.Models;

namespace TombLauncher.Database.UnitOfWork;

public class GamesUnitOfWork : IDisposable
{
    public GamesUnitOfWork()
    {
        _dbContext = new TombLauncherDbContext();
        _games = new Lazy<EfRepository<Game>>(() => new EfRepository<Game>(_dbContext));
        _playSessions = new Lazy<EfRepository<PlaySession>>(() => new EfRepository<PlaySession>(_dbContext));
        _hashes = new Lazy<EfRepository<GameHashes>>(() => new EfRepository<GameHashes>(_dbContext));
        _dbContext.Database.Migrate();
    }

    private readonly TombLauncherDbContext _dbContext;
    private Lazy<EfRepository<Game>> _games;
    private Lazy<EfRepository<PlaySession>> _playSessions;
    private Lazy<EfRepository<GameHashes>> _hashes;
    private bool _disposed;

    internal EfRepository<Game> Games => _games.Value;

    internal EfRepository<PlaySession> PlaySessions => _playSessions.Value;
    internal EfRepository<GameHashes> Hashes => _hashes.Value;

    public void Save()
    {
        _dbContext.SaveChanges();
    }

    public void UpsertGame(GameMetadataDto game)
    {
        var entity = ToGame(game);
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

    private static Game ToGame(GameMetadataDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Dto can't be null", nameof(dto));
        }
        
        return new Game()
        {
            Id = dto.Id,
            Author = dto.Author,
            Length = dto.Length,
            Difficulty = dto.Difficulty,
            Setting = dto.Setting,
            Title = dto.Title,
            ExecutablePath = dto.ExecutablePath,
            GameEngine = dto.GameEngine,
            InstallDate = dto.InstallDate,
            InstallDirectory = dto.InstallDirectory,
            ReleaseDate = dto.ReleaseDate,
            Description = dto.Description,
            Guid = dto.Guid,
            TitlePic = dto.TitlePic
        };
    }

    private static GameHashes ToGameHashes(GameHashDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Dto can't be null", nameof(dto));
        }

        return new GameHashes()
        {
            Id = dto.Id,
            FileName = dto.FileName,
            GameId = dto.GameId,
            Md5Hash = dto.Md5Hash
        };
    }
    public void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }

        _disposed = true;
    }

    public List<GameMetadataDto> GetGames()
    {
        return Games.GetAll().AsEnumerable().ToDtos().ToList();
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
                GameMetadata = game.ToDto(),
                TotalPlayTime = TimeSpan.Zero
            };

            if (thisGamePlaySessions.Any())
            {
                gameWithStatsDto.LastPlayed = thisGamePlaySessions.Max(ps => ps.StartDate);
                gameWithStatsDto.TotalPlayTime =
                    TimeSpan.FromTicks(thisGamePlaySessions.Select(ps => (ps.EndDate - ps.StartDate).Ticks).Sum());
            }
            
            outputList.Add(gameWithStatsDto);
        }

        return outputList;
    }

    public List<PlaySessionDto> GetPlaySessionsByGameId(int gameId)
    {
        return PlaySessions.Get(ps => ps.Game.Id == gameId).AsEnumerable().ToDtos().ToList();
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
        return lastPlaysession.ToDto();
    }

    public List<GameHashDto> GetHashes(GameMetadataDto dto)
    {
        return Hashes.Get(h => h.GameId == dto.Id).AsEnumerable().ToDtos().ToList();
    }

    public bool ExistsHashes(List<GameHashDto> computedHashes)
    {
        var hashesRepo = Hashes.GetAll();
        var tempQueryable = computedHashes.Select(ToGameHashes).AsQueryable();
        var joined = hashesRepo.AsEnumerable().Join(tempQueryable, gh => gh.FileName + "#" + gh.Md5Hash,
            tmp => tmp.FileName + "#" + tmp.Md5Hash,
            (gh, tmp) => gh);
        var matches = joined.GroupBy(m => m.GameId).Select(g => new {Id = g.Key, Count = g.Count()}).ToList();
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Any(m => m.Count == computedHashes.Count))
        {
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}