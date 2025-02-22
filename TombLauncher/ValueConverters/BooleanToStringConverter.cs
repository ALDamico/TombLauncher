using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ValueConverters;

public class BooleanToStringConverter : IValueConverter
{
    public string TrueValue { get; set; }
    public string FalseValue { get; set; }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueValue.GetLocalizedString() : FalseValue.GetLocalizedString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (targetType == typeof(bool))
        {
            return (string)value == TrueValue.GetLocalizedString();
        }

        return false;
    }
}