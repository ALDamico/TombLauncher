using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.ValueConverters;

public class MultiSourceGameSearchResultMetadataCanDownloadToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;
        var castedValue = value as MultiSourceGameSearchResultMetadataViewModel;
        if (castedValue == null)
            return false;
        if (castedValue.InstalledGame != null)
        {
            if (castedValue.InstalledGame.GameMetadata != null)
            {
                return castedValue.InstalledGame.GameMetadata.IsInstalled;
            }
        }

        return !castedValue.DownloadLink.IsNullOrWhiteSpace();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}