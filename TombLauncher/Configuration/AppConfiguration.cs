using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration;

public class AppConfiguration : IAppConfiguration
{
    public string ApplicationLanguage { get; set; }
    public string DatabasePath { get; set; }
    public string ApplicationTheme { get; set; }
    public bool? AskForConfirmationBeforeWalkthrough { get; set; }
    public List<DownloaderConfiguration> Downloaders { get; set; }
    public LogLevel? MinimumLogLevel { get; set; }
    public string AppCastUrl { get; set; }
    public string AppCastPublicKey { get; set; }
    public bool UpdaterUseLocalPaths { get; set; }
    public int? RandomGameMaxRerolls { get; set; }
    public bool? BackupSavegamesEnabled { get; set; }
    public int? NumberOfVersionsToKeep { get; set; }
    public int SavegameProcessingDelay { get; set; }
    public string GitHubLink { get; set; }
    public bool DefaultToGridView { get; set; }
    public List<CheckableItem<string>> DocumentationPatterns { get; set; }
    public List<CheckableItem<string>> DocumentationFolderExclusions { get; set; }
    public string UpdateChannelName { get; set; }
    public string WinePath { get; set; }
}