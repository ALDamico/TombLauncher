using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Mapping;

public class GameMetadataMapper
{
    public GameMetadataViewModel ToViewModel(IGameMetadata dto)
    {
        return new GameMetadataViewModel()
        {
            GameEngine = dto.GameEngine,
            Id = dto.Id,
            Title = dto.Title,
            Author = dto.Author ?? "",
            AuthorFullName = dto.AuthorFullName,
            CommunitySetupExecutable = dto.CommunitySetupExecutable,
            Description = dto.Description,
            Difficulty = dto.Difficulty,
            ExecutablePath = dto.ExecutablePath,
            Guid = dto.Guid,
            InstallDate = dto.InstallDate,
            InstallDirectory = dto.InstallDirectory,
            InstalledFromSiteDisplayName = dto.InstalledFromSiteDisplayName,
            IsCompleted = dto.IsCompleted,
            IsFavourite = dto.IsFavourite,
            IsInstalled = dto.IsInstalled,
            Length = dto.Length,
            ReleaseDate = dto.ReleaseDate,
            Setting = dto.Setting,
            SetupExecutable = dto.SetupExecutable,
            SetupExecutableArgs = dto.SetupExecutableArgs,
            TitlePic = ImageUtils.ToBitmap(dto.TitlePic)
        };
    }

    public IEnumerable<GameMetadataViewModel> ToViewModels(IEnumerable<GameMetadataDto> dtos) =>
        dtos.Select(ToViewModel);

    public ObservableCollection<GameMetadataViewModel> ToObservableCollection(IEnumerable<GameMetadataDto> dtos) =>
        ToViewModels(dtos).ToObservableCollection();

    public GameMetadataDto ToDto(GameMetadataViewModel viewModel)
    {
        return new GameMetadataDto()
        {
            Id = viewModel.Id,
            GameEngine = viewModel.GameEngine,
            Title = viewModel.Title,
            Author = viewModel.Author,
            AuthorFullName = viewModel.AuthorFullName,
            CommunitySetupExecutable = viewModel.CommunitySetupExecutable,
            Description = viewModel.Description,
            Difficulty = viewModel.Difficulty,
            ExecutablePath = viewModel.ExecutablePath,
            Guid = viewModel.Guid,
            InstallDate = viewModel.InstallDate,
            InstallDirectory = viewModel.InstallDirectory,
            InstalledFromSiteDisplayName = viewModel.InstalledFromSiteDisplayName,
            IsCompleted = viewModel.IsCompleted,
            IsFavourite = viewModel.IsFavourite,
            IsInstalled = viewModel.IsInstalled,
            Length = viewModel.Length,
            ReleaseDate = viewModel.ReleaseDate,
            Setting = viewModel.Setting,
            SetupExecutable = viewModel.SetupExecutable,
            SetupExecutableArgs = viewModel.SetupExecutableArgs,
            TitlePic = ImageUtils.ToByteArray(viewModel.TitlePic)
        };
    }

    public List<GameMetadataDto> ToDtos(IEnumerable<GameMetadataViewModel> viewModels) =>
        viewModels.Select(ToDto).ToList();

    public GameWithStatsViewModel ToViewModel(GameWithStatsDto dto, GameWithStatsService gameWithStatsService)
    {
        return new GameWithStatsViewModel(gameWithStatsService, ToViewModel(dto.GameMetadata))
        {
            AreCommandsVisible = false,
            LastPlayed = dto.LastPlayed,
            TotalPlayedTime = dto.TotalPlayedTime
        };
    }

    public List<GameWithStatsViewModel> ToViewModels(List<GameWithStatsDto> dtos,
        GameWithStatsService gameWithStatsService) =>
        dtos.Where(d => d != null!).Select(d => ToViewModel(d, gameWithStatsService)).ToList();
}