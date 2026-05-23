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
        GameMetadata = null!;
        _gameWithStatsService = gameWithStatsService;
    }

    private readonly GameWithStatsService _gameWithStatsService;

    [ObservableProperty]
    public partial GameMetadataViewModel GameMetadata { get; set; }

    [ObservableProperty]
    public partial TimeSpan TotalPlayedTime { get; set; }

    [ObservableProperty]
    public partial DateTime? LastPlayed { get; set; }

    [ObservableProperty]
    public partial bool AreCommandsVisible { get; set; }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private async Task Play()
    {
        await _gameWithStatsService.PlayGame(this);
    }

    private bool CanPlay() => _gameWithStatsService.CanPlayGame(this);
    
    [RelayCommand]
    private async Task Open()
    {
        await _gameWithStatsService.OpenGame(this);
    }

    [RelayCommand(CanExecute = nameof(CanLaunchSetup))]
    private async Task LaunchSetup()
    {
        await _gameWithStatsService.LaunchSetup(this);
    }

    private bool CanLaunchSetup()
    {
        return _gameWithStatsService.CanLaunchSetup(this);
    }

    [RelayCommand(CanExecute = nameof(CanLaunchCommunitySetup))]
    private async Task LaunchCommunitySetup()
    {
        await _gameWithStatsService.LaunchCommunitySetup(this);
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