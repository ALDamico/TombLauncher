using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.Utils;

public static class CollectionUtils
{
    public static List<CheckableItem<T>> GetSharedItems<T>(IEnumerable<CheckableItem<T>> first,
        IEnumerable<CheckableItem<T>> second) where T : IEquatable<T>
    {
        if (second.IsNullOrEmpty())
        {
            return first.ToList();
        }

        var sharedElements = first.Where(p => second.All(p2 => !p2.Value.Equals(p.Value)));
        return sharedElements.Concat(second).ToList();
    }
}