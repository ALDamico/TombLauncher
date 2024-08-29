using System.Collections.Generic;
using System.Linq;
using TombLauncher.Models;

namespace TombLauncher.Dto.Extensions;

public static class GameMetadataDtoExtensions
{
    public static GameMetadataDto ToDto(this Game game)
    {
        return new GameMetadataDto()
        {
            Id = game.Id,
            Author = game.Author,
            GameEngine = game.GameEngine,
            InstallDate = game.InstallDate,
            ReleaseDate = game.ReleaseDate,
            Difficulty = game.Difficulty,
            Length = game.Length,
            Setting = game.Setting,
            Title = game.Title,
            ExecutablePath = game.ExecutablePath,
            InstallDirectory = game.InstallDirectory,
            Description = game.Description,
            TitlePic = game.TitlePic,
            AuthorFullName = game.AuthorFullName
        };
    }

    public static IEnumerable<GameMetadataDto> ToDtos(this IEnumerable<Game> games) => games.Select(ToDto);
}