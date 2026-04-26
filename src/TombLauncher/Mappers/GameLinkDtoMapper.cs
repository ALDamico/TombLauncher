using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels;

namespace TombLauncher.Mappers;

public class GameLinkDtoMapper
{
    public GameLinkViewModel ToViewModel(GameLinkDto dto)
    {
        return new GameLinkViewModel()
        {
            DisplayName = dto.DisplayName,
            BaseUrl = dto.BaseUrl,
            Id = dto.Id,
            Link = dto.Link,
            LinkType = dto.LinkType,
        };
    }

    public IEnumerable<GameLinkViewModel> ToViewModels(IEnumerable<GameLinkDto> dtos) => dtos.Select(ToViewModel);

    public ObservableCollection<GameLinkViewModel> ToObservableCollection(IEnumerable<GameLinkDto> dtos) =>
        ToViewModels(dtos).ToObservableCollection();
}