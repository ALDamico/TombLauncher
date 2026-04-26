using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class StatisticsViewModel: ObservableObject
{
    [ObservableProperty] private GameStatisticsViewModel? _latestPlayedGame;
    [ObservableProperty] private GameStatisticsViewModel? _longestPlaySession;
    [ObservableProperty] private GameStatisticsViewModel? _mostLaunches;
    [ObservableProperty] private ChartViewModel _dayOfWeekAveragePlayTimeStatistics = null!;
    [ObservableProperty] private ChartViewModel _dayOfWeekTotalGamesPlayedStatistics = null!;
    [ObservableProperty] private ChartViewModel _dailyAverageGameLengthStatistics = null!;
    [ObservableProperty] private ChartViewModel _spaceUsedStatistics = null!;
}