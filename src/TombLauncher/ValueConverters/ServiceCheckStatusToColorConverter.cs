using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class ServiceCheckStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ServiceCheckStatus status)
        {
            return status switch
            {
                ServiceCheckStatus.Unspecified => Brushes.SlateGray,
                ServiceCheckStatus.Checking => Brushes.SlateGray,
                ServiceCheckStatus.Error => Brushes.Red,
                ServiceCheckStatus.Okay => Brushes.ForestGreen,
                _ => Brushes.SlateGray
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}