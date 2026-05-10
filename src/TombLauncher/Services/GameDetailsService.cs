using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Services;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.MessageBoxes;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Patchers;

namespace TombLauncher.Services;

public class GameDetailsService : IViewService
{
    public GameDetailsService(ViewServiceContext viewContext, GameDataService gameDataService, GameLinkDataService gameLinkDataService,
        IPlatformSpecificFeatures platformSpecificFeatures, ISettingsProvider settingsProvider, ILogger<GameDetailsService> logger, 
        GameLinkDtoMapper mapper, GameMetadataMapper gameMapper, TroubleshootingContextService troubleshootingContextService)
    {
        _logger = logger;
        _mapper = mapper;
        _gameMapper = gameMapper;
        _troubleshootingContextService = troubleshootingContextService;
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _gameLinkDataService = gameLinkDataService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _settingsProvider = settingsProvider;
    }

    private readonly ILogger<GameDetailsService> _logger;
    private readonly GameLinkDtoMapper _mapper;
    private readonly GameMetadataMapper _gameMapper;
    private readonly TroubleshootingContextService _troubleshootingContextService;

    public ViewServiceContext ViewContext { get; }
    private readonly GameDataService _gameDataService;
    private readonly GameLinkDataService _gameLinkDataService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ISettingsProvider _settingsProvider;

    public void InitializeSettings(GameDetailsViewModel target)
    {
        _logger.LogInformation("Initializing game details settings");
        var gameDetailsSettings = _settingsProvider.GetGameDetailsSettings();
        target.AskForConfirmationBeforeOpeningWalkthrough = gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        target.EnabledPatterns = gameDetailsSettings.EnabledPatterns.Select(p => p.Value).ToList();
        target.IgnoredFolders = gameDetailsSettings.ExcludedFolders.Select(p => p.Value).ToList();
        target.DescriptionFontSize = gameDetailsSettings.DescriptionFontSize;
        _logger.LogInformation("Initialized game details settings");
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

    public async Task OpenLaunchOptions(GameDetailsViewModel gameDetailsViewModel)
    {
        await NavigationManager.NavigateTo<LaunchOptionsViewModel>(gameDetailsViewModel.Game.GameMetadata);
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

    public async Task<GameMetadataViewModel> GetGame(int id, CancellationToken ct)
    {
        var game = await _gameDataService.GetGameById(id, ct);
        var viewModel = _gameMapper.ToViewModel(game);
        return viewModel;
    }

    public async Task OpenChat(int gameId, CancellationToken cancellationToken)
    {
        var troubleshootingContext =
            await _troubleshootingContextService.GetTroubleshootingContext(gameId, cancellationToken);
        await NavigationManager.NavigateTo<AiChatViewModel>(troubleshootingContext);
    }

    public bool CanOpenChat(GameMetadataViewModel? vm)
    {
        return _settingsProvider.GetAiCoreSettings().IsEnabled && vm?.IsInstalled == true;
    }

    public EngineSupportState GetEngineSupportState(GameMetadataViewModel? vm) =>
        _platformSpecificFeatures.SupportMatrix.GetEngineSupportState(vm?.GameEngine);

    public async Task OpenWidescreenPatcher(GameMetadataViewModel gameMetadata) =>
        await NavigationManager.NavigateTo<WidescreenPatcherPageViewModel>(gameMetadata);

    public async Task OpenTrxNativePatcher(GameMetadataViewModel gameMetadata) =>
        await NavigationManager.NavigateTo<TrxNativePatcherPageViewModel>(gameMetadata);
}