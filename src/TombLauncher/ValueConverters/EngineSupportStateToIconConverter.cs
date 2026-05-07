using System;
using System.Globalization;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class EngineSupportStateToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EngineSupportState engineSupportState)
        {
            return engineSupportState switch
            {
                EngineSupportState.NativePatchingAvailable => PackIconRemixIconKind.WrenchFill,
                EngineSupportState.SupportedWithCompatibilityLayer => PackIconRemixIconKind.ToolsFill,
                EngineSupportState.FullSupport => PackIconRemixIconKind.CheckFill,
                _ => PackIconRemixIconKind.CloseCircleFill
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}