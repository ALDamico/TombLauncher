using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TombLauncher.Contracts.Patchers;

namespace TombLauncher.ValueConverters;

public class LogSeverityToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogSeverity severity)
        {
            var key = severity switch
            {
                LogSeverity.Information => "TextFillColorPrimaryBrush",
                LogSeverity.Warning => "WarningBrush",
                LogSeverity.Error => "DangerBrush",
                LogSeverity.Success => "SuccessBrush",
                _ => "TextFillColorPrimaryBrush"
            };

            if (Application.Current!.TryFindResource(key, out var resource) && resource is IBrush brush)
                return brush;
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
