namespace TombLauncher.Core.Extensions;

public static class GenericsExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable == null || !enumerable.Any();
    }

    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return !enumerable.IsNullOrEmpty();
    }

    public static List<T> ToEmptyListIfNull<T>(this IEnumerable<T>? enumerable)
    {
        // ReSharper disable PossibleMultipleEnumeration
        if (enumerable!.IsNullOrEmpty())
            return new List<T>();

        return enumerable!.ToList();
    }

    public static T? Coalesce<T>(this T? first, params T?[] elements)
    {
        if (!Equals(first, default(T)))
        {
            return first;
        }

        return elements.FirstOrDefault(e => !Equals(e, default(T)));
    }

    public static T? DefaultIfEquals<T>(this T? first, T? second)
    {
        if (first?.Equals(second) == true)
            return default;
        return first;
    }
}
