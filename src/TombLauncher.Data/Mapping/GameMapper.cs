using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Mapping;

public class GameMapper
{
    private readonly EnvironmentVariableMapper _environmentVariableMapper;

    public GameMapper(EnvironmentVariableMapper environmentVariableMapper)
    {
        _environmentVariableMapper = environmentVariableMapper;
    }

    public GameMetadataDto ToDto(Game game)
    {
        return new GameMetadataDto()
        {
            Id = game.Id,
            GameEngine = game.GameEngine,
            Title = game.Title,
            Author = game.Author,
            AuthorFullName = game.AuthorFullName,
            CommunitySetupExecutable = game.FileBackups
                .FirstOrDefault(b => b.FileType == FileType.CommunitySetupExecutable)?.FileName,
            Description = game.Description ?? "",
            Difficulty = game.Difficulty,
            ExecutablePath = game.FileBackups.OrderByDescending(b => b.BackedUpOn).FirstOrDefault(b => b.FileType == FileType.GameExecutable)?.FileName,
            Guid = game.Guid,
            InstallDate = game.InstallDate,
            InstallDirectory = game.InstallDirectory,
            InstalledFromSiteDisplayName = game.InstalledFromLink?.DisplayName,
            IsCompleted = game.IsCompleted,
            IsFavourite = game.IsFavourite,
            IsInstalled = game.IsInstalled,
            Length = game.Length,
            ReleaseDate = game.ReleaseDate,
            Setting = game.Setting,
            SetupExecutable = game.FileBackups.FirstOrDefault(b => b.FileType == FileType.SetupExecutable)?.FileName,
            SetupExecutableArgs =
                game.FileBackups.FirstOrDefault(b => b.FileType == FileType.SetupExecutable)?.Arguments,
            TitlePic = game.TitlePic ?? [],
            CompatibilityPrefixPath = game.CompatibilityPrefixPath,
            CompatibilityTool = game.CompatibilityTool,
            CompatibilityToolPath = game.CompatibilityToolPath,
            ExtraEnvVars = _environmentVariableMapper.ToDtos(game.EnvironmentVariables).ToList()
        };
    }

    public List<GameMetadataDto> ToDtos(IEnumerable<Game> games) => games.Select(ToDto).ToList();

    public Game ToGame(IGameMetadata dto)
    {
        return new Game()
        {
            Title = dto.Title,
            InstallDirectory = dto.InstallDirectory ?? "",
            Id = dto.Id,
            GameEngine = dto.GameEngine,
            Author = dto.Author,
            AuthorFullName = dto.AuthorFullName,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            Guid = dto.Guid,
            InstallDate = dto.InstallDate,
            IsCompleted = dto.IsCompleted,
            IsFavourite = dto.IsFavourite,
            IsInstalled = dto.IsInstalled,
            Length = dto.Length,
            ReleaseDate = dto.ReleaseDate,
            Setting = dto.Setting,
            TitlePic = dto.TitlePic,
            CompatibilityTool = dto.CompatibilityTool,
            CompatibilityToolPath = dto.CompatibilityToolPath,
            CompatibilityPrefixPath = dto.CompatibilityPrefixPath,
        };
    }
}