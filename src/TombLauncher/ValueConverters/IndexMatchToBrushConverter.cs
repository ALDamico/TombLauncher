using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

/// <summary>
/// Compares two int values (item index and selected index).
/// Returns the parameter value if they match, otherwise a fallback brush.
/// Usage: MultiBinding with bindings [itemIndex, selectedIndex, activeBrush, inactiveBrush].
/// </summary>
public class IndexMatchToBrushConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 4 && values[0] is int itemIndex && values[1] is int selectedIndex)
        {
            return itemIndex == selectedIndex ? values[2] : values[3];
        }
        return null;
    }
}
