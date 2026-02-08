using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.ViewModels;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AppearanceSettingsViewModel : SettingsSectionViewModelBase
{
    public AppearanceSettingsViewModel(PageViewModel settingsPage) : base("APPEARANCE", settingsPage)
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
}