using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class IntegrationsSettingsViewModel : SettingsSectionViewModelBase
{
    public IntegrationsSettingsViewModel(PageViewModel settingsPage) : base("EXTERNAL_INTEGRATIONS", settingsPage,
        PackIconRemixIconKind.PuzzleLine)
    {
    }
    [ObservableProperty] private bool _isDiscordSharingEnabled;

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Integrations.SharePlaySessionsOnDiscord = IsDiscordSharingEnabled;
        base.ApplyTo(userConfig);
    }
}