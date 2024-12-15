using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.Controls.ValueConverters;

public sealed class CountryIdToFlagImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var countryId = value as string;

        if (countryId == null)
            return null;
        try
        {
            var path = $"avares://TombLauncher.Controls/Assets/LipisFlags/{countryId.ToLower()}.svg";
            return new Uri(path, UriKind.Absolute);
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}