using TombLauncher.Core.Dtos;
using TombLauncher.Utils;
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
            InstallDirectory = game.InstallDirectory,
            Guid = game.Guid,
            TitlePic = ImageUtils.ToBitmap(game.TitlePic),
            AuthorFullName = game.AuthorFullName,
            IsInstalled = game.IsInstalled,
            SetupExecutable = game.SetupExecutable,
            CommunitySetupExecutable = game.CommunitySetupExecutable,
            SetupExecutableArgs = game.SetupExecutableArgs,
        };
    }
}