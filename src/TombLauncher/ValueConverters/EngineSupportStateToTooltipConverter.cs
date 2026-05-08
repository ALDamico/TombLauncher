using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ValueConverters;

public class EngineSupportStateToTooltipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EngineSupportState engineSupportState)
        {
            return engineSupportState switch
            {
                EngineSupportState.NativePatchingAvailable => "SUPPORTED_NATIVE_PATCHING_AVAILABLE".GetLocalizedString(),
                EngineSupportState.SupportedWithCompatibilityLayer => "SUPPORTED_WITH_COMPATIBILITY_LAYER".GetLocalizedString(),
                EngineSupportState.FullSupport => "NATIVE_SUPPORT".GetLocalizedString(),
                _ => "NO_SUPPORT".GetLocalizedString()
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}