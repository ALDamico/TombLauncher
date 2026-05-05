using OpenAI.Models;
using TombLauncher.Ai.Models;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Mappers;

public class ModelMapper
{
    public AiModelMetadata ToMetadata(OpenAIModel openAiModel)
    {
        return new AiModelMetadata
        {
            ModelId = openAiModel.Id,
            FriendlyName = openAiModel.Id,
            Vendor = "",
            Description = "",
            FileSizeBytes = null
        };
    }

    public List<AiModelMetadata> ToMetadataList(IEnumerable<OpenAIModel> openAiModels) =>
        openAiModels.Select(ToMetadata).ToList();

    internal AiModelMetadata ToMetadata(ModelInfo modelInfo)
    {
        return new AiModelMetadata()
        {
            ModelId = modelInfo.Key,
            Description = modelInfo.Description ?? "",
            FriendlyName = modelInfo.DisplayName ?? modelInfo.SelectedVariant ?? modelInfo.Key,
            Vendor = modelInfo.Publisher,
            FileSizeBytes = modelInfo.SizeBytes
        };
    }

    internal List<AiModelMetadata> ToMetadataList(IEnumerable<ModelInfo> modelInfos) =>
        modelInfos.Select(ToMetadata).ToList();
}