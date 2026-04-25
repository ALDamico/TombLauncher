using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TombLauncher.Configuration;
using TombLauncher.Extensions;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
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
        ILayeredAppConfiguration appConfiguration,
        ILogger<SettingsPageService> logger,
        ThemeManager themeManager,
        IServiceProvider serviceProvider,
        IAppFileOperationsService appFileOperations,
        ISettingsProvider settingsProvider,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        ViewContext = viewContext;
        _appConfiguration = appConfiguration;
        _logger = logger;
        _themeManager = themeManager;
        _serviceProvider = serviceProvider;
        _appFileOperations = appFileOperations;
        _settingsProvider = settingsProvider;
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly ILayeredAppConfiguration _appConfiguration;
    private readonly IServiceProvider _serviceProvider;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private IMapper _mapper => ViewContext.Mapper;
    private readonly ILogger<SettingsPageService> _logger;
    private readonly ThemeManager _themeManager;
    private readonly IAppFileOperationsService _appFileOperations;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public List<ApplicationLanguageViewModel> GetSupportedLanguages()
    {
        var supportedLanguages = LocalizationManager.GetSupportedLanguages();
        return _mapper.Map<List<ApplicationLanguageViewModel>>(supportedLanguages);
    }

    public CultureInfo GetApplicationLanguage() => _settingsProvider.GetApplicationSettings().ApplicationLanguage;

    public string GetApplicationTheme() => _settingsProvider.GetAppearanceSettings().ApplicationTheme;

    public async Task Save(SettingsPageViewModel viewModel)
    {
        using (viewModel.BusyScope("SAVING_APPLICATION_SETTINGS".GetLocalizedString()))
        {
            foreach (var section in viewModel.Sections)
                section.ApplyTo(_appConfiguration.User);

            await ApplySideEffects(viewModel);

            var userConfigPath = Path.Combine(_platformSpecificFeatures.GetAppDataDirectory(), "appsettings.user.json");
            await File.WriteAllTextAsync(userConfigPath,
                JsonConvert.SerializeObject(_appConfiguration.User, Formatting.Indented,
                    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore }));
        }
    }

    private async Task ApplySideEffects(SettingsPageViewModel viewModel)
    {
        var languageSettings = viewModel.Sections.OfType<LanguageSettingsViewModel>().First();
        var previousCulture = LocalizationManager.CurrentCulture;
        if (languageSettings.ApplicationLanguage?.CultureInfo != null)
            LocalizationManager.ChangeLanguage(languageSettings.ApplicationLanguage.CultureInfo);

        if (languageSettings.ApplicationLanguage?.CultureInfo != null &&
            !languageSettings.ApplicationLanguage.CultureInfo.Equals(previousCulture))
        {
            await ViewContext.PopupService.ShowLocalized(
                "RESTART_REQUIRED_CAPTION",
                "RESTART_REQUIRED_MESSAGE",
                MsgBoxButton.Ok,
                MsgBoxImage.Information);
        }

        var appearanceSettings = viewModel.Sections.OfType<AppearanceSettingsViewModel>().First();
        if (appearanceSettings.SelectedTheme != null)
        {
            _themeManager.ApplyTheme(appearanceSettings.SelectedTheme.Value);
            AppUtils.ChangeTheme(appearanceSettings.SelectedTheme.BaseVariant);
        }
    }

    public List<DownloaderViewModel> GetDownloaderViewModels()
    {
        return _mapper.Map<List<DownloaderViewModel>>(_settingsProvider.GetDownloaderConfigurations());
    }

    public GameDetailsSettingsViewModel GetGameDetailsSettings(PageViewModel settingsPage)
    {
        var settings = _settingsProvider.GetGameDetailsSettings();
        var gd = _appConfiguration.GameDetails;
        return new GameDetailsSettingsViewModel(settingsPage)
        {
            AskForConfirmationBeforeWalkthrough = gd.AskForConfirmationBeforeWalkthrough.GetValueOrDefault(),
            DescriptionFontSize = gd.DescriptionFontSize ?? 18,
            DocumentationPatterns = new EditablePatternListBoxViewModel() { TargetCollection = settings.EnabledPatterns.ToObservableCollection(), HeaderIcon = PackIconRemixIconKind.FileTextLine },
            FolderExclusions = new EditableFolderExclusionsListBoxViewModel() { TargetCollection = settings.ExcludedFolders.ToObservableCollection(), HeaderIcon = PackIconRemixIconKind.FolderLine },
            WinePath = settings.WinePath
        };
    }

    public SavegameSettingsViewModel GetSavegameSettings(PageViewModel settingsPage)
    {
        var sg = _appConfiguration.Savegames;
        return new SavegameSettingsViewModel(settingsPage, this)
        {
            SavegameBackupEnabled = sg.BackupSavegamesEnabled.GetValueOrDefault(),
            LimitNumberOfVersions = sg.NumberOfVersionsToKeep != null,
            NumberOfVersionsToKeep = sg.NumberOfVersionsToKeep,
            SavegameProcessingDelay = sg.SavegameProcessingDelay
        };
    }

    public async Task SyncSavegames(PageViewModel settingsPage)
    {
        var savegameService = _serviceProvider.GetRequiredService<SavegameCommandService>();
        await savegameService.SyncSavegames(settingsPage);
    }

    public async Task CleanUpTempFiles()
    {
        await _appFileOperations.CleanUpTempFiles();
    }
}