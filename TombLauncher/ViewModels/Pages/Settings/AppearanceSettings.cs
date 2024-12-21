using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class AppearanceSettings : ViewModelBase
{
    public AppearanceSettings()
    {
        SelectedTheme = Application.Current.ActualThemeVariant;
        AvailableThemes = new ObservableCollection<ThemeVariant>()
        {
            ThemeVariant.Default,
            ThemeVariant.Light,
            ThemeVariant.Dark
        };
    }
    private ThemeVariant _selectedTheme;

    public ThemeVariant SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            _selectedTheme = value;
            //Application.Current.RequestedThemeVariant = SelectedTheme;
            OnPropertyChanged();
        }
    }
    [ObservableProperty] private ObservableCollection<ThemeVariant> _availableThemes;
}