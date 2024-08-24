using System.Collections.Generic;
using System.Linq;
using TombLauncher.Dto;
using TombLauncher.ViewModels;

namespace TombLauncher.Extensions;

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
            Length = game.Length,
            Description = game.Description,
            ExecutablePath = game.ExecutablePath,
            InstallDirectory = game.InstallDirectory
        };
    }

    

    public static IEnumerable<GameMetadataViewModel> ToViewModels(this IEnumerable<GameMetadataDto> games)
    {
        return games.Select(ToViewModel);
    }

    public static GameMetadataDto ToDto(this GameMetadataViewModel game)
    {
        return new GameMetadataDto()
        {
            Id = game.Id,
            Author = game.Author,
            GameEngine = game.GameEngine,
            InstallDate = game.InstallDate,
            ReleaseDate = game.ReleaseDate,
            Title = game.Title,
            Setting = game.Setting,
            Difficulty = game.Difficulty,
            Length = game.Length,
            InstallDirectory = game.InstallDirectory,
            Description = game.Description,
            ExecutablePath = game.ExecutablePath
        };
    }
}