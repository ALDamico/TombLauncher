using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Models;

public abstract class MetadataBase
{
    public List<GameEngine> AppliesTo { get; init; } = [];
    public List<Platform> Platforms { get; init; } = [];
    public string? Source { get; set; }
}