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
            return str switch
            {
                "Microsoft" => PackIconRemixIconKind.MicrosoftFill,
                "Meta" => PackIconRemixIconKind.MetaFill,
                "Alibaba Cloud" => PackIconRemixIconKind.AlibabaCloudFill,
                "Mistral AI" => null,
                "Cohere" => null,
                _ => PackIconRemixIconKind.BrainLine
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}