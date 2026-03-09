using System.Collections.ObjectModel;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AppearanceSettingsViewModel : SettingsSectionViewModelBase
{
    public AppearanceSettingsViewModel(PageViewModel settingsPage) : base("APPEARANCE", settingsPage, PackIconRemixIconKind.PaletteLine)
    {
        AvailableThemes = new ObservableCollection<ApplicationTheme>()
        {
            new ApplicationTheme("Scion (Dark)", "Scion", ThemeVariant.Dark),
            new ApplicationTheme("Scion (Light)", "Scion Light", ThemeVariant.Light),
            new ApplicationTheme("Xian (Dark)", "Xian", ThemeVariant.Dark),
            new ApplicationTheme("Xian (Light)", "Xian Light", ThemeVariant.Light),
        };
        // Default (will be overwritten by service)
        SelectedTheme = AvailableThemes[0];
    }

    [ObservableProperty] private ApplicationTheme _selectedTheme;
    [ObservableProperty] private ObservableCollection<ApplicationTheme> _availableThemes;
    [ObservableProperty] private bool _defaultToGridView;

    public override void ApplyTo(AppConfiguration userConfig)
    {
        if (SelectedTheme != null)
            userConfig.Appearance.ApplicationTheme = SelectedTheme.Value;
        userConfig.Appearance.DefaultToGridView = DefaultToGridView;
    }
}