using Newtonsoft.Json;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class AppCrashMapper
{
    public ExceptionDto? ToDto(Exception? exception)
    {
        if (exception == null) return null;
        return new ExceptionDto()
        {
            Message = exception.Message,
            Source = exception.Source ?? "",
            StackTrace = exception.StackTrace ?? "",
            Type = exception.GetType().FullName ?? exception.GetType().Name,
            HelpLink = exception.HelpLink,
            InnerException = ToDto(exception.InnerException)
        };
    }

    public AppCrashDto? ToDto(AppCrash? appCrash)
    {
        if (appCrash == null) 
            return null;
        return new AppCrashDto()
        {
            ExceptionDto = JsonConvert.DeserializeObject<ExceptionDto>(appCrash.Exception) ?? null!,
            DateTime = appCrash.DateTime,
            Id = appCrash.Id
        };
    }
}