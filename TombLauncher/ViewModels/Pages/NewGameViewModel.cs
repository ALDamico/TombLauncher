﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(NewGameService newGameService) 
    {
        _newGameService = newGameService;
        _gameMetadata = new GameMetadataViewModel();
        //_gameMetadata.PropertyChanged += OnGameMetadataPropertyChanged;

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

        PickZipArchiveCmd = new AsyncRelayCommand(PickZipArchive);
        PickFolderCmd = new AsyncRelayCommand(PickFolder);
    }

    private readonly NewGameService _newGameService;

   /* private void OnGameMetadataPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameMetadataViewModel.Title))
        {
            SaveCmd.NotifyCanExecuteChanged();
        }
    }*/
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source;
    public ObservableCollection<EnumViewModel<GameLength>> AvailableLengths { get; }
    public ObservableCollection<EnumViewModel<GameDifficulty>> AvailableDifficulties { get; }
    public IProgress<CopyProgressInfo> InstallProgress { get; }

    public ICommand PickZipArchiveCmd { get; }

    private async Task PickZipArchive()
    {
        Source = await _newGameService.PickZipArchive();
    }

    public ICommand PickFolderCmd { get; }

    private async Task PickFolder()
    {
        Source = await _newGameService.PickFolder();
    }

    protected override async Task SaveInner()
    {
        IsBusy = true;
        
        await _newGameService.InstallGame(GameMetadata, InstallProgress, Source);
    }

    protected override bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(GameMetadata.Title) && !string.IsNullOrWhiteSpace(Source);
    }

    public IDialogService DialogService => _newGameService.DialogService;

    /*protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameMetadataViewModel.Title) || e.PropertyName == nameof(Source))
        {
            SaveCmd.NotifyCanExecuteChanged();
        }

        base.OnPropertyChanged(e);
    }*/
}