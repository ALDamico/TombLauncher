using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.Mappers;

public class AiMapper
{
    public AiModelViewModel ToViewModel(AiModelMetadata metadata,
        NotificationService notificationService)
    {
        return new AiModelViewModel(metadata, notificationService);
    }

    public AiModelMetadata ToDto(AiModelViewModel viewModel) => viewModel.Metadata;

    public IEnumerable<AiModelViewModel> ToViewModels(IEnumerable<AiModelMetadata> metadata,
        NotificationService notificationService) =>
        metadata.Select(m => ToViewModel(m, notificationService));

    public ObservableCollection<AiModelViewModel> ToObservableCollection(IEnumerable<AiModelMetadata> metadata, NotificationService notificationService) =>
        ToViewModels(metadata, notificationService).ToObservableCollection();
}
