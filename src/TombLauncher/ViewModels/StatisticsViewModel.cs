using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TombLauncher.ViewModels;

public partial class StatisticsViewModel: ObservableObject
{
    public StatisticsViewModel()
    {
        
    }
    [ObservableProperty] private GameStatisticsViewModel _latestPlayedGame = null!;
    [ObservableProperty] private GameStatisticsViewModel _longestPlaySession = null!;
    [ObservableProperty] private GameStatisticsViewModel _mostLaunches = null!;
    [ObservableProperty] private ChartViewModel _dayOfWeekAveragePlayTimeStatistics = null!;
    [ObservableProperty] private ChartViewModel _dayOfWeekTotalGamesPlayedStatistics = null!;
    [ObservableProperty] private ChartViewModel _dailyAverageGameLengthStatistics = null!;
    [ObservableProperty] private ChartViewModel _spaceUsedStatistics = null!;
}