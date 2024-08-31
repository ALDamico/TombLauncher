using System;
using System.Collections.Generic;
using System.Text.Json;
using TombLauncher.Database.Repositories;
using TombLauncher.Dto.Extensions;
using TombLauncher.Models;

namespace TombLauncher.Database.UnitOfWork;

public class AppCrashUnitOfWork : UnitOfWorkBase
{
    public AppCrashUnitOfWork()
    {
        _dbContext = new TombLauncherDbContext();
        _appCrashes = GetRepository<AppCrash>();
    }
    private readonly TombLauncherDbContext _dbContext;

    private Lazy<EfRepository<AppCrash>> _appCrashes;
    public EfRepository<AppCrash> Crashes => _appCrashes.Value;

    public void InsertAppCrash(Exception exception)
    {
        var serializedException = JsonSerializer.Serialize(exception.ToDto());
        var crash = new AppCrash()
        {
            Exception = serializedException,
            DateTime = DateTime.Now
        };
        Crashes.Insert(crash);
        Save();
    }
}