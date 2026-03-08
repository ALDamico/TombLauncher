using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Helpers;

namespace TombLauncher.ValueConverters;

public class TransferSpeedConverter : IValueConverter
{
    private static readonly string[] Units = ["B/s", "kB/s", "MB/s", "GB/s"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            return ByteSizeFormatHelper.Format(d, Units);
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}