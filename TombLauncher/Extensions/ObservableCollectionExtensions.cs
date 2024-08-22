using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TombLauncher.Extensions;

public static class ObservableCollectionExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    } 
}