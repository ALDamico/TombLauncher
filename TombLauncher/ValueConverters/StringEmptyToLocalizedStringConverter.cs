using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ValueConverters;

public class StringEmptyToLocalizedStringConverter : IValueConverter
{
    public string EmptyValue { get; set; }
    public string NotEmptyValue { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return EmptyValue.GetLocalizedString();
        if (value is string str)
        {
            if (str.IsNullOrWhiteSpace())
                return EmptyValue.GetLocalizedString();
            return NotEmptyValue.GetLocalizedString();
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}