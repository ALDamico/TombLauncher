using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class NewGameService : IViewService
{
    public NewGameService(ViewServiceContext viewContext, GameDataService gameDataService, GameHashDataService gameHashDataService,
        GameFileHashCalculator hashCalculator,
        TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector,
        ILogger<NewGameService> logger,
        GameMetadataMapper gameMetadataMapper)
    {
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _gameHashDataService = gameHashDataService;
        _gameFileHashCalculator = hashCalculator;
        _levelInstaller = levelInstaller;
        _engineDetector = engineDetector;
        _logger = logger;
        _gameMetadataMapper = gameMetadataMapper;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly ILogger<NewGameService> _logger;
    private readonly GameMetadataMapper _gameMetadataMapper;
    private readonly GameDataService _gameDataService;
    private readonly GameHashDataService _gameHashDataService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly GameFileHashCalculator _gameFileHashCalculator;
    private readonly TombRaiderLevelInstaller _levelInstaller;
    private readonly TombRaiderEngineDetector _engineDetector;

    public async Task<string> PickZipArchive()
    {
        var file = await ViewContext.PopupService.OpenFile("SELECT_A_ZIP_FILE".GetLocalizedString(), new List<FilePickerFileType>()
        {
            new("ZIP_FILES".GetLocalizedString())
            {
                Patterns = new List<string>() { "*.zip" }
            }
        });

        return file ?? string.Empty;
    }

    public async Task<string> PickFolder()
    {
        return await ViewContext.PopupService.OpenFolder("SELECT_A_FOLDER".GetLocalizedString()) ?? string.Empty;
    }

    public async Task InstallGame(GameMetadataViewModel gameMetadata, IProgress<CopyProgressInfo> progress, string source)
    {
        _logger.LogInformation("Installing game {GameTitle}", gameMetadata.Title);
        progress.Report(new CopyProgressInfo() { Message = "INSTALLING_GAMENAME".GetLocalizedString(gameMetadata.Title) });
        var hashes = await _gameFileHashCalculator.CalculateHashes(source);
        var (hashesExist, _) = await _gameHashDataService.ExistsHashes(hashes, CancellationToken.None);
        if (hashesExist)
        {
            _logger.LogWarning("Game {GameTitle} is already installed", gameMetadata.Title);
            var messageBoxResult = await Dispatcher.UIThread.InvokeAsync(() =>
                ViewContext.PopupService.ShowLocalized("The same mod is already installed",
                    "The same mod is already installed TEXT",
                    MsgBoxButton.YesNo,
                    MsgBoxImage.Error,
                    noButtonText: "Cancel",
                    yesButtonText: "Install anyway"));
            if (messageBoxResult.ButtonResult == MsgBoxButtonResult.No)
            {
                _logger.LogInformation("Will not install");
                return;
            }
            else
            {
                _logger.LogWarning("Will install anyway");
            }
        }

        gameMetadata.InstallDate = DateTime.Now;
        var guid = Guid.NewGuid();
        gameMetadata.Guid = guid;

        var gameMetadataDto = _gameMetadataMapper.ToDto(gameMetadata);

        var installLocation = await _levelInstaller.Install(source, gameMetadataDto, CancellationToken.None, progress);
        progress.Report(new CopyProgressInfo() { Message = "FINISHING_UP".GetLocalizedString() });
        gameMetadata.InstallDirectory = installLocation;
        var gameEngineResult = _engineDetector.Detect(installLocation);
        gameMetadata.GameEngine = gameEngineResult.GameEngine;
        gameMetadata.ExecutablePath = gameEngineResult.ExecutablePath;
        gameMetadata.SetupExecutable = gameEngineResult.SetupExecutablePath;
        gameMetadata.SetupExecutableArgs = gameEngineResult.SetupArgs;
        gameMetadata.CommunitySetupExecutable = gameEngineResult.CommunitySetupExecutablePath;
        gameMetadata.IsInstalled = true;

        var dto = _gameMetadataMapper.ToDto(gameMetadata);
        await _gameDataService.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        await _gameHashDataService.SaveHashes(hashes);
        _logger.LogInformation("Game {GameTitle} installed successfully", gameMetadata.Title);

        await NavigationManager.GoBack();
    }
}