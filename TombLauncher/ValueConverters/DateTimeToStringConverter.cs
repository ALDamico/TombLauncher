using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Extensions;

namespace TombLauncher.ValueConverters;

public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "Never";
        }
        if (value is DateTime dateTime)
        {
            var dateTimeAtMidnight = dateTime.GetDateAtMidnight();
            var today = DateTime.Today;
            if (dateTimeAtMidnight == today)
            {
                return "Today";
            }

            if (dateTimeAtMidnight.IsYesterday())
            {
                return "Yesterday";
            }

            var differenceInDays = (today - dateTimeAtMidnight).Days;
            if (differenceInDays > 1 && differenceInDays < 7)
            {
                return $"{differenceInDays} days ago";
            }

            var weeksElapsed = differenceInDays / 7;
            if (weeksElapsed == 1)
            {
                return "Last week";
            }

            if (weeksElapsed < 4)
            {
                return $"{weeksElapsed} weeks ago";
            }

            var yearsElapsed = today.Year - dateTimeAtMidnight.Year;
            if (yearsElapsed == 1)
            {
                return "Last year";
            }

            return $"{yearsElapsed} years ago";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}