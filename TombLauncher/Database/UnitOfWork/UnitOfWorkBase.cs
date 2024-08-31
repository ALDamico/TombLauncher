﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Database.Repositories;

namespace TombLauncher.Database.UnitOfWork;

public abstract class UnitOfWorkBase : IDisposable, IAsyncDisposable
{
    public UnitOfWorkBase()
    {
        DbContext = new TombLauncherDbContext();
        DbContext.Database.Migrate();
    }
    protected TombLauncherDbContext DbContext;
    private bool _disposed;
    public void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DbContext.Dispose();
            }
        }

        _disposed = true;
    }
    
    public void Save()
    {
        DbContext.SaveChanges();
    }

    protected Lazy<EfRepository<T>> GetRepository<T>() where T : class
    {
        return new Lazy<EfRepository<T>>(() => new EfRepository<T>(DbContext));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (DbContext != null) await DbContext.DisposeAsync();
    }
}