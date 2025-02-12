﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Enums;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameWithStatsViewModel game) 
    {
        _gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
        _game = game;
        BrowseFolderCmd = new RelayCommand(BrowseFolder, CanBrowseFolder);
        UninstallCmd = new AsyncRelayCommand(Uninstall, CanUninstall);
        ReadWalkthroughCmd = new AsyncRelayCommand<GameLinkViewModel>(ReadWalkthrough);
        ManageSaveGamesCmd = new RelayCommand(ManageSavegames);
        OpenLaunchOptionsCmd = new RelayCommand(OpenLaunchOptions);
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        var gameDetailsSettings = settingsService.GetGameDetailsSettings();
        _askForConfirmationBeforeOpeningWalkthrough = gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        _useInternalViewerIfAvailable = gameDetailsSettings.UseInternalViewerIfAvailable;
        Initialize += OnInitialize;
    }

    private bool _askForConfirmationBeforeOpeningWalkthrough;
    private bool _useInternalViewerIfAvailable;
    private IDialogService _dialogService; 

    private async void OnInitialize()
    {
        await _gameDetailsService.FetchLinks(this, LinkType.Walkthrough);
    }

    private readonly GameDetailsService _gameDetailsService;
    [ObservableProperty] private GameWithStatsViewModel _game;
    [ObservableProperty] private ObservableCollection<GameLinkViewModel> _walkthroughLinks;
    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        _gameDetailsService.OpenGameFolder(Game.GameMetadata.InstallDirectory);
    }

    private bool CanBrowseFolder()
    {
        return _gameDetailsService.CanUninstall(Game.GameMetadata);
    }
    
    public ICommand ManageSaveGamesCmd { get; }

    private void ManageSavegames()
    {
        _gameDetailsService.OpenSavegameList(this);
    }
    
    public ICommand ReadWalkthroughCmd { get; }

    private async Task ReadWalkthrough(GameLinkViewModel link)
    {
        await _gameDetailsService.OpenWalkthrough(link.Link, _askForConfirmationBeforeOpeningWalkthrough);
    }
    
    public ICommand UninstallCmd { get; }

    private async Task Uninstall()
    {
        await _gameDetailsService.Uninstall(Game.GameMetadata.InstallDirectory, Game.GameMetadata.Id);
    }

    private bool CanUninstall()
    {
        return _gameDetailsService.CanUninstall(Game.GameMetadata);
    }

    [ObservableProperty] private ICommand _installCmd;
    
    public ICommand OpenLaunchOptionsCmd { get; }

    private void OpenLaunchOptions()
    {
        _gameDetailsService.OpenLaunchOptions(this);
    }
}