namespace TombLauncher.Core.Dtos;

public class AiModelMetadata
{
    public required string ModelId { get; init; }
    public required string FriendlyName { get; init; }
    public required string Vendor { get; init; }
    public required string Description { get; init; }
    public long? FileSizeBytes { get; set; }
}