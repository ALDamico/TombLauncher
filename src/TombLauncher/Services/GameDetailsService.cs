using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.Mapping;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.MessageBoxes;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameDetailsService : IViewService
{
    public GameDetailsService(ViewServiceContext viewContext, GameDataService gameDataService, GameLinkDataService gameLinkDataService,
        IPlatformSpecificFeatures platformSpecificFeatures, ISettingsProvider settingsProvider,
        TombRaiderEngineDetector engineDetector, ILogger<GameDetailsService> logger, 
        GameLinkDtoMapper mapper, LaunchOptionsMapper launchOptionsMapper)
    {
        _logger = logger;
        _mapper = mapper;
        _launchOptionsMapper = launchOptionsMapper;
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _gameLinkDataService = gameLinkDataService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _settingsProvider = settingsProvider;
        _engineDetector = engineDetector;
    }

    private readonly ILogger<GameDetailsService> _logger;
    private readonly GameLinkDtoMapper _mapper;
    private readonly LaunchOptionsMapper _launchOptionsMapper;

    public ViewServiceContext ViewContext { get; }
    private readonly GameDataService _gameDataService;
    private readonly GameLinkDataService _gameLinkDataService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ISettingsProvider _settingsProvider;
    private readonly TombRaiderEngineDetector _engineDetector;

    public void InitializeSettings(GameDetailsViewModel target)
    {
        var gameDetailsSettings = _settingsProvider.GetGameDetailsSettings();
        target.AskForConfirmationBeforeOpeningWalkthrough = gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        target.EnabledPatterns = gameDetailsSettings.EnabledPatterns.Select(p => p.Value).ToList();
        target.IgnoredFolders = gameDetailsSettings.ExcludedFolders.Select(p => p.Value).ToList();
        target.DescriptionFontSize = gameDetailsSettings.DescriptionFontSize;
    }

    public void OpenGameFolder(string gameFolder)
    {
        _platformSpecificFeatures.OpenGameFolder(gameFolder);
    }

    public async Task FetchLinks(GameDetailsViewModel game, LinkType linkType)
    {
        var links = await _gameLinkDataService.GetLinks(game.Game.GameMetadata.Id, CancellationToken.None, linkType);
        game.WalkthroughLinks = _mapper.ToObservableCollection(links);
    }

    public async Task OpenWalkthrough(string link, bool askConfirmation)
    {
        if (askConfirmation)
        {
            var confirmation = await ViewContext.PopupService.ShowLocalized("Confirm",
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
            await ViewContext.PopupService.Show(new UrlOpenErrorMessageBox()
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
        ViewContext.PopupService.ShowDialog(new LaunchOptionsDialogViewModel(_engineDetector) { TargetGame = gameDetailsViewModel.Game.GameMetadata }, SaveLaunchOptions);
    }

    private async void SaveLaunchOptions(LaunchOptionsDialogViewModel vm)
    {
        try
        {
            var gameMetadata = vm.TargetGame;
            if (NavigationManager.CurrentPage is PageViewModel currentPage)
            {
                using (currentPage.BusyScope("SAVING_LAUNCH_OPTIONS".GetLocalizedString()))
                {
                    var launchOptionsDto = _launchOptionsMapper.ToDto(vm);

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