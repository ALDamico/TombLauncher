using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ionic.Zip;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Models;
using TombLauncher.Progress;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(GamesUnitOfWork gamesUnitOfWork, IDialogService dialogService, IMessageBoxService messageBoxService) 
    {
        _gamesUoW = gamesUnitOfWork;
        _dialogService = dialogService;
        _messageBoxService = messageBoxService;
        _gameMetadata = new GameMetadataViewModel();
        _gameMetadata.PropertyChanged += OnGameMetadataPropertyChanged;

        AvailableLengths = EnumUtils.GetEnumViewModels<GameLength>().ToObservableCollection();
        AvailableDifficulties = EnumUtils.GetEnumViewModels<GameDifficulty>().ToObservableCollection();

        InstallProgress = new Progress<CopyProgressInfo>(copyProgressInfo =>
        {
            if (copyProgressInfo.Percentage != null)
            {
                PercentageComplete = copyProgressInfo.Percentage;
            }

            CurrentFileName = copyProgressInfo.CurrentFileName;
            if (copyProgressInfo.Message != null)
            {
                BusyMessage = copyProgressInfo.Message;
            }
        });

        PickZipArchiveCmd = new RelayCommand(PickZipArchive);
        PickFolderCmd = new RelayCommand(PickFolder);
    }

    private void OnGameMetadataPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameMetadata.Title))
        {
            SaveCmd.NotifyCanExecuteChanged();
        }
    }

    private GamesUnitOfWork _gamesUoW;
    private readonly IDialogService _dialogService;
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source;
    private readonly IMessageBoxService _messageBoxService;
    public ObservableCollection<EnumViewModel<GameLength>> AvailableLengths { get; }
    public ObservableCollection<EnumViewModel<GameDifficulty>> AvailableDifficulties { get; }
    public IProgress<CopyProgressInfo> InstallProgress { get; }

    public ICommand PickZipArchiveCmd { get; }

    private async void PickZipArchive()
    {
        var file = await _dialogService.OpenFile("Select a ZIP file", new List<FilePickerFileType>()
        {
            new FilePickerFileType("ZIP files")
            {
                Patterns = new List<string>() { "*.zip" }
            }
        });
        if (string.IsNullOrWhiteSpace(file))
        {
            return;
        }

        Source = file;
    }
    
    public ICommand PickFolderCmd { get; }

    private async void PickFolder()
    {
        var folder = await _dialogService.OpenFolder("Select a folder");
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        Source = folder;
    }

    protected override async void SaveInner()
    {
        IsBusy = true;
        InstallProgress.Report(new CopyProgressInfo() { Message = $"Installing {_gameMetadata.Title}..." });
        await Task.Run(async () =>
        {
            var hashCalculator = new GameFileHashCalculator(new HashSet<string>()
            {
                ".tr4",
                ".pak",
                ".tr2",
                ".sfx",
                ".dat",
                ".phd"
            });
            var hashes = hashCalculator.CalculateHashes(Source);
            if (_gamesUoW.ExistsHashes(hashes))
            {
                var messageboxResult = await Dispatcher.UIThread.InvokeAsync(() =>
                    _messageBoxService.Show("The same mod is already installed",
                        "The same mod seems to be already installed. Press OK to keep installing, or Cancel to abort",
                        MsgBoxButton.OkCancel, MsgBoxImage.Error));
                
                if (messageboxResult.ButtonResult == MsgBoxButtonResult.Cancel)
                {
                    return;
                }
            }
            var installer = new TombRaiderLevelInstaller();
            GameMetadata.InstallDate = DateTime.Now;
            var guid = Guid.NewGuid();
            GameMetadata.Guid = guid;
            var installLocation = installer.Install(Source, GameMetadata.ToDto(), InstallProgress);
            InstallProgress.Report(new CopyProgressInfo() { Message = "Finishing up..." });
            
            GameMetadata.InstallDirectory = installLocation;
            var engineDetector = new TombRaiderEngineDetector();
            var gameEngine = engineDetector.Detect(installLocation);
            GameMetadata.GameEngine = gameEngine;
            GameMetadata.ExecutablePath = engineDetector.GetGameExecutablePath(installLocation);
            var dto = GameMetadata.ToDto();
            _gamesUoW.UpsertGame(dto);
            hashes.ForEach(h => h.GameId = dto.Id);
            _gamesUoW.SaveHashes(hashes);
        });

        ClearBusy();
        Program.NavigationManager.GoBack();
    }

    protected override bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(GameMetadata.Title) && !string.IsNullOrWhiteSpace(Source);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameMetadata.Title) || e.PropertyName == nameof(Source))
        {
            SaveCmd.NotifyCanExecuteChanged();
        }

        base.OnPropertyChanged(e);
    }
}