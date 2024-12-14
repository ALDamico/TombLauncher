using System;
using System.Globalization;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;
using TombLauncher.Extensions;
using TombLauncher.Localization;

namespace TombLauncher.ValueConverters;

public class DateTimeToStringConverter : IValueConverter
{
    private Func<string, object[], string> StringGenerator(ILocalizationManager localizationManager)
    {
        if (localizationManager != null)
        {
            return localizationManager.GetLocalizedString;
        }

        return string.Format;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var localizationManager = Ioc.Default.GetService<ILocalizationManager>();
        var func = StringGenerator(localizationManager);
        if (value == null)
        {
            return func("Never", null);
        }

        if (value is not DateTime dateTime) return string.Empty;
        var dateTimeAtMidnight = dateTime.GetDateAtMidnight();
        var today = DateTime.Today;
        if (dateTimeAtMidnight == today)
        {
            return func("Today", null);
        }

        if (dateTimeAtMidnight.IsYesterday())
        {
            return func("Yesterday", null);
        }

        var differenceInDays = (today - dateTimeAtMidnight).Days;
        if (differenceInDays is > 1 and < 7)
        {
            return func("days ago", [differenceInDays]);
        }

        var weeksElapsed = differenceInDays / 7;
        switch (weeksElapsed)
        {
            case 1:
                return func("Last week", null);
            case < 4:
                return func("weeks ago", [weeksElapsed]);
        }

        switch (differenceInDays)
        {
            case <= 31:
                return func("Last month", null);
            case <= 365:
            {
                var monthsElapsed = differenceInDays % 365;
                return func("months ago", [monthsElapsed]);
            }
        }

        var yearsElapsed = today.Year - dateTimeAtMidnight.Year;
        return yearsElapsed == 1 ? func("Last year", null) : func("years ago", [yearsElapsed]);

    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}