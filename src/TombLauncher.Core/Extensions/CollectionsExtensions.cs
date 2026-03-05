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
        var random = new Random();
        var upperBound = collection.Count - 1;
        var index = random.Next(upperBound);
        return collection.ElementAt(index);
    }

    public static IEnumerable<T> GetCheckedItems<T>(this IEnumerable<CheckableItem<T>> enumerable)
        where T : IEquatable<T>
    {
        return enumerable.Where(i => i.IsChecked).Select(i => i.Value);
    }
}