using System;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameWithStatsViewModel : ViewModelBase
{
    public GameWithStatsViewModel(GamesUnitOfWork gamesUnitOfWork, LocalizationManager localizationManager)
    {
        PlayCmd = new RelayCommand(Play);
        OpenCmd = new RelayCommand(Open);
        _gamesUnitOfWork = gamesUnitOfWork;
        _localizationManager = localizationManager;
    }

    private void Open()
    {
        var gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
        Program.NavigationManager.NavigateTo(new GameDetailsViewModel(gameDetailsService, this, _localizationManager));
    }

    private void Play()
    {
        var currentPage =  Program.NavigationManager.GetCurrentPage();
        currentPage.IsBusy = true;
        currentPage.BusyMessage = _localizationManager.GetLocalizedString("Starting GAMENAME", GameMetadata.Title);
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(GameMetadata.ExecutablePath)
            {
                WorkingDirectory = GameMetadata.InstallDirectory,
                UseShellExecute = true,
            },
            EnableRaisingEvents = true
        };
        
        process.Exited += OnGameExited;
        process.Start();
    }

    private void OnGameExited(object sender, EventArgs args)
    {
        var currentPage = Program.NavigationManager.GetCurrentPage();
        currentPage.BusyMessage = _localizationManager["Saving play session..."];

        var process = sender as Process;
        _gamesUnitOfWork.AddPlaySessionToGame(GameMetadata.ToDto(), process.StartTime, process.ExitTime);
        _gamesUnitOfWork.Save();
        Program.NavigationManager.RequestRefresh();
        currentPage.IsBusy = false;
        currentPage.BusyMessage = null;
        
        // TODO Save play session;
    }

    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private TimeSpan _totalPlayedTime;
    [ObservableProperty] private DateTime? _lastPlayed;
    [ObservableProperty] private bool _areCommandsVisible;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    private readonly LocalizationManager _localizationManager;
    public ICommand PlayCmd { get; }
    public ICommand OpenCmd { get; }
}