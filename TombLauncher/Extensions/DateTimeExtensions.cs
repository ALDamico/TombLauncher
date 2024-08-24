using System;

namespace TombLauncher.Extensions;

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
}