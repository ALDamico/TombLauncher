using Microsoft.Extensions.Logging;

namespace TombLauncher.Configuration.Sections;

public interface IApplicationConfig
{
    string? ApplicationLanguage { get; }
    string? DatabasePath { get; }
    LogLevel? MinimumLogLevel { get; }
    string? GitHubLink { get; }
}
