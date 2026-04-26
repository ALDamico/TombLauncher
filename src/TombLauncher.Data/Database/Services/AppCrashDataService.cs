using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class AppCrashDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly AppCrashMapper _mapper;

    public AppCrashDataService(TombLauncherDbContext dbContext, AppCrashMapper appCrashMapper)
    {
        _dbContext = dbContext;
        _mapper = appCrashMapper;
    }

    public void InsertAppCrash(Exception exception)
    {
        var exceptionDto = _mapper.ToDto(exception);
        var serializedException = JsonSerializer.Serialize(exceptionDto);
        var crash = new AppCrash()
        {
            Exception = serializedException,
            DateTime = DateTime.Now
        };
        _dbContext.AppCrashes.Add(crash);
        _dbContext.SaveChanges();
    }

    public AppCrashDto? GetNotNotifiedCrashes()
    {
        var unseenCrash = _dbContext.AppCrashes
            .Where(c => !c.WasNotified)
            .OrderByDescending(c => c.DateTime)
            .FirstOrDefault();
        return _mapper.ToDto(unseenCrash);
    }

    public async Task MarkAsNotified(int id)
    {
        var entityToUpdate = await _dbContext.AppCrashes.FindAsync(id);
        if (entityToUpdate == null) return;
        entityToUpdate.WasNotified = true;
        await _dbContext.SaveChangesAsync();
    }
}
