using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Database.Repositories;
using TombLauncher.Dto;
using TombLauncher.Dto.Extensions;
using TombLauncher.Models.Models;

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