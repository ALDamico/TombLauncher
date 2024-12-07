using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Progress;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class NewGameService : IViewService
{
    public NewGameService(GamesUnitOfWork gamesUnitOfWork, 
        LocalizationManager localizationManager, 
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
    }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public LocalizationManager LocalizationManager { get; }
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
        progress.Report(new CopyProgressInfo() { Message = LocalizationManager.GetLocalizedString("Installing GAMENAME", gameMetadata.Title)});
        var hashes = await GameFileHashCalculator.CalculateHashes(source);
        if (GamesUnitOfWork.ExistsHashes(hashes))
        {
            var messageBoxResult = await Dispatcher.UIThread.InvokeAsync(() =>
                MessageBoxService.Show(LocalizationManager["The same mod is already installed"],
                    LocalizationManager["The same mod is already installed TEXT"],
                    MsgBoxButton.YesNo, 
                    MsgBoxImage.Error, 
                    noButtonText:LocalizationManager["Cancel"], 
                    yesButtonText:LocalizationManager["Install anyway"]));
            if (messageBoxResult.ButtonResult == MsgBoxButtonResult.No)
            {
                return;
            }
        }
        
        gameMetadata.InstallDate = DateTime.Now;
        var guid = Guid.NewGuid();
        gameMetadata.Guid = guid;
        
        var installLocation = await LevelInstaller.Install(source, gameMetadata.ToDto(), progress);
        progress.Report(new CopyProgressInfo() { Message = "Finishing up..." });
        gameMetadata.InstallDirectory = installLocation;
        var gameEngine = EngineDetector.Detect(installLocation);
        gameMetadata.GameEngine = gameEngine;
        gameMetadata.ExecutablePath = EngineDetector.GetGameExecutablePath(installLocation);

        var dto = gameMetadata.ToDto();
        GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        GamesUnitOfWork.SaveHashes(hashes);
        
        NavigationManager.GoBack();
    }
}