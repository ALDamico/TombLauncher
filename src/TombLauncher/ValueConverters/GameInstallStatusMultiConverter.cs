using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Enums;
using TombLauncher.ViewModels;

namespace TombLauncher.ValueConverters;

public class GameInstallStatusMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not InstallProgressViewModel)
            return false;
        var installStatus = (InstallStatus?)values[1];

        return installStatus is InstallStatus.Indeterminate or InstallStatus.Downloading or InstallStatus.Installing;
    }
}