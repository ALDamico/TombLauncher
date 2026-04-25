using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameWithStatsViewModel : ViewModelBase
{
    public GameWithStatsViewModel(GameWithStatsService gameWithStatsService, GameMetadataViewModel gameMetadata) : this(gameWithStatsService)
    {
        GameMetadata = gameMetadata;
    }
    public GameWithStatsViewModel(GameWithStatsService gameWithStatsService)
    {
        _gameMetadata = null!;
        _gameWithStatsService = gameWithStatsService;
    }

    private readonly GameWithStatsService _gameWithStatsService;

    [ObservableProperty] private GameMetadataViewModel _gameMetadata;
    [ObservableProperty] private TimeSpan _totalPlayedTime;
    [ObservableProperty] private DateTime? _lastPlayed;
    [ObservableProperty] private bool _areCommandsVisible;
    [RelayCommand(CanExecute = nameof(CanPlay))]
    private void Play()
    {
        _gameWithStatsService.PlayGame(this);
    }

    private bool CanPlay() => _gameWithStatsService.CanPlayGame(this);
    
    [RelayCommand]
    private async Task Open()
    {
        await _gameWithStatsService.OpenGame(this);
    }

    [RelayCommand(CanExecute = nameof(CanLaunchSetup))]
    private void LaunchSetup()
    {
        _gameWithStatsService.LaunchSetup(this);
    }

    private bool CanLaunchSetup()
    {
        return _gameWithStatsService.CanLaunchSetup(this);
    }

    [RelayCommand(CanExecute = nameof(CanLaunchCommunitySetup))]
    private void LaunchCommunitySetup()
    {
        _gameWithStatsService.LaunchCommunitySetup(this);
    }

    private bool CanLaunchCommunitySetup()
    {
        return _gameWithStatsService.CanLaunchCommunitySetup(this);
    }

    [RelayCommand]
    private async Task ToggleFavourite()
    {
        await _gameWithStatsService.ToggleFavourite(this);
    }

    [RelayCommand]
    private async Task ToggleCompleted()
    {
        await _gameWithStatsService.ToggleCompleted(this);
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanUninstall))]
    private async Task Uninstall()
    {
        await _gameWithStatsService.Uninstall(GameMetadata.Id);
    }

    public bool CanUninstall()
    {
        return _gameWithStatsService.CanUninstall(GameMetadata);
    }
}