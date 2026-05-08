using NuGet.Versioning;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class KnowledgeBaseItemDto
{
    public string? DocumentTitle { get; set; }
    public string? SectionTitle { get; set; }
    public string? Text { get; set; }
    public VersionRange? AppVersionRange { get; set; }
    public Dictionary<GameEngine, VersionRange> EngineVersions { get; set; } = new();
    public List<Platform> Platforms { get; set; } = [];
    public List<GameEngine> AppliesTo { get; set; } = [];
    public string? Source { get; set; }
}