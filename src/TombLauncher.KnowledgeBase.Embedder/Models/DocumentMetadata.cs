using System.Runtime.InteropServices;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.KnowledgeBase.Embedder.Models;

public class DocumentMetadata
{
    public required string DocumentTitle { get; set; }
    public List<GameEngine> AppliesTo { get; init; } = [];
    public List<OSPlatform> Platforms { get; init; } = [];
}