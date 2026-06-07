using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettingsViewModel : SettingsSectionViewModelBase
{
    public LanguageSettingsViewModel(PageViewModel settingsPage) : base("LANGUAGES", settingsPage, PackIconRemixIconKind.GlobalLine)
    {
        InfoTipContent = "LOCALIZATION_INFOTIP_CONTENT".GetLocalizedString();
        InfoTipHeader = "WANT_TO_LOCALIZE_TOMB_LAUNCHER".GetLocalizedString();
    }

    [ObservableProperty]
    public partial ApplicationLanguageViewModel? ApplicationLanguage { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ApplicationLanguageViewModel> AvailableLanguages { get; set; } = [];

    public override void ApplyTo(AppConfiguration userConfig)
    {
        if (ApplicationLanguage?.CultureInfo != null)
            userConfig.Application.ApplicationLanguage = ApplicationLanguage.CultureInfo.IetfLanguageTag;
    }
}