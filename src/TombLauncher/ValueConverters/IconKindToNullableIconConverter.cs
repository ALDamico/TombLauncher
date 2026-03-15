using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ValueConverters;

/// <summary>
/// Converts a <see cref="PackIconRemixIconKind"/> to a <see cref="PackIconRemixIcon"/> control,
/// or <c>null</c> when the kind is <see cref="PackIconRemixIconKind.None"/>, so that the
/// MenuItem icon slot takes no space when no icon is assigned.
/// </summary>
public class IconKindToNullableIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PackIconRemixIconKind kind && kind != PackIconRemixIconKind.None)
            return new PackIconRemixIcon { Kind = kind };
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
