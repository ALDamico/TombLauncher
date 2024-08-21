using TombLauncher.Database.Dto;
using TombLauncher.Database.Entities;
using TombLauncher.Utils;
using TombLauncher.ViewModels.ViewModels;

namespace TombLauncher.ViewModels.Extensions;

public static class GameExtensions
{
    public static GameMetadataViewModel ToViewModel(this GameMetadataDto game)
    {
        return new GameMetadataViewModel()
        {
            Id = game.Id,
            Author = game.Author,
            GameEngine = game.GameEngine,
            InstallDate = game.InstallDate,
            ReleaseDate = game.ReleaseDate,
            Title = game.Title,
            Setting = game.Setting,
            Difficulty = game.Difficulty,
            Length = game.Length
        };
    }

    

    public static IEnumerable<GameMetadataViewModel> ToViewModels(this IEnumerable<GameMetadataDto> games)
    {
        return games.Select(ToViewModel);
    }
}