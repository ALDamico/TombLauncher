using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace TombLauncher.ValueConverters;

public class BooleanToMaterialIconConverter : IValueConverter
{
    public MaterialIconKind TrueValue { get; set; }
    public MaterialIconKind FalseValue { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueValue : FalseValue;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(bool))
        {
            return (MaterialIconKind)value == TrueValue;
        }

        return false;
    }
}