using System.Text.Json;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Dto;
using TombLauncher.Data.Dto.Extensions;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.UnitOfWork;

public class AppCrashUnitOfWork : UnitOfWorkBase
{
    public AppCrashUnitOfWork()
    {
        _appCrashes = GetRepository<AppCrash>();
    }

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

    public AppCrashDto GetNotNotifiedCrashes()
    {
        var unseenCrashes = Crashes.Get(c => !c.WasNotified).OrderByDescending(c => c.DateTime).FirstOrDefault();
        if (unseenCrashes != null)
        {
            return new AppCrashDto()
            {
                Id = unseenCrashes.Id,
                DateTime = unseenCrashes.DateTime,
                ExceptionDto = JsonSerializer.Deserialize<ExceptionDto>(unseenCrashes.Exception)
            };
        }

        return null;
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