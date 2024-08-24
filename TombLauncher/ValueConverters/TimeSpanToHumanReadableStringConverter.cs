using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class TimeSpanToHumanReadableStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            
            var parts = new List<string>();
            var days = timeSpan.Days;
            if (days > 0)
            {
                parts.Add($"{days} days");
            }

            var hours = timeSpan.Hours;
            if (hours > 0)
            {

                parts.Add($"{hours} hours");
            }

            var minutes = timeSpan.Minutes;
            if (minutes > 0)
            {
                parts.Add($"{minutes} minutes");
            }

            var seconds = timeSpan.Seconds;
            if (seconds > 0)
            {
                parts.Add($"{seconds} seconds");
            }

            if (parts.Count == 0)
            {
                return "N/A";
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }
            
            var sb = new StringBuilder();
            var i = 0;
            var lastPart = string.Empty;
            foreach (var part in parts.Take(parts.Count - 1))
            {
                sb.Append(part);
                if (i < parts.Count - 2)
                {
                    sb.Append(", ");
                }

                lastPart = part;
            }

            if (!lastPart.EndsWith(' '))
                sb.Append(' ');
            sb.Append($"and {parts.Last()}");

            return sb.ToString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}