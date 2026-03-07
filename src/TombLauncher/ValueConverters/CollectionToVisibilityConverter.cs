using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class CollectionToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ICollection collection)
        {
            return Invert ? collection.Count > 0 : collection.Count == 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
