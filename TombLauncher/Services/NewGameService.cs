using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class NewGameService : IViewService
{
    public NewGameService(GamesUnitOfWork gamesUnitOfWork, 
        ILocalizationManager localizationManager, 
        NavigationManager navigationManager, 
        IMessageBoxService messageBoxService, 
        IDialogService dialogService,
        GameFileHashCalculator hashCalculator,
        TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector)
    {
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        GameFileHashCalculator = hashCalculator;
        LevelInstaller = levelInstaller;
        EngineDetector = engineDetector;
        _logger = Ioc.Default.GetRequiredService<ILogger<NewGameService>>();
    }

    private readonly ILogger<NewGameService> _logger;
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
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

        return file;
    }

    public async Task<string> PickFolder()
    {
        return await DialogService.OpenFolder(LocalizationManager["Select a folder"]);
    }

    public async Task InstallGame(GameMetadataViewModel gameMetadata, IProgress<CopyProgressInfo> progress, string source)
    {
        _logger.LogInformation("Installing game {GameTitle}", gameMetadata.Title);
        progress.Report(new CopyProgressInfo() { Message = LocalizationManager.GetLocalizedString("Installing GAMENAME", gameMetadata.Title)});
        var hashes = await GameFileHashCalculator.CalculateHashes(source);
        if (GamesUnitOfWork.ExistsHashes(hashes, out _))
        {
            _logger.LogWarning("Game {GameTitle} is already installed", gameMetadata.Title);
            var messageBoxResult = await Dispatcher.UIThread.InvokeAsync(() =>
                MessageBoxService.ShowLocalized("The same mod is already installed",
                    "The same mod is already installed TEXT",
                    MsgBoxButton.YesNo, 
                    MsgBoxImage.Error, 
                    noButtonText:"Cancel", 
                    yesButtonText:"Install anyway"));
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
        
        var installLocation = await LevelInstaller.Install(source, gameMetadata.ToDto(), progress);
        progress.Report(new CopyProgressInfo() { Message = "Finishing up..." });
        gameMetadata.InstallDirectory = installLocation;
        var gameEngineResult = EngineDetector.Detect(installLocation);
        gameMetadata.GameEngine = gameEngineResult.GameEngine;
        gameMetadata.ExecutablePath = gameEngineResult.ExecutablePath;
        gameMetadata.UniversalLauncherPath = gameEngineResult.UniversalLauncherPath;

        var dto = gameMetadata.ToDto();
        await GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        await GamesUnitOfWork.SaveHashes(hashes);
        _logger.LogInformation("Game {GameTitle} installed successfully", gameMetadata.Title);
        
        NavigationManager.GoBack();
    }
}