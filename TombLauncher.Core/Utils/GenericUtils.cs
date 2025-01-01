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
}