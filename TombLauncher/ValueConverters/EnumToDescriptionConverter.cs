using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Enums;
using TombLauncher.Utils;

namespace TombLauncher.ValueConverters;

public class EnumToDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum e)
        {
            return e.GetDescription();
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (targetType != typeof(Enum)) return value;
            if (Enum.TryParse(targetType, str, out var parsed))
            {
                return parsed;
            }
        }

        if (value is Enum e)
        {
            return e.ToString();
        }

        return default;
    }
}