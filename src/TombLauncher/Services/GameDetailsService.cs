using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Navigation;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.MessageBoxes;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameDetailsService : IViewService
{
    public GameDetailsService(ViewServiceContext viewContext, GameDataService gameDataService, GameLinkDataService gameLinkDataService,
        IPlatformSpecificFeatures platformSpecificFeatures, ISettingsProvider settingsProvider,
        TombRaiderEngineDetector engineDetector, ILogger<GameDetailsService> logger)
    {
        _logger = logger;
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _gameLinkDataService = gameLinkDataService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _settingsProvider = settingsProvider;
        _engineDetector = engineDetector;
    }

    private readonly ILogger<GameDetailsService> _logger;

    public ViewServiceContext ViewContext { get; }
    private readonly GameDataService _gameDataService;
    private readonly GameLinkDataService _gameLinkDataService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper Mapper => ViewContext.Mapper;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ISettingsProvider _settingsProvider;
    private readonly TombRaiderEngineDetector _engineDetector;

    public void InitializeSettings(GameDetailsViewModel target)
    {
        var gameDetailsSettings = _settingsProvider.GetGameDetailsSettings();
        target.AskForConfirmationBeforeOpeningWalkthrough = gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        target.EnabledPatterns = gameDetailsSettings.EnabledPatterns.Select(p => p.Value).ToList();
        target.IgnoredFolders = gameDetailsSettings.ExcludedFolders.Select(p => p.Value).ToList();
    }

    public void OpenGameFolder(string gameFolder)
    {
        _platformSpecificFeatures.OpenGameFolder(gameFolder);
    }

    public async Task FetchLinks(GameDetailsViewModel game, LinkType linkType)
    {
        var tf = new TaskFactory();
        var links = await tf.StartNew(() =>
        {
            var links = _gameLinkDataService.GetLinks(game.Game.GameMetadata.Id, linkType);
            return Mapper.Map<List<GameLinkViewModel>>(links);
        });
        game.WalkthroughLinks = links.ToObservableCollection();
    }

    public async Task OpenWalkthrough(string link, bool askConfirmation)
    {
        if (askConfirmation)
        {
            var confirmation = await MessageBoxService.ShowLocalized("Confirm",
                "Are you sure you want to read the walkthrough?",
                 MsgBoxButton.YesNo, MsgBoxImage.Question);
            if (confirmation.ButtonResult == MsgBoxButtonResult.No)
                return;
        }

        try
        {
            _platformSpecificFeatures.OpenUrl(link);
        }
        catch (SystemException)
        {
            await MessageBoxService.Show(new UrlOpenErrorMessageBox()
            {
                MsgBoxImage = MsgBoxImage.Error,
                Message =
                    "AN_ERROR_OCCURRED_WHILE_TRYING_TO_OPEN_THE_WALKTHR".GetLocalizedString(link),
                MsgBoxTitle = "ERROR".GetLocalizedString(),
                TargetUrl = link
            });
        }
    }

    public async Task OpenSavegameList(GameDetailsViewModel game)
    {
        await NavigationManager.NavigateTo<SavegameListViewModel>(game.Game.GameMetadata);
    }

    public void OpenLaunchOptions(GameDetailsViewModel gameDetailsViewModel)
    {
        DialogService.ShowDialog(new LaunchOptionsDialogViewModel(_engineDetector) { TargetGame = gameDetailsViewModel.Game.GameMetadata }, SaveLaunchOptions);
    }

    private async void SaveLaunchOptions(LaunchOptionsDialogViewModel vm)
    {
        try
        {
            var gameMetadata = vm.TargetGame;
            var currentPage = NavigationManager.CurrentPage as PageViewModel;
            if (currentPage != null)
            {
                using (currentPage.BusyScope("SAVING_LAUNCH_OPTIONS".GetLocalizedString()))
                {
                    var launchOptionsDto = Mapper.Map<LaunchOptionsDto>(vm);

                    gameMetadata.ExecutablePath = vm.GameExecutable;
                    gameMetadata.SetupExecutable = vm.SetupExecutable;
                    gameMetadata.SetupExecutableArgs = vm.SetupArgs;
                    gameMetadata.CommunitySetupExecutable = vm.CustomSetupExecutable;

                    await _gameDataService.UpdateLaunchOptions(launchOptionsDto);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error saving launch options.");
        }
    }

    public List<FileInfo> GetDocumentationFiles(string containingFolder, List<string> patterns, List<string> excludedFolders)
    {
        var enumerationOptions = _platformSpecificFeatures.GetEnumerationOptions();
        return patterns.Select(p => Directory.GetFiles(containingFolder, p, enumerationOptions))
            .SelectMany(f => f)
            .Where(f => excludedFolders.All(dir => !f.Contains(dir)))
            .Select(f => new FileInfo(f))
            .ToList();
    }
}