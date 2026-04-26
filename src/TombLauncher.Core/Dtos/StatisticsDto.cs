namespace TombLauncher.Core.Dtos;

public class StatisticsDto
{
    public GameStatisticsDto? LatestPlayedGame { get; set; }
    public GameStatisticsDto? LongestPlaySession { get; set; }
    public GameStatisticsDto? MostLaunches { get; set; }
    public List<DayOfWeekStatisticsDto> DayOfWeekStatistics { get; set; } = [];
    public List<DailyStatisticsDto> DailyStatistics { get; set; } = [];
    public List<GameSpaceUsedDto> SpaceUsedStatistics { get; set; } = [];
}