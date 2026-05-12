using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Models;

public class KbUpdateResult
{
    private KbUpdateResult(NotificationType severity, string message, Exception? exception = null)
    {
        Severity = severity;
        Message = message;
        Exception = exception;
    }
    public NotificationType Severity { get;  }
    public string Message { get;  }
    public Exception? Exception { get;  }

    public static KbUpdateResult Success() => new KbUpdateResult(NotificationType.Success, "");

    public static KbUpdateResult Error(string message, Exception? ex = null) =>
        new KbUpdateResult(NotificationType.Error, message, ex);
}