using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration;

public class AppConfiguration : IAppConfiguration
{
    public string ApplicationLanguage { get; set; }
    public string DatabasePath { get; set; }
    public string ApplicationTheme { get; set; }
    public bool? UseInternalViewer { get; set; }
    public bool? AskForConfirmationBeforeWalkthrough { get; set; }
    public List<DownloaderConfiguration> Downloaders { get; set; }
    public LogLevel? MinimumLogLevel { get; set; }
    public string AppCastUrl { get; set; }
    public string AppCastPublicKey { get; set; }
    public bool UpdaterUseLocalPaths { get; set; }
}