using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class NotificationTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NotificationType severity)
        {
            return severity switch
            {
                NotificationType.Info => PackIconRemixIconKind.InformationLine,
                NotificationType.Warning => PackIconRemixIconKind.AlertLine,
                NotificationType.Error => PackIconRemixIconKind.CloseCircleLine,
                NotificationType.Success => PackIconRemixIconKind.CheckboxCircleLine,
                _ => PackIconRemixIconKind.QuestionLine
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
