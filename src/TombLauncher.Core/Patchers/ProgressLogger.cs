using TombLauncher.Contracts.Enums;
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

    public void Info(string message, params object[] args) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(args), Severity = NotificationType.Info });

    public void Warn(string message, params object[] args) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(args), Severity = NotificationType.Warning });

    public void Error(string message, params object[] args) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(args), Severity = NotificationType.Error });

    public void Success(string message, params object[] args) => _progress.Report(new LogEntry()
        { Message = message.GetLocalizedString(args), Severity = NotificationType.Success });
}