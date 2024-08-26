using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class LocalizedFallbackValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return (string)parameter;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}