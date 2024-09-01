using System.Collections.Generic;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameMerger
{
    IEqualityComparer<GameSearchResultMetadataViewModel> Comparer { get; }

    int Merge(ICollection<GameSearchResultMetadataViewModel> fullList, ICollection<GameSearchResultMetadataViewModel> addedElements);
    //void Merge(List<GameSearchResultMetadataViewModel>)
}