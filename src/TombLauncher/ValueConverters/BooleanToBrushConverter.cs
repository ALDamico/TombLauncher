using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TombLauncher.ValueConverters;

public class BooleanToBrushConverter : IValueConverter
{
    public IBrush? TrueValue { get; set; }
    public IBrush? FalseValue { get; set; }
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueValue : FalseValue;
        }

        return FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}