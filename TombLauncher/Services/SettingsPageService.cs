using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Utils;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Navigation;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Services;

public class SettingsPageService : IViewService
{
    public SettingsPageService(
        ViewServiceContext viewContext,
        IAppConfigurationWrapper appConfiguration,
        ILogger<SettingsPageService> logger,
        ThemeManager themeManager,
        IServiceProvider serviceProvider,
        IAppFileOperationsService appFileOperations,
        ISettingsProvider settingsProvider)
    {
        ViewContext = viewContext;
        _appConfiguration = appConfiguration;
        _logger = logger;
        _themeManager = themeManager;
        _serviceProvider = serviceProvider;
        _appFileOperations = appFileOperations;
        _settingsProvider = settingsProvider;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly IAppConfigurationWrapper _appConfiguration;
    private readonly IServiceProvider _serviceProvider;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper _mapper => ViewContext.Mapper;
    private readonly ILogger<SettingsPageService> _logger;
    private readonly ThemeManager _themeManager;
    private readonly IAppFileOperationsService _appFileOperations;
    private readonly ISettingsProvider _settingsProvider;

    public List<ApplicationLanguageViewModel> GetSupportedLanguages()
    {
        var supportedLanguages = LocalizationManager.GetSupportedLanguages();
        return _mapper.Map<List<ApplicationLanguageViewModel>>(supportedLanguages);
    }

    public CultureInfo GetApplicationLanguage() => _settingsProvider.GetApplicationSettings().ApplicationLanguage;

    public string GetApplicationTheme() => _settingsProvider.GetAppearanceSettings().ApplicationTheme;

    public async Task Save(SettingsPageViewModel viewModel)
    {
        viewModel.SetBusy(true, "Saving application settings...".GetLocalizedString());

        var languageSettings = viewModel.Sections.OfType<LanguageSettingsViewModel>().First();
        var appearanceSettings = viewModel.Sections.OfType<AppearanceSettingsViewModel>().First();
        var downloaderSettings = viewModel.Sections.OfType<DownloaderSettingsViewModel>().First();
        var gameDetailsSettings = viewModel.Sections.OfType<GameDetailsSettingsViewModel>().First();
        var randomGameSettings = viewModel.Sections.OfType<RandomGameSettingsViewModel>().First();
        var backupSettings = viewModel.Sections.OfType<SavegameSettingsViewModel>().First();

        if (languageSettings.ApplicationLanguage?.CultureInfo != null)
        {
            _appConfiguration.ApplicationLanguage = languageSettings.ApplicationLanguage.CultureInfo.IetfLanguageTag;
            LocalizationManager.ChangeLanguage(languageSettings.ApplicationLanguage.CultureInfo);
        }
        if (appearanceSettings.SelectedTheme != null)
        {
            _appConfiguration.ApplicationTheme = appearanceSettings.SelectedTheme.Value;
            _themeManager.ApplyTheme(appearanceSettings.SelectedTheme.Value);
            AppUtils.ChangeTheme(appearanceSettings.SelectedTheme.BaseVariant);
        }
        _appConfiguration.DefaultToGridView = appearanceSettings.DefaultToGridView;
        var mappedDownloaderConfigs =
            _mapper.Map<List<DownloaderConfiguration>>(downloaderSettings.AvailableDownloaders);
        _appConfiguration.Downloaders = mappedDownloaderConfigs;
        _appConfiguration.AskForConfirmationBeforeWalkthrough =
            gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        _appConfiguration.WinePath = gameDetailsSettings.WinePath;
        _appConfiguration.DocumentationPatterns = gameDetailsSettings.DocumentationPatterns?.TargetCollection.ToList() ?? new List<CheckableItem<string>>();
        _appConfiguration.DocumentationFolderExclusions = gameDetailsSettings.FolderExclusions?.TargetCollection.ToList() ?? new List<CheckableItem<string>>();
        _appConfiguration.RandomGameMaxRerolls = randomGameSettings.MaxRerolls;
        _appConfiguration.BackupSavegamesEnabled = backupSettings.SavegameBackupEnabled;
        _appConfiguration.NumberOfVersionsToKeep =
            backupSettings.LimitNumberOfVersions ? backupSettings.NumberOfVersionsToKeep : null;
        await File.WriteAllTextAsync("appsettings.user.json",
            JsonConvert.SerializeObject(_appConfiguration.User, Formatting.Indented,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        viewModel.ClearBusy();
    }

    public List<DownloaderViewModel> GetDownloaderViewModels()
    {
        return _mapper.Map<List<DownloaderViewModel>>(_settingsProvider.GetDownloaderConfigurations());
    }

    public GameDetailsSettingsViewModel GetGameDetailsSettings(PageViewModel settingsPage)
    {
        var settings = _settingsProvider.GetGameDetailsSettings();
        return new GameDetailsSettingsViewModel(settingsPage)
        {
            AskForConfirmationBeforeWalkthrough =
                _appConfiguration.AskForConfirmationBeforeWalkthrough.GetValueOrDefault(),
            DocumentationPatterns = new EditablePatternListBoxViewModel() { TargetCollection = settings.EnabledPatterns.ToObservableCollection() },
            FolderExclusions = new EditableFolderExclusionsListBoxViewModel() { TargetCollection = settings.ExcludedFolders.ToObservableCollection() },
            WinePath = settings.WinePath
        };
    }

    public SavegameSettingsViewModel GetSavegameSettings(PageViewModel settingsPage)
    {
        return new SavegameSettingsViewModel(settingsPage, this)
        {
            SavegameBackupEnabled = _appConfiguration.BackupSavegamesEnabled,
            LimitNumberOfVersions = _appConfiguration.NumberOfVersionsToKeep != null,
            NumberOfVersionsToKeep = _appConfiguration.NumberOfVersionsToKeep,
            SavegameProcessingDelay = _appConfiguration.SavegameProcessingDelay
        };
    }



    public async Task SyncSavegames(PageViewModel settingsPage)
    {
        // Lazy resolution via IServiceProvider to break circular dependency:
        // SettingsPageService -> SavegameCommandService -> SettingsProvider -> AppConfiguration
        var savegameService = _serviceProvider.GetRequiredService<SavegameCommandService>();
        await savegameService.SyncSavegames(settingsPage);
    }

    public async Task CleanUpTempFiles()
    {
        await _appFileOperations.CleanUpTempFiles();
    }
}