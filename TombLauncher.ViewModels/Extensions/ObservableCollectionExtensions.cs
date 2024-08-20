using System.Collections.ObjectModel;

namespace TombLauncher.ExtensionMethods;

public static class ObservableCollectionExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    } 
}