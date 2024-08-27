using System;
using System.Collections.Generic;
using System.Reflection;

namespace TombLauncher.Utils;

public class ReflectionUtils
{
    public static IEnumerable<KeyValuePair<string, string>> GetPropertiesAsKeyValuePairs(object o, Func<string, string> keyTransformation = null)
    {
        var list = new List<KeyValuePair<string, string>>();
        var properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(o);
            var stringPropertyValue = string.Empty;
            if (propertyValue != null)
            {
                stringPropertyValue = propertyValue.ToString();
            }
            var key = property.Name;
            if (keyTransformation != null)
            {
                key = keyTransformation(key);
            }
            list.Add(new KeyValuePair<string, string>(key, stringPropertyValue));
        }

        return list;
    }
}