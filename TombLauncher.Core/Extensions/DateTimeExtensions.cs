namespace TombLauncher.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime GetDateAtMidnight(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
    }

    public static bool IsYesterday(this DateTime dateTime)
    {
        return (DateTime.Today - dateTime.GetDateAtMidnight()).Days == 1;
    }

    public static DateTime GetOneSecondToMidnight(this DateTime dateTime)
    {
        return dateTime.GetDateAtMidnight() + TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1);
    }

    public static TimeSpan Average(this IEnumerable<TimeSpan> timeSpans)
    {
        if (timeSpans == null) 
            return TimeSpan.Zero;
        var ticks = timeSpans.Select(t => t.Ticks).ToList();
        if (ticks.Count == 0)
            return TimeSpan.Zero;

        var average = ticks.Average();
        return TimeSpan.FromTicks((long)Math.Round(average, 0, MidpointRounding.AwayFromZero));
    }

    public static TimeSpan Sum(this IEnumerable<TimeSpan> timeSpans)
    {
        if (timeSpans == null) 
            return TimeSpan.Zero;
        var ticks = timeSpans.Select(t => t.Ticks).ToList();
        if (ticks.Count == 0)
            return TimeSpan.Zero;

        return TimeSpan.FromTicks(ticks.Sum());
    }
}