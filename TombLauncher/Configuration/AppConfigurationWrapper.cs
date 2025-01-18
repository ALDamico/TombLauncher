﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
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
        get => User.ApplicationTheme.Coalesce(Defaults.ApplicationLanguage);
        set => User.ApplicationTheme = value.DefaultIfEquals(Defaults.ApplicationLanguage);
    }

    public bool? UseInternalViewer
    {
        get => User.UseInternalViewer.Coalesce(Defaults.UseInternalViewer);
        set => User.UseInternalViewer = value.DefaultIfEquals(Defaults.UseInternalViewer);
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

    public SaveGameBackupMode SaveGameBackupMode
    {
        get => User.SaveGameBackupMode.Coalesce(Defaults.SaveGameBackupMode);
        set => User.SaveGameBackupMode = value.DefaultIfEquals(Defaults.SaveGameBackupMode);
    }
}