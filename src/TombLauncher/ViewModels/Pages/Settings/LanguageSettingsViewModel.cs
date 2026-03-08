using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettingsViewModel : SettingsSectionViewModelBase
{
    public LanguageSettingsViewModel(PageViewModel settingsPage) : base("LANGUAGES", settingsPage, PackIconRemixIconKind.GlobalLine)
    {
        InfoTipContent = "LOCALIZATION_INFOTIP_CONTENT".GetLocalizedString();
        InfoTipHeader = "WANT_TO_LOCALIZE_TOMB_LAUNCHER".GetLocalizedString();
    }

    [ObservableProperty] private ApplicationLanguageViewModel? _applicationLanguage;
    [ObservableProperty] private ObservableCollection<ApplicationLanguageViewModel> _availableLanguages = new ObservableCollection<ApplicationLanguageViewModel>();
}