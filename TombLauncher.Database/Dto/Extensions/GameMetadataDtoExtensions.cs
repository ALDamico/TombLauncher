using TombLauncher.Database.Entities;
using TombLauncher.Utils;

namespace TombLauncher.Database.Dto.Extensions;

public static class GameMetadataDtoExtensions
{
    public static GameMetadataDto ToDto(this Game game)
    {
        return new GameMetadataDto()
        {
            Id = game.Id,
            Author = game.Author,
            GameEngine = game.GameEngine.GetDescription(),
            InstallDate = game.InstallDate,
            ReleaseDate = game.ReleaseDate,
            Difficulty = game.Difficulty.GetDescription(),
            Length = game.Length.GetDescription(),
            Setting = game.Setting,
            Title = game.Title,
            ExecutablePath = game.ExecutablePath,
            InstallDirectory = game.InstallDirectory,
        };
    }

    public static IEnumerable<GameMetadataDto> ToDtos(this IEnumerable<Game> games) => games.Select(ToDto);
}