namespace TombLauncher.Configuration.Sections;

public class UpdaterConfig : IUpdaterConfig
{
    public string? AppCastUrl { get; set; }
    public string? AppCastPublicKey { get; set; }
    public bool UpdaterUseLocalPaths { get; set; }
    public string? UpdateChannelName { get; set; }
}
