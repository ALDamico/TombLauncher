using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        LanguageSettings = new LanguageSettingsViewModel();
        DownloaderSettings = new DownloaderSettings();
        AppearanceSettings = new AppearanceSettingsViewModel();
        Initialize += InitializeSettings;
    }

    private SettingsService _settingsService;
    [ObservableProperty] private LanguageSettingsViewModel _languageSettings;
    [ObservableProperty] private DownloaderSettings _downloaderSettings;
    [ObservableProperty] private AppearanceSettingsViewModel _appearanceSettings;
    [ObservableProperty] private GameDetailsSettings _gameDetailsSettings;

    private void InitializeSettings()
    {
        var supportedLanguages = _settingsService.GetSupportedLanguages();
        LanguageSettings.AvailableLanguages = supportedLanguages.ToObservableCollection();
        LanguageSettings.ApplicationLanguage =
            supportedLanguages.FirstOrDefault(l => _settingsService.LocalizationManager.CurrentCulture.Equals(l.CultureInfo));

        var downloaders = _settingsService.GetDownloaderViewModels();
        DownloaderSettings.AvailableDownloaders = downloaders.ToObservableCollection();
        GameDetailsSettings = _settingsService.GetGameDetailsSettings();
    }

    protected override async Task SaveInner()
    {
        await _settingsService.Save(this);
    }

    protected override bool CanSave()
    {
        return true;
    }
}