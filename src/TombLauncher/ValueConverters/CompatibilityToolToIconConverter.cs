using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class CompatibilityToolToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CompatibilityTool compatibilityTool)
        {
            switch (compatibilityTool)
            {
                case CompatibilityTool.WindowsNative:
                case CompatibilityTool.Automatic:
                    return PackIconRemixIconKind.FileUnknowFill;
                case CompatibilityTool.LinuxNative:
                    return PackIconRemixIconKind.UbuntuLine;
                case CompatibilityTool.Wine:
                    return PackIconRemixIconKind.Goblet2Line;
                case CompatibilityTool.Proton:
                    return PackIconRemixIconKind.SteamFill;
            }
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}