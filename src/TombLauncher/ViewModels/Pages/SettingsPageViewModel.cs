using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel, IChangeTracking
{
    public SettingsPageViewModel(SettingsPageService settingsService, ISettingsProvider settingsProvider, IMessageBoxService messageBoxService, IPlatformSpecificFeatures platformSpecificFeatures, MapperConfiguration mapperConfiguration)
    {
        _settingsService = settingsService;
        _settingsProvider = settingsProvider;
        _messageBoxService = messageBoxService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _mapperConfiguration = mapperConfiguration;
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

    private readonly SettingsPageService _settingsService;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly MapperConfiguration _mapperConfiguration;
    [ObservableProperty] private ObservableCollection<SettingsSectionViewModelBase> _sections;

    private void SectionPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(IsChanged))
            return;
        RaiseCanExecuteChanged(SaveCmd);
    }

    private void SectionErrorChanged(object? sender, DataErrorsChangedEventArgs args)
    {
        OnPropertyChanged(nameof(IsChanged));
        OnPropertyChanged(nameof(HasPendingEdits));
        RaiseCanExecuteChanged(SaveCmd);
    }

    public override Task OnNavigatedTo(object parameter)
    {
        if (IsInitialized)
            return Task.CompletedTask;

        IsInitialized = true;
        var appearanceCoreSettings = _settingsProvider.GetAppearanceSettings();
        var currentTheme = appearanceCoreSettings.ApplicationTheme;
        var appearanceSettings = new AppearanceSettingsViewModel(this);
        appearanceSettings.SelectedTheme = appearanceSettings.AvailableThemes.FirstOrDefault(t => t.Value == currentTheme)
                                           ?? appearanceSettings.AvailableThemes.First();
        appearanceSettings.DefaultToGridView = appearanceCoreSettings.IsGridViewDefault;
        var supportedLanguages = _settingsService.GetSupportedLanguages();
        var languageSettings = new LanguageSettingsViewModel(this)
        {
            AvailableLanguages = supportedLanguages.ToObservableCollection(),
            ApplicationLanguage = supportedLanguages.FirstOrDefault(l =>
                _settingsService.LocalizationManager.CurrentCulture?.Equals(l.CultureInfo) == true)
        };

        var downloaders = _settingsService.GetDownloaderViewModels();
        var downloaderSettings = new DownloaderSettingsViewModel(this, _settingsProvider, Ioc.Default.GetRequiredService<IAppFileOperationsService>(), _messageBoxService, _platformSpecificFeatures, _mapperConfiguration)
        {
            AvailableDownloaders = downloaders.ToObservableCollection()
        };
        var gameDetailsSettings = _settingsService.GetGameDetailsSettings(this);
        var savegameSettings = _settingsService.GetSavegameSettings(this);

        var welcomePageSettings = new WelcomePageSettingsViewModel(this)
        {
            ShowQuickStats = _settingsService.GetShowQuickStats(),
            ShowQuickActions = _settingsService.GetShowQuickActions(),
            ShowRecentlyPlayed = _settingsService.GetShowRecentlyPlayed(),
            ShowFavourites = _settingsService.GetShowFavourites(),
            RecentlyPlayedCount = _settingsService.GetRecentlyPlayedCount(),
            FavouritesCount = _settingsService.GetFavouritesCount(),
            ShowRandomSuggestion = _settingsService.GetShowRandomSuggestion(),
            MaxRerolls = _settingsProvider.GetApplicationSettings().RandomGameMaxRerolls
        };

        Sections.Add(appearanceSettings);
        Sections.Add(languageSettings);
        Sections.Add(downloaderSettings);
        Sections.Add(gameDetailsSettings);
        Sections.Add(savegameSettings);
        Sections.Add(welcomePageSettings);
        AcceptChanges();
        return Task.CompletedTask;
    }

    protected override async Task SaveInner()
    {
        await _settingsService.Save(this);
        AcceptChanges();
    }

    protected override bool CanSave()
    {
        return IsChanged && !HasPendingEdits;
    }

    public void AcceptChanges()
    {
        foreach (var section in Sections)
        {
            if (!section.EditInProgress)
                section.AcceptChanges();
        }

        OnPropertyChanged(nameof(IsChanged));
        OnPropertyChanged(nameof(HasPendingEdits));
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

    public bool HasPendingEdits
    {
        get
        {
            return Sections.Any(s => s.EditInProgress);
        }
    }
}