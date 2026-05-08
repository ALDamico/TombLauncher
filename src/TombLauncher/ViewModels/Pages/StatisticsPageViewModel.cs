using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class StatisticsPageViewModel : PageViewModel
{
    public StatisticsPageViewModel(StatisticsService statisticsService, GameWithStatsService gameWithStatsService,
        IPlatformSpecificFeatures platformFeatures, IAppConfiguration appConfiguration)
    {
        _statisticsService = statisticsService;
        _gameWithStatsService = gameWithStatsService;
        _platformFeatures = platformFeatures;
        _appConfiguration = appConfiguration;
        TopBarCommands =
        [
            new CommandViewModel()
            {
                Command = new AsyncRelayCommand(Initialize), 
                Icon = PackIconRemixIconKind.RefreshLine,
                Tooltip = "RELOAD".GetLocalizedString()
            }
        ];
    }

    private readonly StatisticsService _statisticsService;
    private readonly GameWithStatsService _gameWithStatsService;
    private readonly IPlatformSpecificFeatures _platformFeatures;
    private readonly IAppConfiguration _appConfiguration;

    [ObservableProperty] private Version? _applicationVersion;
    [ObservableProperty] private long _databaseSize;
    [ObservableProperty] private long _gamesSize;
    [ObservableProperty] private Version? _netVersion;
    [ObservableProperty] private StatisticsViewModel? _statistics;
    [ObservableProperty] private string? _wineVersion;

    public bool IsWineSupported => _platformFeatures.IsWineSupported;

    public override async Task OnNavigatedTo(object parameter)
    {
        await Initialize();
    }

    private async Task Initialize()
    {
        using (BusyScope("GATHERING_STATISTICS".GetLocalizedString()))
        {
            var t1 = Task.Run(() => ApplicationVersion = VersionUtils.GetApplicationVersion());
            var t2 = Task.Run(() => DatabaseSize = _statisticsService.GetDatabaseSize());
            var t3 = Task.Run(() => GamesSize = _statisticsService.GetGamesSize());
            var t4 = _statisticsService.GetStatistics();
            var t5 = Task.Run(() => NetVersion = VersionUtils.GetDotNetVersion());
            var t6 = Task.Run(() => WineVersion = _platformFeatures.GetWineVersion(
                _appConfiguration.Compatibility.WinePath ?? string.Empty));

            await Task.WhenAll(t1, t2, t3, t4, t5, t6);
            Statistics = t4.Result;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task OpenGame(int gameId)
    {
        await _gameWithStatsService.OpenGame(gameId);
    }
}