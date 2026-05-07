namespace TombLauncher.Ai.Models;

public class KbManifest
{
    public DateTimeOffset GeneratedAt { get; set; }
    public int SchemaVersion { get; set; }
    public string Sha256 { get; set; } = "";
}
