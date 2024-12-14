using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Fastenshtein;
using TombLauncher.Core.Extensions;
using TombLauncher.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers;

public class GameSearchResultMetadataDistanceCalculator 
{
    public double Calculate(IGameSearchResultMetadata x, IGameSearchResultMetadata y)
    {
        var xKey = GetKey(x);
        var yKey = GetKey(y);

        ;
        if (xKey.Contains(yKey) || yKey.Contains(xKey))
            return 0;
        
        var lev = new Levenshtein(xKey);
        var dist = lev.DistanceFrom(yKey);
        var threshold = 3;
        if (IgnoreSubTitle)
        {
            threshold += 2;
        }
        
        if (dist < 5)
        {
            if (xKey.Contains("demo", StringComparison.InvariantCultureIgnoreCase) && !yKey.Contains("demo", StringComparison.InvariantCultureIgnoreCase) 
                || (!xKey.Contains("demo", StringComparison.InvariantCultureIgnoreCase) && yKey.Contains("demo", StringComparison.InvariantCultureIgnoreCase)))
                return double.MaxValue;
        }
        
        if (dist < 2)
        {
            var partRegex = new Regex(@"part(\d+)");
            var xMatch = partRegex.Match(xKey);
            var yMatch = partRegex.Match(yKey);
            if (xMatch.Success && yMatch.Success)
            {
                
                if (xMatch.Groups[1].Value == yMatch.Groups[1].Value)
                {
                    return 0;
                }

                return double.MaxValue;
            }
        }
        
        return (double)dist / Math.Abs(yKey.Length + xKey.Length);
    }

    private string GetKey(IGameSearchResultMetadata obj)
    {
        if (obj == null) return String.Empty;
        var key = obj.Title.RemoveDiacritics().RemoveIncidentals();
        /*if (IgnoreSubTitle)
        {
            if (key.Contains(" - "))
            {
                key = key.Substring(0, key.IndexOf(" - "));
            }
        }*/
        key = key.Remove(" ").ToLowerInvariant();
        if (UseAuthor)
        {
            key += "#" + obj.Author.RemoveDiacritics().RemoveIncidentals().Remove(" ").ToLowerInvariant();
        }

        return key;
    }

    public bool UseAuthor { get; set; }
    public bool IgnoreSubTitle { get; set; }
}