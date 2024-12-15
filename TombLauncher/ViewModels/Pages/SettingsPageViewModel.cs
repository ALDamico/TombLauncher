using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel
{
    public SettingsPageViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        LanguageSettings = new LanguageSettings();
        DownloaderSettings = new DownloaderSettings();
        Initialize += InitializeSettings;
    }

    private SettingsService _settingsService;
    [ObservableProperty] private LanguageSettings _languageSettings;
    [ObservableProperty] private DownloaderSettings _downloaderSettings;

    private void InitializeSettings()
    {
        var supportedLanguages = _settingsService.GetSupportedLanguages();
        LanguageSettings.AvailableLanguages = supportedLanguages.ToObservableCollection();
        LanguageSettings.ApplicationLanguage =
            supportedLanguages.FirstOrDefault(l => CultureInfo.CurrentUICulture.Equals(l.CultureInfo));

        var downloaders = _settingsService.GetDownloaders();
        DownloaderSettings.AvailableDownloaders = downloaders.ToObservableCollection();
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