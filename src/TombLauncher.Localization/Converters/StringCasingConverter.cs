using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.Localization.Converters;

public enum StringCasing
{
    Normal,
    Lower,
    Upper,
    Title
}

public class StringCasingConverter : IValueConverter
{
    public StringCasing Casing { get; set; } = StringCasing.Normal;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str || string.IsNullOrEmpty(str))
            return value;

        var effectiveCasing = Casing;
        if (parameter is string paramStr && Enum.TryParse<StringCasing>(paramStr, true, out var parsedCasing))
        {
            effectiveCasing = parsedCasing;
        }

        return effectiveCasing switch
        {
            StringCasing.Lower => str.ToLower(culture),
            StringCasing.Upper => str.ToUpper(culture),
            StringCasing.Title => culture.TextInfo.ToTitleCase(str.ToLower(culture)),
            _ => str
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
