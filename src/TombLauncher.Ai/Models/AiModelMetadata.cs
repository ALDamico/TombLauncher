using System.Globalization;

namespace TombLauncher.Ai.Models;

public record AiModelMetadata
{
    public required string ModelId { get; init; }
    public required string FriendlyName { get; init; }
    public required string Vendor { get; init; }
    public required string Description { get; init; }
    public Enum? Icon { get; init; }
    public AiModelClass AiModelClass { get; init; }
    public required string DownloadLink { get; init; }
    public required string FileName { get; init; }
    public List<CultureInfo> SupportedLanguages { get; init; } = [];
}