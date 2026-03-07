using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class NotificationTypeToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType type)
        {
            var key = type switch
            {
                NotificationType.Info => "PrimaryBrush",
                NotificationType.Success => "SuccessBrush",
                NotificationType.Warning => "WarningBrush",
                NotificationType.Error => "DangerBrush",
                _ => "SecondaryBrush"
            };

            if (Application.Current!.TryFindResource(key, out var resource) && resource is IBrush brush)
            {
                return brush;
            }
        }

        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
