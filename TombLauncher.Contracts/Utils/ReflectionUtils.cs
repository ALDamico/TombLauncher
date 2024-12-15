using System.Reflection;

namespace TombLauncher.Contracts.Utils;

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

    public static IEnumerable<Type> GetImplementingTypes<T>()
    {
        var type = typeof(T);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p != type)
            .Where(p => type.IsAssignableFrom(p));
    }

    public static IEnumerable<T> GetImplementors<T>(BindingFlags bindingFlags = BindingFlags.Default)
    {
        return GetImplementingTypes<T>().Select(t => (T)Activator.CreateInstance(t));
    }

    public static Type GetTypeByName(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .FirstOrDefault(p => p.Name == typeName);
    }
}