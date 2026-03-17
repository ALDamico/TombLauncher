using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using NetSparkleUpdater.SignatureVerifiers;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Updater;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Notifications;

namespace TombLauncher.Services;

public class UpdateService
{
    private const string StableChannel = "stable";
    private const string PortableChannel = "portable";

    private readonly IAppConfiguration _appConfiguration;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ISettingsProvider _settingsProvider;
    private readonly NotificationService _notificationService;
    private readonly ILocalizationManager _localizationManager;

    private SparkleUpdater? _sparkle;

    public UpdateService(
        IAppConfiguration appConfiguration,
        IPlatformSpecificFeatures platformSpecificFeatures,
        ISettingsProvider settingsProvider,
        NotificationService notificationService,
        ILocalizationManager localizationManager)
    {
        _appConfiguration = appConfiguration;
        _platformSpecificFeatures = platformSpecificFeatures;
        _settingsProvider = settingsProvider;
        _notificationService = notificationService;
        _localizationManager = localizationManager;
    }

    public async Task StartAsync()
    {
        var channelName = _appConfiguration.Updater.UpdateChannelName;
        var logger = new SerilogLogWriter();

        IAppCastDataDownloader appCastDataDownloader;
        IUpdateDownloader updateDownloader;

        if (_appConfiguration.Updater.UpdaterUseLocalPaths)
        {
            appCastDataDownloader = new LocalFileAppCastDownloader { UseLocalUriPath = true };
            updateDownloader = new LocalFileDownloader(logger) { UseLocalUriPath = true };
        }
        else
        {
            appCastDataDownloader = new WebRequestAppCastDataDownloader(logger);
            updateDownloader = new WebFileDownloader(logger);
        }

        AppCastHelper? appCastHelper = null;
        if (channelName.IsNotNullOrWhiteSpace())
        {
            var filter = new ChannelAppCastFilter(logger)
            {
                ChannelSearchNames = [channelName]
            };
            appCastHelper = new AppCastHelper { AppCastFilter = filter };
        }

        _sparkle = new SparkleUpdater(
            _appConfiguration.Updater.AppCastUrl ?? string.Empty,
            new Ed25519Checker(SecurityMode.Strict, _appConfiguration.Updater.AppCastPublicKey))
        {
            // Intentional: portable users are directed to GitHub via UpdateCommand; SparkleUpdater's UI is never invoked.
            UIFactory = GetUiFactory(channelName),
            AppCastDataDownloader = appCastDataDownloader,
            UpdateDownloader = updateDownloader,
            LogWriter = logger,
            UserInteractionMode = UserInteractionMode.NotSilent,
        };

        if (appCastHelper != null)
        {
            _sparkle.AppCastHelper = appCastHelper;
            _sparkle.AppCastHelper.SetupAppCastHelper(appCastDataDownloader, _appConfiguration.Updater.AppCastUrl!, AppUtils.GetApplicationVersion()?.ToString(), _sparkle.SignatureVerifier, _sparkle.LogWriter);
        }

        _sparkle.UpdateDetected += OnUpdateDetected;
        await _sparkle.StartLoop(true, true);
    }

    private void OnUpdateDetected(object? sender, UpdateDetectedEventArgs args)
    {
        var channelName = _appConfiguration.Updater.UpdateChannelName;
        var payload = new UpdateCommandPayload(_sparkle!, args);

        _ = _notificationService.AddNotificationAsync(new NotificationViewModel
        {
            Content = new StringNotificationViewModel
            {
                Text = _localizationManager.GetLocalizedString("UPDATE_AVAILABLE_NOTIFICATION", args.LatestVersion.Version ?? "")
            },
            IsDismissable = true,
            IsCancelable = false,
            IsOpenable = true,
            OpenCommand = GetUpdateCommand(channelName),
            OpenIcon = GetNotificationIcon(channelName),
            OpenCmdParam = payload
        });
    }

    private IUIFactory? GetUiFactory(string? channelName) => channelName switch
    {
        PortableChannel => null,
        _ => new TombLauncherUIFactory()
    };

    private ICommand GetUpdateCommand(string? channelName) => channelName switch
    {
        PortableChannel => new RelayCommand<UpdateCommandPayload>(_ =>
        {
            var gitHubLink = _settingsProvider.GetApplicationSettings().GitHubLink;
            _platformSpecificFeatures.OpenUrl(gitHubLink);
        }),
        _ => new RelayCommand<UpdateCommandPayload>(payload =>
        {
            if (payload?.Args != null)
                payload.Sparkle.ShowUpdateNeededUI(payload.Args.AppCastItems);
        })
    };

    private static PackIconRemixIconKind GetNotificationIcon(string? channelName) => channelName switch
    {
        PortableChannel => PackIconRemixIconKind.GithubLine,
        _ => PackIconRemixIconKind.Download2Line
    };
}
