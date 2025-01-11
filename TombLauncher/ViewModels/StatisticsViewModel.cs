using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class StatisticsViewModel: ObservableObject
{
    [ObservableProperty] private GameStatisticsViewModel _latestPlayedGame;
    [ObservableProperty] private GameStatisticsViewModel _longestPlaySession;
    [ObservableProperty] private GameStatisticsViewModel _mostLaunches;
    [ObservableProperty] private ObservableCollection<DayOfWeekStatisticsViewModel> _dayOfWeekStatistics;
    [ObservableProperty] private ObservableCollection<DailyStatisticsViewModel> _dailyStatistics;
}