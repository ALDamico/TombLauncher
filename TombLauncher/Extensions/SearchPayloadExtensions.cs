using TombLauncher.Contracts.Downloaders;
using TombLauncher.Installers.Downloaders;
using TombLauncher.ViewModels;

namespace TombLauncher.Extensions;

public static class SearchPayloadExtensions
{
    public static DownloaderSearchPayload ToDto(this DownloaderSearchPayloadViewModel viewModel)
    {
        return new DownloaderSearchPayload()
        {
            Duration = viewModel.Duration,
            Rating = viewModel.Rating,
            AuthorName = viewModel.AuthorName,
            GameDifficulty = viewModel.GameDifficulty,
            GameEngine = viewModel.GameEngine,
            LevelName = viewModel.LevelName
        };
    }
}