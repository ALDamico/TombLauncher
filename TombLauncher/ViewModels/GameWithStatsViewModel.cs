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
    public GameWithStatsViewModel(GameMetadataViewModel gameMetadata) : this()
    {
        GameMetadata = gameMetadata;
    }
    public GameWithStatsViewModel()
    {
        _gameWithStatsService = Ioc.Default.GetRequiredService<GameWithStatsService>();
        PlayCmd = new RelayCommand(Play, CanPlay);
        OpenCmd = new AsyncRelayCommand(Open);
        LaunchSetupCmd = new RelayCommand(LaunchSetup, CanLaunchSetup);
        LaunchCommunitySetupCmd = new RelayCommand(LaunchCommunitySetup, CanLaunchCommunitySetup);
        MarkGameAsFavouriteCmd = new AsyncRelayCommand(MarkGameAsFavourite);
        MarkGameAsCompletedCmd = new AsyncRelayCommand(MarkGameAsComplete);
        UninstallCmd = new AsyncRelayCommand(Uninstall, CanUninstall);
    }

    private readonly GameWithStatsService _gameWithStatsService;
    
    [ObservableProperty]
    private GameMetadataViewModel _gameMetadata;
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

    private bool CanLaunchSetup()
    {
        return _gameWithStatsService.CanLaunchSetup(this);
    }
    
    public ICommand LaunchCommunitySetupCmd { get; }

    private void LaunchCommunitySetup()
    {
        _gameWithStatsService.LaunchCommunitySetup(this);
    }

    private bool CanLaunchCommunitySetup()
    {
        return _gameWithStatsService.CanLaunchCommunitySetup(this);
    }
    
    public IAsyncRelayCommand MarkGameAsFavouriteCmd { get; }

    private async Task MarkGameAsFavourite()
    {
        await _gameWithStatsService.ToggleFavourite(this);
    }
    
    public IAsyncRelayCommand MarkGameAsCompletedCmd { get; }

    private async Task MarkGameAsComplete()
    {
        await _gameWithStatsService.ToggleCompleted(this);
    }
    
    public ICommand UninstallCmd { get; }

    private async Task Uninstall()
    {
        await _gameWithStatsService.Uninstall(GameMetadata.InstallDirectory, GameMetadata.Id);
    }

    public bool CanUninstall()
    {
        return _gameWithStatsService.CanUninstall(GameMetadata);
    }
}