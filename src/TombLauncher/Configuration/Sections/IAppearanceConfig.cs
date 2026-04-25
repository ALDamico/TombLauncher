namespace TombLauncher.Configuration.Sections;

public interface IAppearanceConfig
{
    string? ApplicationTheme { get; }
    bool DefaultToGridView { get; }
}
