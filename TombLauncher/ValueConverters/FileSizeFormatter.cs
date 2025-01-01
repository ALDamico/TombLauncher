using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class FileSizeFormatter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var multiples = new string[]
        {
            "bytes",
            "kB",
            "MB",
            "GB",
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

            return $"{tmpVal:F} {unit}";

        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}