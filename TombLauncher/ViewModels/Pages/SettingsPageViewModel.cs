using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel, IChangeTracking
{
    public SettingsPageViewModel()
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        LanguageSettings = new LanguageSettingsViewModel();
        DownloaderSettings = new DownloaderSettingsViewModel();
        AppearanceSettings = new AppearanceSettingsViewModel();
        RandomGameSettings = new RandomGameSettingsViewModel();
        Sections = new ObservableCollection<SettingsSectionViewModelBase>()
        {
            LanguageSettings, DownloaderSettings, AppearanceSettings, RandomGameSettings
        };
        foreach (var section in Sections)
        {
            section.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                    return;
                RaiseCanExecuteChanged(SaveCmd);
            };
            section.ErrorsChanged += (sender, args) =>
            {
                OnPropertyChanged(nameof(IsChanged));
                RaiseCanExecuteChanged(SaveCmd);
            };
        }
        Initialize += InitializeSettings;
    }

    private SettingsService _settingsService;
    [ObservableProperty] private LanguageSettingsViewModel _languageSettings;
    [ObservableProperty] private DownloaderSettingsViewModel _downloaderSettings;
    [ObservableProperty] private AppearanceSettingsViewModel _appearanceSettings;
    [ObservableProperty] private GameDetailsSettingsViewModel _gameDetailsSettings;
    [ObservableProperty] private RandomGameSettingsViewModel _randomGameSettings;
    [ObservableProperty] private ObservableCollection<SettingsSectionViewModelBase> _sections;

    private void InitializeSettings()
    {
        var supportedLanguages = _settingsService.GetSupportedLanguages();
        LanguageSettings.AvailableLanguages = supportedLanguages.ToObservableCollection();
        LanguageSettings.ApplicationLanguage =
            supportedLanguages.FirstOrDefault(l => _settingsService.LocalizationManager.CurrentCulture.Equals(l.CultureInfo));

        var downloaders = _settingsService.GetDownloaderViewModels();
        DownloaderSettings.AvailableDownloaders = downloaders.ToObservableCollection();
        GameDetailsSettings = _settingsService.GetGameDetailsSettings();
        RandomGameSettings.MaxRerolls = _settingsService.GetRandomGameMaxRerolls();
        AcceptChanges();
    }

    protected override async Task SaveInner()
    {
        await _settingsService.Save(this);
        AcceptChanges();
    }

    protected override bool CanSave()
    {
        return IsChanged;
    }

    public void AcceptChanges()
    {
        foreach (var section in Sections)
        {
            section.AcceptChanges();
        }
        OnPropertyChanged(nameof(IsChanged));
        RaiseCanExecuteChanged(SaveCmd);
    }

    public bool IsChanged
    {
        get
        {
            var anyChanged = Sections.Any(s => s.IsChanged);
            var anyErrors = Sections.Any(s => s.HasErrors);
            return anyChanged && !anyErrors;
        }
    }
}