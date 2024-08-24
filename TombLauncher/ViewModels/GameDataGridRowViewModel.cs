using System;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class GameDataGridRowViewModel : ViewModelBase
{
    public GameDataGridRowViewModel(NavigationManager navigationManager, GamesUnitOfWork gamesUnitOfWork)
    {
        PlayCmd = new RelayCommand(Play);
        OpenCmd = new RelayCommand(Open);
        _navigationManager = navigationManager;
        _gamesUnitOfWork = gamesUnitOfWork;
    }

    private void Open()
    {
        Console.WriteLine($"Opening details for {_gameMetadata.Title}");
    }

    private void Play()
    {
        var currentPage = _navigationManager.GetCurrentPage();
        currentPage.IsBusy = true;
        currentPage.BusyMessage = $"Starting {GameMetadata.Title}";
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
        Console.WriteLine($"Playing {_gameMetadata.Title}");
    }

    private void OnGameExited(object sender, EventArgs args)
    {
        var currentPage = _navigationManager.GetCurrentPage();
        currentPage.BusyMessage = "Saving play session...";

        var process = sender as Process;
        _gamesUnitOfWork.AddPlaySessionToGame(GameMetadata.ToDto(), process.StartTime, process.ExitTime);
        _gamesUnitOfWork.Save();
        currentPage.IsBusy = false;
        currentPage.BusyMessage = null;
        
        // TODO Save play session;
    }

    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private bool _areCommandsVisible;
    private NavigationManager _navigationManager;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ICommand PlayCmd { get; }
    public ICommand OpenCmd { get; }
}