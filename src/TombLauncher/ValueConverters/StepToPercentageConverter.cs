using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class StepToPercentageConverter : IValueConverter
{
    public int MaxStep { get; set; }
    public int DecimalPlaces { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var maxStep = Math.Max(1, MaxStep);
        if (value is IConvertible c)
        {
            var perc = c.ToDouble(culture) / maxStep;
            return perc.ToString($"P{DecimalPlaces}", culture);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
