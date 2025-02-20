using System.Globalization;

namespace TombLauncher.Core.Utils;

public class DayOfWeekCultureSensitiveComparer : IComparer<DayOfWeek>
{
    public DayOfWeekCultureSensitiveComparer(CultureInfo targetCulture)
    {
        _targetCulture = targetCulture;
    }

    private readonly CultureInfo _targetCulture;

    public int Compare(DayOfWeek x, DayOfWeek y)
    {
        var firstDayOfWeek = _targetCulture.DateTimeFormat.FirstDayOfWeek;
        var weightedX = (x - firstDayOfWeek + 7) % 7;
        var weightedY = (y - firstDayOfWeek + 7) % 7;
        return weightedX.CompareTo(weightedY);
    }
}