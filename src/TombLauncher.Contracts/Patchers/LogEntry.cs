using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Patchers;

public class LogEntry
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required string Message { get; set; }
    public NotificationType Severity { get; set; }
}