using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class StatisticsPageViewModel : PageViewModel
{
    public StatisticsPageViewModel()
    {
        _statisticsService = Ioc.Default.GetRequiredService<StatisticsService>();
        Initialize += FetchStatistics;
        TopBarCommands = new ObservableCollection<CommandViewModel>()
        {
            new()
            {
                Command = new RelayCommand(FetchStatistics), Icon = MaterialIconKind.Reload, Tooltip = "Reload".GetLocalizedString()
            }
        };
    }

    private readonly StatisticsService _statisticsService;

    [ObservableProperty] private Version _applicationVersion;
    [ObservableProperty] private long _databaseSize;
    [ObservableProperty] private long _gamesSize;
    [ObservableProperty] private Version _netVersion;
    [ObservableProperty] private StatisticsViewModel _statistics;

    private async void FetchStatistics()
    {
        IsBusy = true;
        BusyMessage = "Gathering statistics...".GetLocalizedString();
        var t1 = Task.Factory.StartNew(() => ApplicationVersion = _statisticsService.GetApplicationVersion());
        var t2 = Task.Factory.StartNew(() => DatabaseSize = _statisticsService.GetDatabaseSize());
        var t3 = Task.Factory.StartNew(() => GamesSize = _statisticsService.GetGamesSize());
        var t4 = Task.Factory.StartNew(() => Statistics = _statisticsService.GetStatistics());
        var t5 = Task.Factory.StartNew(() => NetVersion = _statisticsService.GetNetVersion());

        await Task.WhenAll(t1, t2, t3, t4, t5);
        SetBusy(false);
    }
}