using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.ValueConverters;

public class TimeSpanToHumanReadableStringConverter : IValueConverter
{
    private Func<string, object[], string> StringGenerator(ILocalizationManager localizationManager)
    {
        if (localizationManager != null)
        {
            return localizationManager.GetLocalizedString;
        }

        return string.Format;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
                    parts.Add(func("1 day", null));
                else
                    parts.Add(func("days formattable", [days]));
            }

            var hours = timeSpan.Hours;
            if (hours > 0)
            {
                if (hours == 1)
                    parts.Add(func("1 hour", null));
                else
                    parts.Add(func("hours formattable", [hours]));
            }

            var minutes = timeSpan.Minutes;
            if (minutes > 0)
            {
                if (minutes == 1)
                    parts.Add(func("1 minute", null));
                else
                    parts.Add(func("minutes formattable", [minutes]));
            }

            var seconds = timeSpan.Seconds;
            if (seconds > 0)
            {
                if (seconds == 1)
                    parts.Add(func("1 second", null));
                else
                    parts.Add(func("seconds formattable", [seconds]));
            }

            if (parts.Count == 0)
            {
                return func("N/A", null);
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
            sb.Append(func("and formattable", [parts.Last()]));

            return sb.ToString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}