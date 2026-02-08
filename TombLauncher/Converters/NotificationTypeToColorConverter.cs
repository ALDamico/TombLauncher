using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Converters;

public class NotificationTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => Brushes.DodgerBlue,
                NotificationType.Success => Brushes.ForestGreen,
                NotificationType.Warning => Brushes.Goldenrod,
                NotificationType.Error => Brushes.Crimson,
                _ => Brushes.Gray
            };
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
