using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using TombLauncher.Dto;
using TombLauncher.ViewModels;

namespace TombLauncher.Extensions;

public static class GameExtensions
{
    public static GameMetadataViewModel ToViewModel(this GameMetadataDto game)
    {
        Bitmap bitmap = null;
        if (game.TitlePic != null)
        {
            var imageMemoryStream = new MemoryStream(game.TitlePic);
            bitmap = new Bitmap(imageMemoryStream);
        }
        
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
            TitlePic = bitmap
        };
    }

    

    public static IEnumerable<GameMetadataViewModel> ToViewModels(this IEnumerable<GameMetadataDto> games)
    {
        return games.Select(ToViewModel);
    }

    public static GameMetadataDto ToDto(this GameMetadataViewModel game)
    {
        byte[] titlePic = null;
        if (game.TitlePic != null)
        {
            using var memoryStream = new MemoryStream();
            game.TitlePic.Save(memoryStream);
            titlePic = memoryStream.ToArray();
        }
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
            ExecutablePath = game.ExecutablePath,
            Guid = game.Guid,
            TitlePic = titlePic
        };
    }
}