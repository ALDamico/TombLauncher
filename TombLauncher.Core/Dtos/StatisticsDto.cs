namespace TombLauncher.Core.Dtos;

public class StatisticsDto
{
    public GameStatisticsDto LatestPlayedGame { get; set; }
    public GameStatisticsDto LongestPlaySession { get; set; }
    public GameStatisticsDto MostLaunches { get; set; }
    public List<DayOfWeekStatisticsDto> DayOfWeekStatistics { get; set; } = new List<DayOfWeekStatisticsDto>();
    public List<DailyStatisticsDto> DailyStatistics { get; set; }
}