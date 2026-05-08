using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ValueConverters;

public class AiVendorToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.ToLowerInvariant() switch
            {
                "microsoft" => PackIconRemixIconKind.MicrosoftFill,
                "meta" => PackIconRemixIconKind.MetaFill,
                "alibaba cloud" => PackIconRemixIconKind.AlibabaCloudFill,
                "mistralai" => PackIconRemixIconKind.MixtralFill,
                "deepseek" => PackIconRemixIconKind.DeepseekFill,
                _ => PackIconRemixIconKind.BrainAi3Fill
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}