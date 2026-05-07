namespace TombLauncher.Ai.Models;

public class KbManifest
{
    public const int SupportedSchemaVersion = 1;

    public DateTimeOffset GeneratedAt { get; set; }
    public int SchemaVersion { get; set; }
    public string Sha256 { get; set; } = "";
}
