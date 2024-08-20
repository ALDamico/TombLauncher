using TombLauncher.Database.Entities;
using TombLauncher.ViewModels.ViewModels;

namespace TombLauncher.ViewModels.Extensions;

public static class GameExtensions
{
    public static GameMetadataViewModel ToViewModel(this Game game)
    {
        return new GameMetadataViewModel()
        {
            Id = game.Id,
            Author = game.Author,
            GameEngine = game.GameEngine.GetDescription(),
            InstallDate = game.InstallDate,
            ReleaseDate = game.ReleaseDate,
            Title = game.Title,
            Setting = game.Setting,
            Difficulty = game.Difficulty.GetDescription(),
            Length = game.Length.GetDescription()
        };
    }

    public static IEnumerable<GameMetadataViewModel> ToViewModels(this IEnumerable<Game> games)
    {
        return games.Select(ToViewModel);
    }
}