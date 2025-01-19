using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Core.Extensions;

namespace TombLauncher.ValueConverters;

public class UppercaseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
            return str?.RemoveDiacritics().ToUpper();
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}