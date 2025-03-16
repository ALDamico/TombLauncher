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
    [ObservableProperty] private GameStatisticsViewModel _latestPlayedGame;
    [ObservableProperty] private GameStatisticsViewModel _longestPlaySession;
    [ObservableProperty] private GameStatisticsViewModel _mostLaunches;
    [ObservableProperty] private ChartViewModel _dayOfWeekAveragePlayTimeStatistics;
    [ObservableProperty] private ChartViewModel _dayOfWeekTotalGamesPlayedStatistics;
    [ObservableProperty] private ChartViewModel _dailyAverageGameLengthStatistics;
    [ObservableProperty] private ChartViewModel _spaceUsedStatistics;
}