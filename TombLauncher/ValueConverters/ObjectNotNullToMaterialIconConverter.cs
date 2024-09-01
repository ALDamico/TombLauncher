using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;
using Material.Icons.Avalonia;

namespace TombLauncher.ValueConverters;

public class ObjectNotNullToMaterialIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? FalseValue : TrueValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public MaterialIconKind TrueValue { get; set; }
    public MaterialIconKind FalseValue { get; set; }
}