using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using TombLauncher.Core.Extensions;

namespace TombLauncher.ValueConverters;

public class StringNullToNullBitmapConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var themeVariant = App.Current.ActualThemeVariant;
        if (value is string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                if (themeVariant == ThemeVariant.Dark)
                {
                    return DarkThemeVariantValue;
                }

                return LightThemeVariantValue;
            }

            return str;
        }

        if (value == null)
        {
            if (themeVariant == ThemeVariant.Dark)
            {
                return DarkThemeVariantValue;
            }

            return LightThemeVariantValue;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (str == null || str == LightThemeVariantValue || str == DarkThemeVariantValue)
                return null;
            return str;
        }

        return value;
    }
    
    public string LightThemeVariantValue { get; set; }
    public string DarkThemeVariantValue { get; set; }
}