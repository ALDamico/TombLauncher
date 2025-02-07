using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Data.Database.Repositories;

namespace TombLauncher.Data.Database.UnitOfWork;

public abstract class UnitOfWorkBase : IDisposable, IAsyncDisposable
{
    public UnitOfWorkBase()
    {
        DbContext = Ioc.Default.GetRequiredService<TombLauncherDbContext>();
        DbContext.Database.Migrate();
    }
    protected readonly TombLauncherDbContext DbContext;
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
    
    public async Task Save()
    {
        await DbContext.SaveChangesAsync();
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