using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ValueConverters;

public class FullScreenToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is WindowState windowState)
        {
            return windowState == WindowState.FullScreen
                ? PackIconRemixIconKind.FullscreenExitLine
                : PackIconRemixIconKind.FullscreenLine;
        }

        return BindingOperations.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}