using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TombLauncher.ViewModels;

namespace TombLauncher.Utils;

public static class EnumUtils
{
    public static IEnumerable<EnumViewModel<T>> GetEnumViewModels<T>() where T: struct, Enum
    {
        return Enum.GetValues<T>().Select(e => new EnumViewModel<T>(e));
    }
}