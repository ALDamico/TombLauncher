namespace TombLauncher.Core.Dtos;

public class DayOfWeekStatisticsDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan AverageTimePlayed { get; set; }
    public int PlaySessionsCount { get; set; }
}