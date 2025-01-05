using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettingsViewModel : SettingsSectionViewModelBase
{
    public LanguageSettingsViewModel() : base("LANGUAGES")
    {
    }

    [ObservableProperty] private ApplicationLanguageViewModel _applicationLanguage;
    [ObservableProperty] private ObservableCollection<ApplicationLanguageViewModel> _availableLanguages;
}