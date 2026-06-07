using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.PlatformSpecific.Models;

public class GameSupportMatrixEntry
{
    public GameEngine Engine { get; set; }
    public EngineSupportState SupportState { get; set; }
    public required string ShortDescription { get; set; }
    public string? LongDescription { get; set; }
}