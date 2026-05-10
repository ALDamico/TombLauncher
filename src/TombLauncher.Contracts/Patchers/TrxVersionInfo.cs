namespace TombLauncher.Contracts.Patchers;

public class TrxVersionInfo
{
    public required string InternalName { get; set; }
    public required string ExecutableName { get; set; }
    public required Version Version { get; set; }
}