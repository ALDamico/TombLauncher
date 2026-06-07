namespace TombLauncher.Core.Utils;

public static class DictionaryExtensions
{
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary,
        IDictionary<TKey, TValue>? other) where TKey : notnull
    {
        dictionary ??= new Dictionary<TKey, TValue>();
        if (other == null)
            return dictionary;
        foreach (var (key, value) in other)
        {
            dictionary.TryAdd(key, value);
        }

        return dictionary;
    }
}