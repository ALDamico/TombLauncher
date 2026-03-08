using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Services;

public class AppCrashDataService
{
    private readonly TombLauncherDbContext _dbContext;
    private readonly IMapper _mapper;

    public AppCrashDataService(TombLauncherDbContext dbContext, MapperConfiguration mapperConfiguration)
    {
        _dbContext = dbContext;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public void InsertAppCrash(Exception exception)
    {
        var exceptionDto = _mapper.Map<ExceptionDto>(exception);
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
        return _mapper.Map<AppCrashDto>(unseenCrash);
    }

    public async Task MarkAsNotified(int id)
    {
        var entityToUpdate = await _dbContext.AppCrashes.FindAsync(id);
        if (entityToUpdate == null) return;
        entityToUpdate.WasNotified = true;
        await _dbContext.SaveChangesAsync();
    }
}
