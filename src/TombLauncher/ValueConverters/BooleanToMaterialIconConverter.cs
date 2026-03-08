using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ValueConverters;

public class BooleanToMaterialIconConverter : IValueConverter
{
    public PackIconRemixIconKind? TrueValue { get; set; }
    public PackIconRemixIconKind? FalseValue { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueValue : FalseValue;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return false;
        if (targetType == typeof(bool))
        {
            return (PackIconRemixIconKind)value == TrueValue;
        }

        return false;
    }
}