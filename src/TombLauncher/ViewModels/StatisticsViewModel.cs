using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class StatisticsViewModel: ObservableObject
{
    [ObservableProperty]
    public partial GameStatisticsViewModel? LatestPlayedGame { get; set; }

    [ObservableProperty]
    public partial GameStatisticsViewModel? LongestPlaySession { get; set; }

    [ObservableProperty]
    public partial GameStatisticsViewModel? MostLaunches { get; set; }

    [ObservableProperty]
    public partial ChartViewModel DayOfWeekAveragePlayTimeStatistics { get; set; } = null!;

    [ObservableProperty]
    public partial ChartViewModel DayOfWeekTotalGamesPlayedStatistics { get; set; } = null!;

    [ObservableProperty]
    public partial ChartViewModel DailyAverageGameLengthStatistics { get; set; } = null!;

    [ObservableProperty]
    public partial ChartViewModel SpaceUsedStatistics { get; set; } = null!;
}