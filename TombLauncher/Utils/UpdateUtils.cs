using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.UI.Avalonia;
using TombLauncher.Configuration;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Services;
using TombLauncher.Updater;

namespace TombLauncher.Utils;

public static class UpdateUtils
{
    private static Dictionary<string, Func<IUIFactory>> _uiFactories;
    private static Dictionary<string, IRelayCommand<UpdateCommandPayload>> _notificationCmds;
    private static Dictionary<string, MaterialIconKind> _notificationIcons;

    private const string StableChannel = "stable";
    private const string PortableChannel = "portable";

    static UpdateUtils()
    {
        if (_uiFactories == null)
        {
            _uiFactories = new Dictionary<string, Func<IUIFactory>>()
            {
                { PortableChannel, () => null },
                { StableChannel, () => new UIFactory() }
            };
        }

        if (_notificationCmds == null)
        {
            _notificationCmds = new Dictionary<string, IRelayCommand<UpdateCommandPayload>>()
            {
                {
                    StableChannel,
                    new RelayCommand<UpdateCommandPayload>(payload =>
                        payload.Sparkle.ShowUpdateNeededUI(payload.Args.AppCastItems))
                },
                {
                    PortableChannel, new RelayCommand<UpdateCommandPayload>(payload =>
                    {
                        var settings = Ioc.Default.GetRequiredService<SettingsService>();
                        var gitHubLink = settings.GetGitHubLink();
                        var platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
                        platformSpecificFeatures.OpenUrl(gitHubLink);
                    })
                }
            };
        }

        if (_notificationIcons == null)
        {
            _notificationIcons = new Dictionary<string, MaterialIconKind>()
            {
                { StableChannel, MaterialIconKind.Download },
                { PortableChannel, MaterialIconKind.Github }
            };
        }
    }

    private static Func<IUIFactory> GetUiFactory(string channelName)
    {
        if (channelName == null)
        {
            channelName = StableChannel;
        }
        if (!_uiFactories.TryGetValue(channelName, out var func))
            func = _uiFactories[StableChannel];

        return func;
    }

    private static ICommand GetUpdateCommand(string channelName)
    {
        if (channelName == null)
        {
            channelName = StableChannel;
        }
        if (!_notificationCmds.TryGetValue(channelName, out var cmd))
            cmd = _notificationCmds[StableChannel];

        return cmd;
    }

    private static MaterialIconKind GetNotificationIcon(string channelName)
    {
        if (channelName == null)
        {
            channelName = StableChannel;
        }
        if (!_notificationIcons.TryGetValue(channelName, out var icon))
            icon = MaterialIconKind.Download;
        return icon;
    }

    public static UpdaterWorkersPayload AppCastWorkersFactory(IAppConfigurationWrapper appConfiguration)
    {
        UpdaterWorkersPayload updateWorkers = new UpdaterWorkersPayload()
        {
            LoggerToUse = new SerilogLogWriter()
        };
        if (appConfiguration.UpdaterUseLocalPaths)
        {
            updateWorkers.AppCastDataDownloader = new LocalFileAppCastDownloader() { UseLocalUriPath = true };
            updateWorkers.UpdateDownloader = new LocalFileDownloader(updateWorkers.LoggerToUse)
                { UseLocalUriPath = true };
        }
        else
        {
            updateWorkers.AppCastDataDownloader = new WebRequestAppCastDataDownloader(updateWorkers.LoggerToUse);
            updateWorkers.UpdateDownloader = new WebFileDownloader(updateWorkers.LoggerToUse);
        }

        if (appConfiguration.UpdateChannelName.IsNotNullOrWhiteSpace())
        {
            var appCastFilter = new ChannelAppCastFilter(updateWorkers.LoggerToUse)
            {
                ChannelSearchNames = [appConfiguration.UpdateChannelName],
            };
            var helper = new AppCastHelper()
            {
                AppCastFilter = appCastFilter
            };
            updateWorkers.AppCastHelper = helper;
        }

        updateWorkers.UiFactory = GetUiFactory(appConfiguration.UpdateChannelName);
        updateWorkers.UpdateCommand = GetUpdateCommand(appConfiguration.UpdateChannelName);
        updateWorkers.UpdateIcon = GetNotificationIcon(appConfiguration.UpdateChannelName);

        return updateWorkers;
    }
}