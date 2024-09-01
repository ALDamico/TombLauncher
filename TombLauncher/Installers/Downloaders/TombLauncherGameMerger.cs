﻿using System.Collections.Generic;
using System.Linq;
using TombLauncher.Models;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public class TombLauncherGameMerger : IGameMerger
{
    public TombLauncherGameMerger(IEqualityComparer<GameSearchResultMetadataViewModel> comparer)
    {
        Comparer = comparer;
    }

    public IEqualityComparer<GameSearchResultMetadataViewModel> Comparer { get; }
    public int Merge(ICollection<GameSearchResultMetadataViewModel> fullList, ICollection<GameSearchResultMetadataViewModel> addedElements)
    {
        var unmatched = new List<GameSearchResultMetadataViewModel>(addedElements);
        var mergedCount = 0;
        foreach (var element in fullList)
        {
            var matching = addedElements.Where(e => Comparer.Equals(element, e)).ToList();
            mergedCount++;
            foreach (var match in matching)
            {
                unmatched.Remove(match);
                if (element.Author == null)
                {
                    element.Author = match.Author;
                }

                if (element.AuthorFullName == null)
                {
                    element.AuthorFullName = match.AuthorFullName;
                }

                if (element.Difficulty == null || element.Difficulty == GameDifficulty.Unknown)
                {
                    element.Difficulty = match.Difficulty;
                }

                if (element.Engine == null || element.Engine == GameEngine.Unknown)
                {
                    element.Engine = match.Engine;
                }

                if (element.Length == null || element.Length == GameLength.Unknown)
                {
                    element.Length = match.Length;
                }

                if (element.Rating is null or 0)
                {
                    element.Rating = match.Rating;
                }

                if (string.IsNullOrWhiteSpace(element.Setting))
                {
                    element.Setting = match.Setting;
                }

                if (element.TitlePic == null)
                {
                    element.TitlePic = match.TitlePic;
                }

                if (element.SizeInMb is null or 0)
                {
                    element.SizeInMb = match.SizeInMb;
                }

                if (element.ReleaseDate == null)
                {
                    element.ReleaseDate = match.ReleaseDate;
                }
            }
        }

        foreach (var element in unmatched)
        {
            fullList.Add(element);
        }

        return mergedCount;
    }
}