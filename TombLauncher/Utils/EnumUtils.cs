using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TombLauncher.ViewModels;

namespace TombLauncher.Utils;

public static class EnumUtils
{
    public static string GetDescription(this Enum value)
    {
        if (value == null)
            return string.Empty;
        var type = value.GetType();
        var descriptionAttribute = (DescriptionAttribute[])type.GetField(value.ToString()).GetCustomAttributes<DescriptionAttribute>(false);
        return descriptionAttribute.Length > 0 ? descriptionAttribute[0].Description : string.Empty;
    }

    public static IEnumerable<EnumViewModel<T>> GetEnumViewModels<T>() where T: struct, Enum
    {
        return Enum.GetValues<T>().Select(e => new EnumViewModel<T>(e));
    }
}