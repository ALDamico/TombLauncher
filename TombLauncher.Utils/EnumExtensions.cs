using System.ComponentModel;
using System.Reflection;

namespace TombLauncher.Utils;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var descriptionAttribute = type.GetCustomAttribute(typeof(DescriptionAttribute), false) as DescriptionAttribute;
        if (descriptionAttribute is null)
        {
            return string.Empty;
        }

        return descriptionAttribute.Description;
    }
}