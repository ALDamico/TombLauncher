﻿using System;
using System.Collections.Generic;
using Fastenshtein;
using TombLauncher.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers;

public class GameSearchResultMetadataEqualityComparer : EqualityComparer<GameSearchResultMetadataViewModel>
{
    public override bool Equals(GameSearchResultMetadataViewModel x, GameSearchResultMetadataViewModel y)
    {
        var xKey = GetKey(x);
        var yKey = GetKey(y);
        var lev = new Levenshtein(xKey);
        var dist = lev.DistanceFrom(yKey);
        var threshold = 5;
        if (IgnoreSubTitle)
        {
            threshold += 20;
        }

        return dist <= threshold;
    }

    public override int GetHashCode(GameSearchResultMetadataViewModel obj)
    {
        return obj.GetHashCode();
    }

    private string GetKey(GameSearchResultMetadataViewModel obj)
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