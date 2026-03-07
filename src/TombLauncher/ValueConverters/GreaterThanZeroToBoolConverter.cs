using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class GreaterThanZeroToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IConvertible convertible)
        {
            return convertible.ToDouble(CultureInfo.InvariantCulture) > 0;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}