using OpenAI.Models;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Mappers;

public class ModelMapper
{
    public AiModelMetadata ToMetadata(OpenAIModel openAiModel)
    {
        return new AiModelMetadata
        {
            ModelId = openAiModel.Id,
            FriendlyName = "",
            Vendor = "",
            Description = "",
            DownloadLink = "",
            FileName = "",
        };
    }

    public List<AiModelMetadata> ToMetadataList(IEnumerable<OpenAIModel> openAiModels) =>
        openAiModels.Select(ToMetadata).ToList();
}