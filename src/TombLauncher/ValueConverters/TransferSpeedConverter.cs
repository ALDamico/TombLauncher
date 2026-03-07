using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class TransferSpeedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var multiples = new string[]
        {
            "B/s",
            "kB/s",
            "MB/s",
            "GB/s",
        };
        if (value is double d)
        {
            var tmpVal = d;
            var unit = string.Empty;
            
            foreach (var multiple in multiples.Select((m, i) => (m, i)))
            {
                if (multiple.i > 0)
                {
                    tmpVal /= 1024;
                }

                unit = multiple.m;

                if (tmpVal <= 1024)
                {
                    break;
                }
            }

            return $"{tmpVal:F2} {unit}";
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}