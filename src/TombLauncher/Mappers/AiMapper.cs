using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Ai.Services;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.Mappers;

public class AiMapper
{
    public AiModelViewModel ToViewModel(AiModelMetadata metadata, ModelDownloadService modelDownloadService,
        NotificationService notificationService)
    {
        return new AiModelViewModel(metadata, modelDownloadService, notificationService);
    }

    public AiModelMetadata ToDto(AiModelViewModel viewModel) => viewModel.Metadata;

    public IEnumerable<AiModelViewModel> ToViewModels(IEnumerable<AiModelMetadata> metadata,
        ModelDownloadService modelDownloadService,
        NotificationService notificationService) =>
        metadata.Select(m => ToViewModel(m, modelDownloadService, notificationService));

    public ObservableCollection<AiModelViewModel> ToObservableCollection(IEnumerable<AiModelMetadata> metadata,
        ModelDownloadService modelDownloadService, NotificationService notificationService) =>
        ToViewModels(metadata, modelDownloadService, notificationService).ToObservableCollection();
}
