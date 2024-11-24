namespace TombLauncher.Data.Dto.Extensions;

public static class ExceptionExtensions
{
    public static ExceptionDto ToDto(this Exception e)
    {
        if (e == null) return null;
        return new ExceptionDto()
        {
            Message = e.Message,
            Source = e.Source,
            Type = e.GetType().FullName,
            HelpLink = e.HelpLink,
            StackTrace = e.StackTrace,
            InnerException = e.InnerException.ToDto()
        };
    }
}