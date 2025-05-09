﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration;

public interface IAppConfiguration
{
    string ApplicationLanguage { get; set; }
    string DatabasePath { get; set; }
    string ApplicationTheme { get; set; }
    bool? AskForConfirmationBeforeWalkthrough { get; set; }
    List<DownloaderConfiguration> Downloaders { get; set; }
    LogLevel? MinimumLogLevel { get; set; }
    string AppCastUrl { get; set; }
    string AppCastPublicKey { get; set; }
    bool UpdaterUseLocalPaths { get; set; }
    int? RandomGameMaxRerolls { get; set; }
    bool? BackupSavegamesEnabled { get; set; }
    int? NumberOfVersionsToKeep { get; set; }
    public int SavegameProcessingDelay { get; set; }
    string GitHubLink { get; set; }
    bool DefaultToGridView { get; set; }
    List<CheckableItem<string>> DocumentationPatterns { get; set; }
    List<CheckableItem<string>> DocumentationFolderExclusions { get; set; }
    string UpdateChannelName { get; set; }
    string WinePath { get; set; }
    string UnzipFallbackMethod { get; set; }
}