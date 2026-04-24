using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class StatisticsPageViewModel : PageViewModel
{
    public StatisticsPageViewModel(StatisticsService statisticsService, GameWithStatsService gameWithStatsService)
    {
        _statisticsService = statisticsService;
        _gameWithStatsService = gameWithStatsService;
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

    [ObservableProperty] private Version? _applicationVersion;
    [ObservableProperty] private long _databaseSize;
    [ObservableProperty] private long _gamesSize;
    [ObservableProperty] private Version? _netVersion;
    [ObservableProperty] private StatisticsViewModel? _statistics;

    public override async Task OnNavigatedTo(object parameter)
    {
        await Initialize();
    }

    private async Task Initialize()
    {
        IsBusy = true;
        BusyMessage = "GATHERING_STATISTICS".GetLocalizedString();
        var t1 = Task.Run(() => ApplicationVersion = AppUtils.GetApplicationVersion());
        // Note: GetDatabaseSize might access shared resources but running in Task.Factory.StartNew on background thread might be risky if DbContext is not thread safe?
        // StatisticService typically uses new context or scope.
        // Assuming Services handle thread safety.
        var t2 = Task.Run(() => DatabaseSize = _statisticsService.GetDatabaseSize());
        var t3 = Task.Run(() => GamesSize = _statisticsService.GetGamesSize());
        var t4 = _statisticsService.GetStatistics();
        var t5 = Task.Run(() => NetVersion = AppUtils.GetDotNetVersion());

        await Task.WhenAll(t1, t2, t3, t4, t5);
        Statistics = t4.Result;
        SetBusy(false);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task OpenGame(int gameId)
    {
        await _gameWithStatsService.OpenGame(gameId);
    }
}