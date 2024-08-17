using System;
using Microsoft.EntityFrameworkCore;
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

    public EfRepository<Game> Games
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

        this._disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}