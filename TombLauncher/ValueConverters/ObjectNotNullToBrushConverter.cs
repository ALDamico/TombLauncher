using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TombLauncher.ValueConverters;

public class ObjectNotNullToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return FalseValue;
        return TrueValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public IBrush TrueValue { get; set; }
    public IBrush FalseValue { get; set; }
}