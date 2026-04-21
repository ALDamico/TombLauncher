using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Ai.Services;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Ai;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Mappers;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.ViewModels.Pages;

public partial class SettingsPageViewModel : PageViewModel, IChangeTracking
{
    public SettingsPageViewModel(SettingsPageService settingsService, 
        ISettingsProvider settingsProvider, 
        IPopupService popupService, 
        IPlatformSpecificFeatures platformSpecificFeatures, 
        IAppFileOperationsService appFileOperationsService, 
        ILayeredAppConfiguration appConfiguration,
        SettingsMapper settingsMapper,
        AiModelRegistry aiModelRegistry,
        IHttpClientFactory httpClientFactory,
        ModelDownloadService modelDownloadService,
        NotificationService notificationService)
    {
        _settingsService = settingsService;
        _settingsProvider = settingsProvider;
        _popupService = popupService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _appFileOperationsService = appFileOperationsService;
        _appConfiguration = appConfiguration;
        _settingsMapper = settingsMapper;
        _aiModelRegistry = aiModelRegistry;
        _httpClientFactory = httpClientFactory;
        _modelDownloadService = modelDownloadService;
        _notificationService = notificationService;
        Sections = new ObservableCollection<SettingsSectionViewModelBase>();

        Sections.CollectionChanged += (_, args) =>
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
    private readonly IPopupService _popupService;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly IAppFileOperationsService _appFileOperationsService;
    private readonly ILayeredAppConfiguration _appConfiguration;
    private readonly AiModelRegistry _aiModelRegistry;
    private readonly SettingsMapper _settingsMapper;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ModelDownloadService _modelDownloadService;
    private readonly NotificationService _notificationService;
    [ObservableProperty] private ObservableCollection<SettingsSectionViewModelBase> _sections;

    private void SectionPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(IsChanged))
            return;
        RaiseCanExecuteChanged(SaveCommand);
    }

    private void SectionErrorChanged(object? sender, DataErrorsChangedEventArgs args)
    {
        OnPropertyChanged(nameof(IsChanged));
        OnPropertyChanged(nameof(HasPendingEdits));
        RaiseCanExecuteChanged(SaveCommand);
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        if (IsInitialized)
            return;

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
            AvailableLanguages = supportedLanguages.OrderBy(l => l.DisplayName).ToObservableCollection(),
            ApplicationLanguage = supportedLanguages.FirstOrDefault(l =>
                _settingsService.LocalizationManager.CurrentCulture.Equals(l.CultureInfo))
        };

        var downloaders = _settingsService.GetDownloaderViewModels();
        var downloaderSettings = new DownloaderSettingsViewModel(this, _settingsProvider, _appFileOperationsService, _popupService, _platformSpecificFeatures, _settingsMapper)
        {
            AvailableDownloaders = downloaders.ToObservableCollection()
        };
        var gameDetailsSettings = _settingsService.GetGameDetailsSettings(this);
        var savegameSettings = _settingsService.GetSavegameSettings(this);

        var wp = _appConfiguration.WelcomePage;
        var welcomePageSettings = new WelcomePageSettingsViewModel(this)
        {
            ShowQuickStats = wp.ShowQuickStats.GetValueOrDefault(true),
            ShowQuickActions = wp.ShowQuickActions.GetValueOrDefault(true),
            ShowRecentlyPlayed = wp.ShowRecentlyPlayed.GetValueOrDefault(true),
            ShowFavourites = wp.ShowFavourites.GetValueOrDefault(true),
            RecentlyPlayedCount = wp.RecentlyPlayedCount.GetValueOrDefault(5),
            FavouritesCount = wp.FavouritesCount.GetValueOrDefault(5),
            ShowRandomSuggestion = wp.ShowRandomSuggestion.GetValueOrDefault(true),
            MaxRerolls = wp.RandomGameMaxRerolls.GetValueOrDefault(10)
        };

        var aiCoreSettings = _settingsProvider.GetAiCoreSettings();

        var aiSettings = new AiSettingsViewModel(this)
        {
            AvailableModels = _aiModelRegistry.AvailableModels.Select(m => new AiModelViewModel(m, _modelDownloadService, _notificationService)).ToObservableCollection(),
            GpuOffloadLevel = (int)(aiCoreSettings.GpuOffloadPercentage.GetValueOrDefault() * AiConstants.MaxOffloadLevel),
            IsEnabled = aiCoreSettings.IsEnabled,
        };

        foreach (var model in aiCoreSettings.ModelSizes)
        {
            aiSettings.AvailableModels.FirstOrDefault(m => m.Metadata.ModelId == model.Key)?.FileSizeBytes =
                model.Value;
        }

        aiSettings.SelectedModel =
            aiSettings.AvailableModels.SingleOrDefault(m => m.Metadata.ModelId == aiCoreSettings.ModelName);

        _ = Task.WhenAll(aiSettings.AvailableModels.Where(m => m.FileSizeBytes == null).Select(async m => await FetchSize(m)));

        Sections.Add(aiSettings);
        Sections.Add(appearanceSettings);
        Sections.Add(languageSettings);
        Sections.Add(downloaderSettings);
        Sections.Add(gameDetailsSettings);
        Sections.Add(savegameSettings);
        Sections.Add(welcomePageSettings);

        var compat = _appConfiguration.Compatibility;
        var compatVm = new CompatibilitySettingsViewModel(this, _platformSpecificFeatures)
        {
            WinePath = compat.WinePath ?? string.Empty,
            CompatibilityPrefixPath = compat.CompatibilityPrefixPath ?? string.Empty,
            SelectedTool = compat.CompatibilityTool,
            ManualProtonPath = compat.ProtonPath,
        };
        // Pre-select the stored Proton installation in the ComboBox if it matches
        if (!string.IsNullOrWhiteSpace(compat.ProtonPath))
        {
            compatVm.SelectedProtonInstallation =
                compatVm.AvailableProtonInstallations.FirstOrDefault(p => p.ExecutablePath == compat.ProtonPath);
        }
        Sections.Add(compatVm);
        Sections.Add(aiSettings);
        AcceptChanges();
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
        RaiseCanExecuteChanged(SaveCommand);
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
    
    private async Task FetchSize(AiModelViewModel m)
    {
        m.IsFetchingSize = true;
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            m.FileSizeBytes = await httpClient.FetchSizeAsync(m.Metadata.DownloadLink, CancellationToken.None);
        }
        catch
        {
            // FileSizeBytes rimane null → la view mostra "dimensione sconosciuta"
        }
        finally
        {
            m.IsFetchingSize = false;
        }
    }
}