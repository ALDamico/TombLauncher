using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.ViewModels;

namespace TombLauncher.ValueConverters;

public class GameMetadataIsNotInstalledToBooleanConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return true;
       
        if (value is GameWithStatsViewModel gameWithStats)
        {
            value = gameWithStats.GameMetadata;
        }
        
        if (value is GameMetadataViewModel vm)
        {
            return !vm.IsInstalled;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}