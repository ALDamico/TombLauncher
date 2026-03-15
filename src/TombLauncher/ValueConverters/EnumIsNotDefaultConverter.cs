using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

/// <summary>
/// Returns true if the bound enum value is not the default (0 / None).
/// Used to collapse icon placeholders in menu items that have no icon assigned.
/// </summary>
public class EnumIsNotDefaultConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Enum e && System.Convert.ToInt32(e) != 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
