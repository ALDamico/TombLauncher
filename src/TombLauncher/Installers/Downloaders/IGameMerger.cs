using System.Collections.Generic;
using TombLauncher.Contracts.Downloaders;

namespace TombLauncher.Installers.Downloaders;

public interface IGameMerger
{
    GameSearchResultMetadataDistanceCalculator Comparer { get; }

    int Merge(ICollection<IMergedGameSearchResultMetadata> fullList, ICollection<IGameSearchResultMetadata> addedElements);
}