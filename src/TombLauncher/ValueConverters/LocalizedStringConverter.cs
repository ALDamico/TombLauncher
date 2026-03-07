using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.ValueConverters;

public class LocalizedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Ioc.Default.GetService<ILocalizationManager>()?.GetLocalizedString((string?)parameter ?? string.Empty, value ?? new object()) ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}