using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration;

public interface IAppConfiguration
{
    string ApplicationLanguage { get; set; }
    string DatabasePath { get; set; }
    string ApplicationTheme { get; set; }
    bool? UseInternalViewer { get; set; }
    bool? AskForConfirmationBeforeWalkthrough { get; set; }
    List<DownloaderConfiguration> Downloaders { get; set; }
    LogLevel? MinimumLogLevel { get; set; }
    string AppCastUrl { get; set; }
    string AppCastPublicKey { get; set; }
    bool UpdaterUseLocalPaths { get; set; }
    int? RandomGameMaxRerolls { get; set; }
    bool BackupSavegamesEnabled { get; set; }
    int? NumberOfVersionsToKeep { get; set; }
    public int SavegameProcessingDelay { get; set; }
    string GitHubLink { get; set; }
}