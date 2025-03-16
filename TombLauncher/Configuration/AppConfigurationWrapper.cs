using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Configuration;

public class AppConfigurationWrapper : IAppConfigurationWrapper
{
    public IAppConfiguration Defaults { get; set; } = new AppConfiguration();
    public IAppConfiguration User { get; set; } = new AppConfiguration();

    public string ApplicationLanguage
    {
        get => User.ApplicationLanguage.Coalesce(Defaults.ApplicationLanguage);
        set => User.ApplicationLanguage = value.DefaultIfEquals(Defaults.ApplicationLanguage);
    }

    public string DatabasePath
    {
        get => User.DatabasePath.Coalesce(Defaults.DatabasePath);
        set => User.DatabasePath = value.DefaultIfEquals(Defaults.DatabasePath);
    }

    public string ApplicationTheme
    {
        get => User.ApplicationTheme.Coalesce(Defaults.ApplicationTheme);
        set => User.ApplicationTheme = value.DefaultIfEquals(Defaults.ApplicationTheme);
    }

    public bool? AskForConfirmationBeforeWalkthrough
    {
        get => User.AskForConfirmationBeforeWalkthrough.Coalesce(Defaults.AskForConfirmationBeforeWalkthrough);
        set => User.AskForConfirmationBeforeWalkthrough =
            value.DefaultIfEquals(Defaults.AskForConfirmationBeforeWalkthrough);
    }

    public List<DownloaderConfiguration> Downloaders
    {
        get => User.Downloaders.Coalesce(Defaults.Downloaders);
        set
        {
            if (value.SequenceEqual(Defaults.Downloaders, new DownloaderConfiguration()))
            {
                User.Downloaders = null;
                return;
            }

            User.Downloaders = value;
        }
    }

    public LogLevel? MinimumLogLevel
    {
        get => User.MinimumLogLevel.Coalesce(Defaults.MinimumLogLevel);
        set => User.MinimumLogLevel = value.DefaultIfEquals(Defaults.MinimumLogLevel);
    }

    public string AppCastUrl
    {
        get => Defaults.AppCastUrl;
        set => Defaults.AppCastUrl = value;
    }

    public string AppCastPublicKey
    {
        get => Defaults.AppCastPublicKey;
        set => Defaults.AppCastPublicKey = value;
    }

    public bool UpdaterUseLocalPaths
    {
        get => Defaults.UpdaterUseLocalPaths;
        set => Defaults.UpdaterUseLocalPaths = value;
    }

    public int? RandomGameMaxRerolls
    {
        get => User.RandomGameMaxRerolls.Coalesce(Defaults.RandomGameMaxRerolls);
        set => User.RandomGameMaxRerolls = value.DefaultIfEquals(Defaults.RandomGameMaxRerolls);
    }

    public bool? BackupSavegamesEnabled
    {
        get => User.BackupSavegamesEnabled.Coalesce(Defaults.BackupSavegamesEnabled);
        set => User.BackupSavegamesEnabled = value.DefaultIfEquals(Defaults.BackupSavegamesEnabled);
    }

    public int? NumberOfVersionsToKeep
    {
        get => User.NumberOfVersionsToKeep.Coalesce(Defaults.NumberOfVersionsToKeep);
        set => User.NumberOfVersionsToKeep = value.DefaultIfEquals(Defaults.NumberOfVersionsToKeep);
    }

    public int SavegameProcessingDelay
    {
        get => User.SavegameProcessingDelay.Coalesce(Defaults.SavegameProcessingDelay);
        set => User.SavegameProcessingDelay = value.DefaultIfEquals(Defaults.SavegameProcessingDelay);
    }

    public string GitHubLink
    {
        get => Defaults.GitHubLink;
        set => Defaults.GitHubLink = value;
    }

    public bool DefaultToGridView
    {
        get => User.DefaultToGridView.Coalesce(Defaults.DefaultToGridView);
        set => User.DefaultToGridView = value.DefaultIfEquals(DefaultToGridView);
    }

    public List<CheckableItem<string>> DocumentationPatterns
    {
        get => CollectionUtils.GetSharedItems(Defaults.DocumentationPatterns, User.DocumentationPatterns);
        set => User.DocumentationPatterns = value.Except(Defaults.DocumentationPatterns).ToList();
    }

    public List<CheckableItem<string>> DocumentationFolderExclusions
    {
        get => CollectionUtils.GetSharedItems(Defaults.DocumentationFolderExclusions,
            User.DocumentationFolderExclusions);
        set => User.DocumentationFolderExclusions = value.Except(Defaults.DocumentationFolderExclusions).ToList();
    }

    public string UpdateChannelName
    {
        get => User.UpdateChannelName.Coalesce(Defaults.UpdateChannelName);
        set => User.UpdateChannelName = value.DefaultIfEquals(Defaults.UpdateChannelName);
    }

    public string WinePath
    {
        get => User.WinePath.Coalesce(Defaults.WinePath);
        set => User.WinePath = value.DefaultIfEquals(Defaults.WinePath);
    }

    public string UnzipFallbackMethod
    {
        get => User.UnzipFallbackMethod.Coalesce(Defaults.UnzipFallbackMethod);
        set => value.DefaultIfEquals(Defaults.UnzipFallbackMethod);
    }
}