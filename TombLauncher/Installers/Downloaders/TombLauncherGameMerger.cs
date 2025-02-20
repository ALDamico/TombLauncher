using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Models;
using TombLauncher.Services;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public class TombLauncherGameMerger : IGameMerger
{
    public TombLauncherGameMerger(GameSearchResultMetadataDistanceCalculator comparer)
    {
        Comparer = comparer;
    }

    public GameSearchResultMetadataDistanceCalculator Comparer { get; }
    public int Merge(ICollection<IMultiSourceSearchResultMetadata> fullList, ICollection<IGameSearchResultMetadata> addedElements)
    {
        var unmatched = new List<IGameSearchResultMetadata>(addedElements);
        var mergedCount = 0;
        foreach (var element in fullList)
        {
            var matching = addedElements.Select(e => new { Score = Comparer.Calculate(element, e), Element = e })
                .Where(e => e.Score <= 0.25)
                .GroupBy(e => e.Score)
                .MinBy(e => e.Key)?.Select(e => e.Element);
            if (matching == null) continue;
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

                if (element.Difficulty == GameDifficulty.Unknown)
                {
                    element.Difficulty = match.Difficulty;
                }

                if (element.Engine == GameEngine.Unknown)
                {
                    element.Engine = match.Engine;
                }

                if (element.Length == GameLength.Unknown)
                {
                    element.Length = match.Length;
                }

                if (element.Rating is null or 0)
                {
                    element.Rating = match.Rating;
                }

                if (element.Setting.IsNullOrWhiteSpace())
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
                
                element.Sources.Add(match);
            }
        }

        foreach (var element in unmatched)
        {
            fullList.Add(new MultiSourceSearchResultMetadataDto()
            {
                Author = element.Author,
                Difficulty = element.Difficulty,
                Description = element.Description,
                Engine = element.Engine,
                Length = element.Length,
                Rating = element.Rating,
                Setting = element.Setting,
                Sources = new HashSet<IGameSearchResultMetadata>(){element},
                Title = element.Title,
                BaseUrl = element.BaseUrl,
                SourceSiteDisplayName = element.SourceSiteDisplayName,
                DetailsLink = element.DetailsLink,
                DownloadLink = element.DownloadLink,
                ReleaseDate = element.ReleaseDate,
                ReviewsLink = element.ReviewsLink,
                TitlePic = element.TitlePic,
                WalkthroughLink = element.WalkthroughLink,
                AuthorFullName = element.AuthorFullName,
                SizeInMb = element.SizeInMb
            });
        }

        return mergedCount;
    }
}