using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class FilenameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return Path.GetFileName(str);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}