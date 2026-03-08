using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Helpers;

namespace TombLauncher.ValueConverters;

public class FileSizeConverter : IValueConverter
{
    private static readonly string[] Units = ["bytes", "kB", "MB", "GB"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double d;

        if (value is long l)
            d = l;
        else if (value is double dv)
            d = dv;
        else
            return string.Empty;

        return ByteSizeFormatHelper.Format(d, Units);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}