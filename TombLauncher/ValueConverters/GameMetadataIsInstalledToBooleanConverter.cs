using System;
using System.Globalization;

namespace TombLauncher.ValueConverters;

public class GameMetadataIsInstalledToBooleanConverter : GameMetadataIsNotInstalledToBooleanConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var retValue = base.Convert(value, targetType, parameter, culture);
        if (retValue is bool b)
        {
            return !b;
        }

        return false;
    }
}