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