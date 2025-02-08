namespace TombLauncher.Core.Exceptions;

public class AppRestartRequestedException : Exception
{
    public AppRestartRequestedException(int crashId)
    {
        CrashId = crashId;
    }
    public int CrashId { get; set; }
}