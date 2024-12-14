using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;
using TombLauncher.Localization;

namespace TombLauncher.ValueConverters;

public class DateTimeFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        if (value is DateTime dateTime)
        {
            if (dateTime == default)
                return string.Empty;
            if (string.IsNullOrWhiteSpace(DesiredFormat))
            {
                return dateTime.ToString(localizationManager.CurrentCulture.DateTimeFormat);
            }
            return dateTime.ToString(localizationManager[DesiredFormat]);
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        if (targetType == typeof(DateTime))
        {
            if (DateTime.TryParse(value?.ToString(), localizationManager.CurrentCulture, out var dateTime))
            {
                return dateTime;
            }
        }

        if (value is DateTime dt) return dt;
        return null;
    }

    public string DesiredFormat { get; set; }
}