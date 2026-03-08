using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;
using TombLauncher.Helpers;

namespace TombLauncher.ValueConverters;

public class TimeSpanToHumanReadableStringConverter : IValueConverter
{
    private static Func<string, object[], string> StringGenerator(ILocalizationManager? localizationManager)
        => LocalizationHelper.GetStringGenerator(localizationManager);
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var localizationManager = Ioc.Default.GetService<ILocalizationManager>();
        var func = StringGenerator(localizationManager);
        if (value is TimeSpan timeSpan)
        {

            var parts = new List<string>();
            var days = timeSpan.Days;
            if (days > 0)
            {
                if (days == 1)
                    parts.Add(func("1_DAY", Array.Empty<object>()));
                else
                    parts.Add(func("DAYS_FORMATTABLE", new object[] { days }));
            }

            var hours = timeSpan.Hours;
            if (hours > 0)
            {
                if (hours == 1)
                    parts.Add(func("1_HOUR", Array.Empty<object>()));
                else
                    parts.Add(func("HOURS_FORMATTABLE", new object[] { hours }));
            }

            var minutes = timeSpan.Minutes;
            if (minutes > 0)
            {
                if (minutes == 1)
                    parts.Add(func("1_MINUTE", Array.Empty<object>()));
                else
                    parts.Add(func("MINUTES_FORMATTABLE", new object[] { minutes }));
            }

            var seconds = timeSpan.Seconds;
            if (seconds > 0)
            {
                if (seconds == 1)
                    parts.Add(func("1_SECOND", Array.Empty<object>()));
                else
                    parts.Add(func("SECONDS_FORMATTABLE", new object[] { seconds }));
            }

            if (parts.Count == 0)
            {
                return func("NA", Array.Empty<object>());
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
                i++;
            }

            if (!lastPart.EndsWith(' '))
                sb.Append(' ');
            sb.Append(func("AND_FORMATTABLE", [parts.Last()]));

            return sb.ToString();
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}