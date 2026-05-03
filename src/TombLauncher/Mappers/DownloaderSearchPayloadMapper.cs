using TombLauncher.Contracts.Downloaders;
using TombLauncher.ViewModels;

namespace TombLauncher.Mappers;

public class DownloaderSearchPayloadMapper
{
    public DownloaderSearchPayload ToDto(DownloaderSearchPayloadViewModel vm)
    {
        return new DownloaderSearchPayload()
        {
            GameEngine = vm.GameEngine,
            LevelName = vm.LevelName,
            AuthorName = vm.AuthorName,
            Duration = vm.Duration,
            GameDifficulty = vm.GameDifficulty,
            Rating = vm.Rating
        };
    }
}