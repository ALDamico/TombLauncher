using System.Collections.Generic;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameMerger
{
    GameSearchResultMetadataDistanceCalculator Comparer { get; }

    int Merge(ICollection<MultiSourceGameSearchResultMetadataViewModel> fullList, ICollection<GameSearchResultMetadataViewModel> addedElements);
    //void Merge(List<GameSearchResultMetadataViewModel>)
}