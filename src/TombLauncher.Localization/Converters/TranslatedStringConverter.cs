using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.Localization.Converters;

public class TranslatedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key) return value;

        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null) return key;

        return localizationManager[key];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
