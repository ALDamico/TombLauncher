using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;
using TombLauncher.Helpers;
using TombLauncher.Core.Extensions;

namespace TombLauncher.ValueConverters;

public class DateTimeToStringConverter : IValueConverter
{
    private static Func<string, object[], string> StringGenerator(ILocalizationManager? localizationManager)
        => LocalizationHelper.GetStringGenerator(localizationManager);
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var localizationManager = Ioc.Default.GetService<ILocalizationManager>();
        var func = StringGenerator(localizationManager);
        if (value == null)
        {
            return func("NEVER", Array.Empty<object>());
        }

        if (value is not DateTime dateTime) return string.Empty;
        var dateTimeAtMidnight = dateTime.GetDateAtMidnight();
        var today = DateTime.Today;
        if (dateTimeAtMidnight == today)
        {
            return func("TODAY", Array.Empty<object>());
        }

        if (dateTimeAtMidnight.IsYesterday())
        {
            return func("YESTERDAY", Array.Empty<object>());
        }

        var differenceInDays = (today - dateTimeAtMidnight).Days;
        if (differenceInDays is > 1 and < 7)
        {
            return func("DAYS_AGO", [differenceInDays]);
        }

        var weeksElapsed = differenceInDays / 7;
        switch (weeksElapsed)
        {
            case 1:
                return func("LAST_WEEK", Array.Empty<object>());
            case < 4:
                return func("WEEKS_AGO", [weeksElapsed]);
        }

        switch (differenceInDays)
        {
            case <= 31:
                return func("LAST_MONTH", Array.Empty<object>());
            case <= 365:
                {
                    var monthsElapsed = differenceInDays / 30;
                    return func("MONTHS_AGO", [monthsElapsed]);
                }
        }

        var yearsElapsed = today.Year - dateTimeAtMidnight.Year;
        return yearsElapsed == 1 ? func("LAST_YEAR", Array.Empty<object>()) : func("YEARS_AGO", [yearsElapsed]);

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}