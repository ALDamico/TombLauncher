using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Ai.Factories;
using TombLauncher.Ai.Services;
using TombLauncher.Configuration;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
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
        AiMapper aiMapper,
        NotificationService notificationService,
        AiBackendFactory aiBackendFactory,
        KbUpdateService kbUpdateService)
    {
        _settingsService = settingsService;
        _settingsProvider = settingsProvider;
        _popupService = popupService;
        _platformSpecificFeatures = platformSpecificFeatures;
        _appFileOperationsService = appFileOperationsService;
        _appConfiguration = appConfiguration;
        _settingsMapper = settingsMapper;
        _aiMapper = aiMapper;
        _notificationService = notificationService;
        _aiBackendFactory = aiBackendFactory;
        _kbUpdateService = kbUpdateService;
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
    private readonly SettingsMapper _settingsMapper;
    private readonly AiMapper _aiMapper;
    private readonly NotificationService _notificationService;
    private readonly AiBackendFactory _aiBackendFactory;
    private readonly KbUpdateService _kbUpdateService;
    
    [ObservableProperty]
    public partial ObservableCollection<SettingsSectionViewModelBase> Sections { get; set; }

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

    private async Task InitAppearanceSettings()
    {
        var appearanceCoreSettings = _settingsProvider.GetAppearanceSettings();
        var currentTheme = appearanceCoreSettings.ApplicationTheme;
        var appearanceSettings = new AppearanceSettingsViewModel(this);
        appearanceSettings.SelectedTheme =
            appearanceSettings.AvailableThemes.FirstOrDefault(t => t.Value == currentTheme)
            ?? appearanceSettings.AvailableThemes.First();
        appearanceSettings.DefaultToGridView = appearanceCoreSettings.IsGridViewDefault;
        Sections.Add(appearanceSettings);
    }

    private async Task InitLanguageSettings()
    {
        var supportedLanguages = await _settingsService.GetSupportedLanguages();
        var languageSettings = new LanguageSettingsViewModel(this)
        {
            AvailableLanguages = supportedLanguages.OrderBy(l => l.IsSystemLanguage)
                .ThenBy(l => l.DisplayName).ToObservableCollection(),
            ApplicationLanguage = supportedLanguages.FirstOrDefault(l =>
                _settingsService.LocalizationManager.CurrentCulture.Equals(l.CultureInfo))
        };
        Sections.Add(languageSettings);
    }

    private async Task InitWelcomePageSettings()
    {
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
        
        Sections.Add(welcomePageSettings);
    }

    private async Task InitDownloaderSettings()
    {
        var downloaders = _settingsService.GetDownloaderViewModels();
        var downloaderSettings = new DownloaderSettingsViewModel(this, _settingsProvider, _appFileOperationsService,
            _popupService, _platformSpecificFeatures, _settingsMapper)
        {
            AvailableDownloaders = downloaders.ToObservableCollection()
        };
        
        Sections.Add(downloaderSettings);
    }

    private async Task InitGameDetailsSettings()
    {
        var gameDetailsSettings = _settingsService.GetGameDetailsSettings(this);
        Sections.Add(gameDetailsSettings);
    }

    private async Task InitSavegameSettings()
    {
        var savegameSettings = _settingsService.GetSavegameSettings(this);
        Sections.Add(savegameSettings);
    }

    private async Task InitAiSettings()
    {
        var aiCoreSettings = _settingsProvider.GetAiCoreSettings();

        var aiSettings = new AiSettingsViewModel(this, _aiBackendFactory, _aiMapper, _notificationService, _kbUpdateService)
        {
            AvailableModels = [],
            IsEnabled = aiCoreSettings.IsEnabled,
            SavedModelId = aiCoreSettings.ModelId,
            EmbeddingModelId = aiCoreSettings.EmbeddingModelId,
            ApiKey = aiCoreSettings.ApiKey,
            SelectedBackendType = aiCoreSettings.BackendType,
            Endpoint = aiCoreSettings.Endpoint,
            Temperature = aiCoreSettings.Temperature
        };
        Sections.Add(aiSettings);
    }

    private async Task InitCompatibilitySettings()
    {
        var compat = _appConfiguration.Compatibility;
        var compatVm = new CompatibilitySettingsViewModel(this, _platformSpecificFeatures)
        {
            WinePath = compat.WinePath ?? string.Empty,
            CompatibilityPrefixPath = compat.CompatibilityPrefixPath ?? string.Empty,
            SelectedTool = compat.CompatibilityTool,
            ManualProtonPath = compat.ProtonPath,
        };
        if (!string.IsNullOrWhiteSpace(compat.ProtonPath))
        {
            compatVm.SelectedProtonInstallation =
                compatVm.AvailableProtonInstallations.FirstOrDefault(p => p.ExecutablePath == compat.ProtonPath);
        }
        Sections.Add(compatVm);
    }

    private async Task InitLoggingSettings()
    {
        var loggingSettings = new LoggingSettingsViewModel(_platformSpecificFeatures, this);
        Sections.Add(loggingSettings);
    }

    private async Task InitIntegrationsSettings()
    {
        var externalIntegrationsSettings = new IntegrationsSettingsViewModel(this)
        {
            IsDiscordSharingEnabled = _appConfiguration.Integrations.SharePlaySessionsOnDiscord.GetValueOrDefault()
        };
        Sections.Add(externalIntegrationsSettings);
    }

    private async Task InitGamepadSettings()
    {
        var gamepadSettings = new GamepadSettingsViewModel(this, _appConfiguration, _popupService, _platformSpecificFeatures)
        {
            GamepadTool = _appConfiguration.Gamepad.GamepadTool.GetValueOrDefault(),
            GamepadToolPath = _appConfiguration.Gamepad.ToolPath
        };
        Sections.Add(gamepadSettings);
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        if (IsInitialized)
            return;

        using (BusyScope("LOADING_SETTINGS".GetLocalizedString()))
        {
            await InitWelcomePageSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitAppearanceSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitLanguageSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitDownloaderSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitGameDetailsSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitSavegameSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitAiSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitCompatibilitySettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitLoggingSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitIntegrationsSettings();
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

            await InitGamepadSettings();

            AcceptChanges();
            IsInitialized = true;
        }
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
}