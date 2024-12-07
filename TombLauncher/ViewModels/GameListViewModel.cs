﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameListViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<GameWithStatsViewModel> _games;
    [ObservableProperty] private GameWithStatsViewModel _selectedGame;

    public GameListViewModel(GameListService gameListService, LocalizationManager localizationManager) : base(localizationManager)
    {
        _gameListService = gameListService;
        AddGameCmd = new RelayCommand(AddGame);
        UninstallCmd = new RelayCommand<GameWithStatsViewModel>(Uninstall);
        _gameListService.NavigationManager.OnNavigated += OnInit;
    }

    private GameListService _gameListService;

    private async void OnInit()
    {
        SetBusy(true,  LocalizationManager.GetLocalizedString("Loading games..."));
        Games = await _gameListService.FetchGames();
        ClearBusy();
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        _gameListService.AddGame();
    }
    
    public ICommand UninstallCmd { get; }

    private async void Uninstall(GameWithStatsViewModel game)
    {
        await _gameListService.Uninstall(this, game);
    }
}