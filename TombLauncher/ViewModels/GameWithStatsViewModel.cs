using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameWithStatsViewModel : ViewModelBase
{
    public GameWithStatsViewModel(GameWithStatsService gameWithStatsService)
    {
        _gameWithStatsService = gameWithStatsService;
        PlayCmd = new RelayCommand(Play, CanPlay);
        OpenCmd = new RelayCommand(Open);
    }

    private GameWithStatsService _gameWithStatsService;
    
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private TimeSpan _totalPlayedTime;
    [ObservableProperty] private DateTime? _lastPlayed;
    [ObservableProperty] private bool _areCommandsVisible;
    public ICommand PlayCmd { get; }
    private void Play()
    {
        _gameWithStatsService.PlayGame(this);
    }

    private bool CanPlay() => _gameWithStatsService.CanPlayGame(this);
    public ICommand OpenCmd { get; }
    private void Open()
    {
        _gameWithStatsService.OpenGame(this);
    }
}