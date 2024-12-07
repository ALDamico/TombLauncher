﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Data.Models;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Localization;
using TombLauncher.Progress;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(NewGameService newGameService, IDialogService dialogService,
        IMessageBoxService messageBoxService, LocalizationManager localizationManager) : base(localizationManager, messageBoxService, dialogService)
    {
        _newGameService = newGameService;
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

    private readonly NewGameService _newGameService;

    private void OnGameMetadataPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameMetadata.Title))
        {
            SaveCmd.NotifyCanExecuteChanged();
        }
    }
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private string _source;
    public ObservableCollection<EnumViewModel<GameLength>> AvailableLengths { get; }
    public ObservableCollection<EnumViewModel<GameDifficulty>> AvailableDifficulties { get; }
    public IProgress<CopyProgressInfo> InstallProgress { get; }

    public ICommand PickZipArchiveCmd { get; }

    private async void PickZipArchive()
    {
        Source = await _newGameService.PickZipArchive();
    }

    public ICommand PickFolderCmd { get; }

    private async void PickFolder()
    {
        Source = await _newGameService.PickFolder();
    }

    protected override async void SaveInner()
    {
        IsBusy = true;
        
        await _newGameService.InstallGame(_gameMetadata, InstallProgress, _source);
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