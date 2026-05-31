namespace TombLauncher.Contracts.Extensions;

public static class NullableExtensions
{
    public static T? NullIf<T>(this T? value, T? targetNull)
    {
        if (value?.Equals(targetNull) == true)
            return default;

        return value;
    }
}