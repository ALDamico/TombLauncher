using System.Collections.ObjectModel;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.Extensions;

public static class CollectionsExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }

    public static T PickOneAtRandom<T>(this ICollection<T> collection)
    {
        if (collection.Count == 0)
        {
            throw new InvalidOperationException("Cannot pick an element from an empty collection.");
        }

        var index = Random.Shared.Next(collection.Count);
        return collection.ElementAt(index);
    }

    public static IEnumerable<T> GetCheckedItems<T>(this IEnumerable<CheckableItem<T>> enumerable)
        where T : IEquatable<T>
    {
        return enumerable.Where(i => i.IsChecked).Select(i => i.Value);
    }

    public static List<CheckableItem<T>> MergeWithOverrides<T>(this IEnumerable<CheckableItem<T>> defaults,
        IEnumerable<CheckableItem<T>> overrides) where T : IEquatable<T>
    {
        if (overrides == null || !overrides.Any())
        {
            return defaults.ToList();
        }

        var nonOverriddenDefaults = defaults.Where(d => overrides.All(o => !o.Value.Equals(d.Value)));
        return nonOverriddenDefaults.Concat(overrides).ToList();
    }
}