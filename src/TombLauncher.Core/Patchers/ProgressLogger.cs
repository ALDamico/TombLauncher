using TombLauncher.Contracts.Patchers;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.Core.Patchers;

public class ProgressLogger
{
    private readonly IProgress<LogEntry> _progress;

    public ProgressLogger(IProgress<LogEntry> progress)
    {
        _progress = progress;
    }

    public void Info(string message) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(), Severity = LogSeverity.Information });

    public void Warn(string message) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(), Severity = LogSeverity.Warning });

    public void Error(string message) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(), Severity = LogSeverity.Error });

    public void Success(string message) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(), Severity = LogSeverity.Success });
}