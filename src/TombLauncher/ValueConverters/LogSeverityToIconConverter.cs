using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.ValueConverters;

public class LogSeverityToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Information => PackIconRemixIconKind.InformationLine,
                LogSeverity.Warning => PackIconRemixIconKind.AlertLine,
                LogSeverity.Error => PackIconRemixIconKind.CloseCircleLine,
                LogSeverity.Success => PackIconRemixIconKind.CheckboxCircleLine,
                _ => PackIconRemixIconKind.QuestionLine
            };
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
