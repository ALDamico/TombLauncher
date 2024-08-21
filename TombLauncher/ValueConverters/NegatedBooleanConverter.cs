using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class NegatedBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(bool) && value is bool boolean)
        {
            return !boolean;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}