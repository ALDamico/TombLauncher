﻿using System.Collections.Generic;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public interface IGameMerger
{
    GameSearchResultMetadataDistanceCalculator Comparer { get; }

    int Merge(ICollection<IMultiSourceSearchResultMetadata> fullList, ICollection<IGameSearchResultMetadata> addedElements);
    //void Merge(List<GameSearchResultMetadataViewModel>)
}