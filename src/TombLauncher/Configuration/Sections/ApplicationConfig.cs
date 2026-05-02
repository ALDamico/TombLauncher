using Microsoft.Extensions.Logging;

namespace TombLauncher.Configuration.Sections;

public class ApplicationConfig : IApplicationConfig
{
    public string? ApplicationLanguage { get; set; }
    public string? DatabasePath { get; set; }
    public LogLevel? MinimumLogLevel { get; set; }
    public string? GitHubLink { get; set; }
    public string? WebsiteLink { get; set; }
}
