namespace TombLauncher.Configuration.Sections;

public class AppearanceConfig : IAppearanceConfig
{
    public string? ApplicationTheme { get; set; }
    public bool DefaultToGridView { get; set; }
}
