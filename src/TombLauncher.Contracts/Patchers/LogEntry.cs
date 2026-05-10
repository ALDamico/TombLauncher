namespace TombLauncher.Contracts.Patchers;

public class LogEntry
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public required string Message { get; set; }
    public LogSeverity Severity { get; set; }
}