namespace TombLauncher.Core.Utils;

public static class GenericUtils
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.IsNullOrEmpty();
    }

    public static List<T> ToEmptyListIfNull<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable.IsNullOrEmpty())
            return new List<T>();

        return enumerable.ToList();
    }

    public static T Coalesce<T>(this T first, params T[] elements)
    {
        var enumerator = elements.GetEnumerator();
        if (!first?.Equals(default) == true)
        {
            return first;
        }

        while (enumerator.MoveNext())
        {
            var current = (T)enumerator.Current;
            if (!current?.Equals(default) == true)
                return current;
        }

        return default;
    }

    public static T DefaultIfEquals<T>(this T first, T second)
    {
        if (first?.Equals(second) == true)
            return default;
        return first;
    }
}