using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameWithStatsViewModel : ViewModelBase
{
    public GameWithStatsViewModel()
    {
        _gameWithStatsService = Ioc.Default.GetRequiredService<GameWithStatsService>();
        PlayCmd = new RelayCommand(Play, CanPlay);
        OpenCmd = new AsyncRelayCommand(Open);
        LaunchSetupCmd = new RelayCommand(LaunchSetup, CanPlay);
    }

    private readonly GameWithStatsService _gameWithStatsService;
    
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
    private async Task Open()
    {
        await _gameWithStatsService.OpenGame(this);
    }
    
    public ICommand LaunchSetupCmd { get; }

    private void LaunchSetup()
    {
        _gameWithStatsService.LaunchSetup(this);
    }
}