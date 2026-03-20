using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Enums;
using TombLauncher.ViewModels;

namespace TombLauncher.ValueConverters;

public class GameIsInstallingToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is InstallProgressViewModel vm)
        {
            return vm.InstallStatus != InstallStatus.Completed;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}