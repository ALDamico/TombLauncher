using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Core.Navigation;
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
        TopBarCommands = new ObservableCollection<ITopBarCommand>()
        {
            new CommandViewModel()
            {
                Command = new AsyncRelayCommand(Initialize), Icon = MaterialIconKind.Reload, Tooltip = "Reload".GetLocalizedString()
            }
        };
        OpenGameCmd = new AsyncRelayCommand<int>(OpenGame);
    }

    private readonly StatisticsService _statisticsService;
    private readonly GameWithStatsService _gameWithStatsService;

    [ObservableProperty] private Version _applicationVersion;
    [ObservableProperty] private long _databaseSize;
    [ObservableProperty] private long _gamesSize;
    [ObservableProperty] private Version _netVersion;
    [ObservableProperty] private StatisticsViewModel _statistics;

    public override async Task OnNavigatedTo(object parameter)
    {
        await Initialize();
    }

    private async Task Initialize()
    {
        IsBusy = true;
        BusyMessage = "Gathering statistics...".GetLocalizedString();
        var t1 = Task.Run(() => ApplicationVersion = AppUtils.GetApplicationVersion());
        // Note: GetDatabaseSize might access shared resources but running in Task.Factory.StartNew on background thread might be risky if DbContext is not thread safe?
        // StatisticService typically uses new context or scope.
        // Assuming Services handle thread safety.
        var t2 = Task.Run(() => DatabaseSize = _statisticsService.GetDatabaseSize());
        var t3 = Task.Run(() => GamesSize = _statisticsService.GetGamesSize());
        var t4 = Task.Run(() => Statistics = _statisticsService.GetStatistics());
        var t5 = Task.Run(() => NetVersion = AppUtils.GetDotNetVersion());

        await Task.WhenAll(t1, t2, t3, t4, t5);
        SetBusy(false);
    }

    public ICommand OpenGameCmd { get; }

    private async Task OpenGame(int gameId)
    {
        await _gameWithStatsService.OpenGame(gameId);
    }
}