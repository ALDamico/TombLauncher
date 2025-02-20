using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettingsViewModel : SettingsSectionViewModelBase
{
    public LanguageSettingsViewModel(PageViewModel settingsPage) : base("LANGUAGES", settingsPage)
    {
        InfoTipContent = "Localization infotip content".GetLocalizedString();
        InfoTipHeader = "Want to localize Tomb Launcher?".GetLocalizedString();
    }

    [ObservableProperty] private ApplicationLanguageViewModel _applicationLanguage;
    [ObservableProperty] private ObservableCollection<ApplicationLanguageViewModel> _availableLanguages;
}