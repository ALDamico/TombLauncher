using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.ValueConverters;

public class MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not MultiSourceGameSearchResultMetadataViewModel vm)
            return false;

        if (vm.InstalledGame?.GameMetadata is { IsInstalled: true })
            return true;

        return !vm.DownloadLink.IsNullOrWhiteSpace();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}