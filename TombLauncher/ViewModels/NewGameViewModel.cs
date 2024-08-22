using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Models.Models;
using TombLauncher.Progress;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(GamesUnitOfWork gamesUnitOfWork, IDialogService dialogService)
    {
        _gamesUoW = gamesUnitOfWork;
        _dialogService = dialogService;
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
            CurrentFile = copyProgressInfo.CurrentFileName;
            if (copyProgressInfo.Message != null)
            {
                BusyMessage = copyProgressInfo.Message;
            }
        });

        PickZipArchiveCmd = new RelayCommand(PickZipArchive);
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
    [ObservableProperty] private string _currentFile;
    [ObservableProperty] private double? _percentageComplete;
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

    protected override async void SaveInner()
    {
        IsBusy = true;
        InstallProgress.Report(new CopyProgressInfo(){Message = $"Installing {_gameMetadata.Title}..."});
        await Task.Run(() =>
        {
            var installer = new TombRaiderLevelInstaller();
            GameMetadata.InstallDate = DateTime.Now;
            var installLocation = installer.Install(Source, GameMetadata.ToDto(), InstallProgress);
            InstallProgress.Report(new CopyProgressInfo() { Message = "Finishing up..." });
            GameMetadata.InstallDirectory = installLocation;
            var engineDetector = new TombRaiderEngineDetector();
            var gameEngine = engineDetector.Detect(installLocation);
            GameMetadata.GameEngine = gameEngine;
            _gamesUoW.UpsertGame(GameMetadata.ToDto());
        });

        IsBusy = false;
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