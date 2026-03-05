using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fastenshtein;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers;

public class GameSearchResultMetadataDistanceCalculator 
{
    private string[] GetAuthorsArray(IGameSearchResultMetadata searchResult)
    {
        return searchResult.Author.Split(',', StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries);
    }
    public double Calculate(IGameSearchResultMetadata x, IGameSearchResultMetadata y)
    {
        var levTitle = new Levenshtein(x.Title);
        var distTitle = levTitle.DistanceFrom(y.Title);

        var xAuthor = GetAuthorsArray(x);
        var yAuthor = GetAuthorsArray(y);
        var matchedAuthors = xAuthor.Intersect(yAuthor).Count();

        var isAuthorMatch = matchedAuthors / 100.0;
        if (distTitle + isAuthorMatch < 1)
            return distTitle + isAuthorMatch;

        return 1;
    }

    public bool UseAuthor { get; set; }
    public bool IgnoreSubTitle { get; set; }
}