using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Navigation;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class NewGameService : IViewService
{
    public NewGameService(ViewServiceContext viewContext, GamesUnitOfWork gamesUnitOfWork,
        GameFileHashCalculator hashCalculator,
        TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector,
        ILogger<NewGameService> logger)
    {
        ViewContext = viewContext;
        GamesUnitOfWork = gamesUnitOfWork;
        GameFileHashCalculator = hashCalculator;
        LevelInstaller = levelInstaller;
        EngineDetector = engineDetector;
        _logger = logger;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly ILogger<NewGameService> _logger;
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper _mapper => ViewContext.Mapper;
    public GameFileHashCalculator GameFileHashCalculator { get; }
    public TombRaiderLevelInstaller LevelInstaller { get; }
    public TombRaiderEngineDetector EngineDetector { get; }

    public async Task<string> PickZipArchive()
    {
        var file = await DialogService.OpenFile(LocalizationManager["Select a ZIP file"], new List<FilePickerFileType>()
        {
            new FilePickerFileType(LocalizationManager["ZIP files"])
            {
                Patterns = new List<string>() { "*.zip" }
            }
        });

        return file ?? string.Empty;
    }

    public async Task<string> PickFolder()
    {
        return await DialogService.OpenFolder(LocalizationManager["Select a folder"]) ?? string.Empty;
    }

    public async Task InstallGame(GameMetadataViewModel gameMetadata, IProgress<CopyProgressInfo> progress, string source)
    {
        _logger.LogInformation("Installing game {GameTitle}", gameMetadata.Title);
        progress.Report(new CopyProgressInfo() { Message = LocalizationManager.GetLocalizedString("Installing GAMENAME", gameMetadata.Title) });
        var hashes = await GameFileHashCalculator.CalculateHashes(source);
        if (GamesUnitOfWork.ExistsHashes(hashes, out _))
        {
            _logger.LogWarning("Game {GameTitle} is already installed", gameMetadata.Title);
            var messageBoxResult = await Dispatcher.UIThread.InvokeAsync(() =>
                MessageBoxService.ShowLocalized("The same mod is already installed",
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

        var gameMetadataDto = _mapper.Map<GameMetadataDto>(gameMetadata);

        var installLocation = await LevelInstaller.Install(source, gameMetadataDto, CancellationToken.None, progress);
        progress.Report(new CopyProgressInfo() { Message = "Finishing up..." });
        gameMetadata.InstallDirectory = installLocation;
        var gameEngineResult = EngineDetector.Detect(installLocation);
        gameMetadata.GameEngine = gameEngineResult.GameEngine;
        gameMetadata.ExecutablePath = gameEngineResult.ExecutablePath;
        gameMetadata.SetupExecutable = gameEngineResult.SetupExecutablePath;
        gameMetadata.SetupExecutableArgs = gameEngineResult.SetupArgs;
        gameMetadata.CommunitySetupExecutable = gameEngineResult.CommunitySetupExecutablePath;
        gameMetadata.IsInstalled = true;

        var dto = _mapper.Map<GameMetadataDto>(gameMetadata);
        await GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        await GamesUnitOfWork.SaveHashes(hashes);
        _logger.LogInformation("Game {GameTitle} installed successfully", gameMetadata.Title);

        await NavigationManager.GoBack();
    }
}