using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace TombLauncher.ViewModels;

/// <summary>
/// Caches which properties are marked with <see cref="IgnoreChangesAttribute"/>
/// per concrete type, so reflection runs only once per type.
/// </summary>
public static class IgnoreChangesHelper
{
    private static readonly ConcurrentDictionary<Type, HashSet<string>> Cache = new();

    public static bool IsIgnored(Type type, string? propertyName)
    {
        if (propertyName == null) return true;

        var ignored = Cache.GetOrAdd(type, t =>
        {
            var set = new HashSet<string>();

            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute<IgnoreChangesAttribute>() != null)
                    set.Add(prop.Name);
            }

            // CommunityToolkit.Mvvm: [ObservableProperty] on _fieldName generates FieldName property
            foreach (var field in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<IgnoreChangesAttribute>() != null)
                {
                    var name = field.Name.TrimStart('_');
                    name = char.ToUpperInvariant(name[0]) + name.Substring(1);
                    set.Add(name);
                }
            }

            return set;
        });

        return ignored.Contains(propertyName);
    }
}
