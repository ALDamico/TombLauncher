using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

/// <summary>
/// Compares two int values (item index and selected index).
/// Returns the converter parameter parsed as double if they match, otherwise a fallback.
/// Usage: MultiBinding with [itemIndex, selectedIndex], ConverterParameter="activeValue|inactiveValue".
/// </summary>
public class IndexMatchToDoubleConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is int itemIndex && values[1] is int selectedIndex
            && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2
                && double.TryParse(parts[0], CultureInfo.InvariantCulture, out var activeValue)
                && double.TryParse(parts[1], CultureInfo.InvariantCulture, out var inactiveValue))
            {
                return itemIndex == selectedIndex ? activeValue : inactiveValue;
            }
        }
        return 8.0;
    }
}
