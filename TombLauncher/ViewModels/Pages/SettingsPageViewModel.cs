using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Core.Extensions;
using TombLauncher.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel
{
    public SettingsPageViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LanguageSettings = new LanguageSettings();
        Initialize += InitializeSettings;
    }

    private SettingsService _settingsService;
    [ObservableProperty] private LanguageSettings _languageSettings;

    private void InitializeSettings()
    {
        LanguageSettings.AvailableLanguages = _settingsService.GetSupportedLanguages().ToObservableCollection();
        LanguageSettings.ApplicationLanguage = CultureInfo.CurrentUICulture;
    }

    protected override async void SaveInner()
    {
        await _settingsService.Save(this);
    }

    protected override bool CanSave()
    {
        return true;
    }
}