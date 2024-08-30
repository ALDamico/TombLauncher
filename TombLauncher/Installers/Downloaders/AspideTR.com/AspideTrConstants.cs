using System.Collections.Generic;
using System.Collections.Immutable;

namespace TombLauncher.Installers.Downloaders.AspideTR.com;

public static class AspideTrConstants
{
    // SETTINGS
    public const int Home = 1;
    public const int Egypt = 2;
    public const int Jungle = 3;
    public const int Civilization = 4;
    public const int ColdSnowy = 5;
    public const int City = 6;
    public const int SpaceAlien = 7;
    public const int Remake = 9;
    public const int BaseLab = 10;
    public const int Ship = 11;
    public const int Temple = 12;
    public const int VariousNc = 13;
    public const int Joke = 14;
    public const int Oriental = 15;
    public const int Train = 17;
    public const int Christmas = 20;
    public const int Easter = 21;
    public const int Venice = 22;
    public const int CaveCat = 23;
    public const int Coastal = 24;
    public const int Desert = 25;
    public const int WildWest = 26;
    public const int SouthAmerica = 27;
    public const int Greece = 28;
    public const int Rome = 29;
    public const int FantasySurreal = 30;
    public const int MysteryHorror = 31;
    public const int Steampunk = 32;
    public const int NordicViking = 33;
    public const int IslandAquatic = 34;
    public const int Library = 35;
    public const int Castle = 36;
    public const int Atlantis = 38;
    public const int Cambodia = 40;
    public const int SouthPacific = 41;
    public const int Persia = 42;

    // Other flags
    public const int Saga = 8;
    public const int Shooter = 16;
    public const int YoungLara = 18;
    public const int ChildFriendly = 19;
    public const int Demo = 39;
    public const int HighDifficulty = 43;
    public const int Peaceful = 44;

    public static ImmutableDictionary<string, int> Mappings = new Dictionary<string, int>()
    {
        { nameof(Atlantis).ToLowerInvariant(), Atlantis },
        { nameof(Desert).ToLowerInvariant(), Desert },
        { nameof(Christmas).ToLowerInvariant(), Christmas },
        { "spacealien", SpaceAlien },
        { "space", SpaceAlien },
        { "alien", SpaceAlien },
        { "baselab", BaseLab },
        { "base", BaseLab },
        { "lab", BaseLab },
        { nameof(Egypt).ToLowerInvariant(), Egypt },
        { nameof(Ship).ToLowerInvariant(), Ship },
        { nameof(Steampunk).ToLowerInvariant(), Steampunk },
        { nameof(Library).ToLowerInvariant(), Library },
        { "fantasysurreal", FantasySurreal },
        { "fantasy", FantasySurreal },
        { "surreal", FantasySurreal },
        { "nordicviking", NordicViking },
        { "nordic", NordicViking },
        { "viking", NordicViking },
        { nameof(SouthAmerica).ToLowerInvariant(), SouthAmerica },
        { nameof(Cambodia).ToLowerInvariant(), Cambodia },
        { "coldsnowy", ColdSnowy },
        { "cold", ColdSnowy },
        { "snowy", ColdSnowy },
        { nameof(Oriental).ToLowerInvariant(), Oriental },
        { nameof(SouthPacific).ToLowerInvariant(), SouthPacific },
        { nameof(Castle).ToLowerInvariant(), Castle },
        { nameof(Jungle).ToLowerInvariant(), Jungle },
        { nameof(Easter).ToLowerInvariant(), Easter },
        { nameof(Temple).ToLowerInvariant(), Temple },
        { nameof(CaveCat).ToLowerInvariant(), CaveCat },
        { "cave", CaveCat },
        { "catacomb", CaveCat },
        { "catacombs", CaveCat },
        { "cat", CaveCat },
        { nameof(Greece).ToLowerInvariant(), Greece },
        { nameof(Persia).ToLowerInvariant(), Persia },
        { nameof(Train).ToLowerInvariant(), Train },
        { nameof(City).ToLowerInvariant(), City },
        { nameof(Home).ToLowerInvariant(), Home },
        { "homeeng", Home },
        { "homeloc", Home },
        { nameof(Remake).ToLowerInvariant(), Remake },
        { nameof(VariousNc).ToLowerInvariant(), VariousNc },
        { "various", VariousNc },
        { "nc", VariousNc },
        { nameof(Civilization).ToLowerInvariant(), Civilization },
        { nameof(IslandAquatic).ToLowerInvariant(), IslandAquatic },
        { "island", IslandAquatic },
        { "aquatic", IslandAquatic },
        { nameof(Rome).ToLowerInvariant(), Rome },
        { nameof(Venice).ToLowerInvariant(), Venice },
        { nameof(Coastal).ToLowerInvariant(), Coastal },
        { nameof(MysteryHorror).ToLowerInvariant(), MysteryHorror },
        { "mystery", MysteryHorror },
        { "horror", MysteryHorror },
        { nameof(Joke), Joke },
        { nameof(WildWest).ToLowerInvariant(), WildWest },
        { "west", WildWest },
        { nameof(Saga).ToLowerInvariant(), Saga },
        { nameof(Peaceful).ToLowerInvariant(), Peaceful },
        { nameof(YoungLara).ToLowerInvariant(), YoungLara },
        { nameof(Demo).ToLowerInvariant(), Demo },
        { nameof(ChildFriendly).ToLowerInvariant(), ChildFriendly },
        { "child", ChildFriendly },
        { "children", ChildFriendly },
        { nameof(Shooter).ToLowerInvariant(), Shooter }
    }.ToImmutableDictionary();
}