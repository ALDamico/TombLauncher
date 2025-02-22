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
        Sections = new ObservableCollection<SettingsSectionViewModelBase>();

        Sections.CollectionChanged += (sender, args) =>
        {
            if (args.NewItems != null)
            {
                foreach (var item in args.NewItems)
                {
                    var section = (item as SettingsSectionViewModelBase)!;
                    section.PropertyChanged += SectionPropertyChanged;
                    section.ErrorsChanged += SectionErrorChanged;
                }
            }

            if (args.OldItems != null)
            {
                foreach (var item in args.OldItems)
                {
                    var section = (item as SettingsSectionViewModelBase)!;
                    section.PropertyChanged -= SectionPropertyChanged;
                    section.ErrorsChanged -= SectionErrorChanged;
                }
            }
        };
    }

    private readonly SettingsService _settingsService;
    [ObservableProperty] private ObservableCollection<SettingsSectionViewModelBase> _sections;

    private void SectionPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(IsChanged))
            return;
        RaiseCanExecuteChanged(SaveCmd);
    }

    private void SectionErrorChanged(object sender, DataErrorsChangedEventArgs args)
    {
        OnPropertyChanged(nameof(IsChanged));
        RaiseCanExecuteChanged(SaveCmd);
    }

    protected override Task RaiseInitialize()
    {
        var currentTheme = _settingsService.GetApplicationTheme();
        var appearanceSettings = new AppearanceSettingsViewModel(this)
        {
            SelectedTheme = currentTheme,
            DefaultToGridView = _settingsService.IsGridViewDefault()
        };
        var supportedLanguages = _settingsService.GetSupportedLanguages();
        var languageSettings = new LanguageSettingsViewModel(this)
        {
            AvailableLanguages = supportedLanguages.ToObservableCollection(),
            ApplicationLanguage = supportedLanguages.FirstOrDefault(l =>
                _settingsService.LocalizationManager.CurrentCulture.Equals(l.CultureInfo))
        };

        var downloaders = _settingsService.GetDownloaderViewModels();
        var downloaderSettings = new DownloaderSettingsViewModel(this)
        {
            AvailableDownloaders = downloaders.ToObservableCollection()
        };
        var gameDetailsSettings = _settingsService.GetGameDetailsSettings(this);
        var randomGameSettings = new RandomGameSettingsViewModel(this)
        {
            MaxRerolls = _settingsService.GetRandomGameMaxRerolls()
        };
        var savegameSettings = _settingsService.GetSavegameSettings(this);

        Sections.Add(appearanceSettings);
        Sections.Add(languageSettings);
        Sections.Add(downloaderSettings);
        Sections.Add(gameDetailsSettings);
        Sections.Add(randomGameSettings);
        Sections.Add(savegameSettings);
        AcceptChanges();
        return base.RaiseInitialize();
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