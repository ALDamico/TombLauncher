using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AppearanceSettingsViewModel : SettingsSectionViewModelBase
{
    public AppearanceSettingsViewModel(PageViewModel settingsPage) : base("APPEARANCE", settingsPage)
    {
        SelectedTheme = Application.Current.ActualThemeVariant;
        AvailableThemes = new ObservableCollection<ThemeVariant>()
        {
            ThemeVariant.Default,
            ThemeVariant.Light,
            ThemeVariant.Dark
        };
    }
    
    [ObservableProperty] private ThemeVariant _selectedTheme;
    [ObservableProperty] private ObservableCollection<ThemeVariant> _availableThemes;
    [ObservableProperty] private bool _defaultToGridView;
}