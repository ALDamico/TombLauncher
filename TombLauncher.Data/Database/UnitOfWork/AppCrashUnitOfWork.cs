using System.Text.Json;
using AutoMapper;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.UnitOfWork;

public class AppCrashUnitOfWork : UnitOfWorkBase
{
    public AppCrashUnitOfWork(MapperConfiguration mapperConfiguration)
    {
        _mapper = mapperConfiguration.CreateMapper();
        _appCrashes = GetRepository<AppCrash>();
    }

    private IMapper _mapper;

    private Lazy<EfRepository<AppCrash>> _appCrashes;
    public EfRepository<AppCrash> Crashes => _appCrashes.Value;

    public void InsertAppCrash(Exception exception)
    {
        var exceptionDto = _mapper.Map<ExceptionDto>(exception);
        
        var serializedException = JsonSerializer.Serialize(exceptionDto);
        var crash = new AppCrash()
        {
            Exception = serializedException,
            DateTime = DateTime.Now
        };
        Crashes.Insert(crash);
        Save();
    }

    public AppCrashDto GetNotNotifiedCrashes()
    {
        var unseenCrashes = Crashes.Get(c => !c.WasNotified).OrderByDescending(c => c.DateTime).FirstOrDefault();
        var mapped = _mapper.Map<AppCrashDto>(unseenCrashes);
        return mapped;
    }

    public void MarkAsNotified(int id)
    {
        var entityToUpdate = Crashes.GetEntityById(id);
        if (entityToUpdate == null) return;
        entityToUpdate.WasNotified = true;
        Crashes.Update(entityToUpdate);
        Save();
    }
}