using System;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Database.Dto;
using TombLauncher.Database.Dto.Extensions;
using TombLauncher.Database.Entities;
using TombLauncher.Database.Repositories;
using TombLauncher.Models;

namespace TombLauncher.Database.UnitOfWork;

public class GamesUnitOfWork : IDisposable
{
    public GamesUnitOfWork()
    {
        _dbContext = new TombLauncherDbContext();
        _games = new Lazy<EfRepository<Game>>(() => new EfRepository<Game>(_dbContext));
        _dbContext.Database.Migrate();
    }

    private readonly TombLauncherDbContext _dbContext;
    private Lazy<EfRepository<Game>> _games;
    private bool _disposed;

    internal EfRepository<Game> Games
        => _games.Value;

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
    }

    private static Game ToGame(GameMetadataDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentException("Dto can't be null", nameof(dto));
        }
        Enum.TryParse<GameLength>(dto.Length, out var gameLength);
        Enum.TryParse<GameDifficulty>(dto.Difficulty, out var gameDifficulty);
        Enum.TryParse<GameEngine>(dto.GameEngine, out var gameEngine);
        return new Game()
        {
            Id = dto.Id,
            Author = dto.Author,
            Length = gameLength,
            Difficulty = gameDifficulty,
            Setting = dto.Setting,
            Title = dto.Title,
            ExecutablePath = dto.ExecutablePath,
            GameEngine = gameEngine,
            InstallDate = dto.InstallDate,
            InstallDirectory = dto.InstallDirectory,
            ReleaseDate = dto.ReleaseDate
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}