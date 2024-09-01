using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

public class GreaterThanZeroToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is byte b)
        {
            return b > 0;
        }

        if (value is double d)
        {
            return d > 0;
        }

        if (value is decimal dec)
        {
            return dec > 0;
        }

        if (value is int i)
        {
            return i > 0;
        }

        if (value is long l)
        {
            return l > 0;
        }

        if (value is short s)
        {
            return s > 0;
        }

        if (value is sbyte sb)
        {
            return sb > 0;
        }

        if (value is float f)
        {
            return f > 0;
        }

        if (value is uint u)
        {
            return u > 0;
        }

        if (value is ushort us)
        {
            return us > 0;
        }

        if (value is ulong ul)
        {
            return ul > 0;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}