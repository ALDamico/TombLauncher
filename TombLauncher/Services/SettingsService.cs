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
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Services;

public class SettingsService : IViewService
{
    public SettingsService()
    {
        LocalizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        NavigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        MessageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        DialogService = Ioc.Default.GetRequiredService<IDialogService>();
        var mapperConfiguration = Ioc.Default.GetRequiredService<MapperConfiguration>();
        _mapper = mapperConfiguration.CreateMapper();
        _appConfiguration = Ioc.Default.GetRequiredService<IAppConfigurationWrapper>();
        _logger = Ioc.Default.GetRequiredService<ILogger<SettingsService>>();
    }

    private readonly IAppConfigurationWrapper _appConfiguration;
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private readonly IMapper _mapper;
    private readonly ILogger<SettingsService> _logger;

    public List<ApplicationLanguageViewModel> GetSupportedLanguages()
    {
        var supportedLanguages = LocalizationManager.GetSupportedLanguages();
        return _mapper.Map<List<ApplicationLanguageViewModel>>(supportedLanguages);
    }

    public CultureInfo GetApplicationLanguage()
    {
        return new CultureInfo(_appConfiguration.ApplicationLanguage);
    }

    public ThemeVariant GetApplicationTheme()
    {
        var targetSetting = _appConfiguration.ApplicationTheme;
        var themeToApply = ReflectionUtils.GetStaticInstanceByName<ThemeVariant>(targetSetting);
        return themeToApply;
    }

    public async Task Save(SettingsPageViewModel viewModel)
    {
        viewModel.SetBusy(true, "Saving application settings...".GetLocalizedString());

        var languageSettings = viewModel.Sections.OfType<LanguageSettingsViewModel>().First();
        var appearanceSettings = viewModel.Sections.OfType<AppearanceSettingsViewModel>().First();
        var downloaderSettings = viewModel.Sections.OfType<DownloaderSettingsViewModel>().First();
        var gameDetailsSettings = viewModel.Sections.OfType<GameDetailsSettingsViewModel>().First();
        var randomGameSettings = viewModel.Sections.OfType<RandomGameSettingsViewModel>().First();
        var backupSettings = viewModel.Sections.OfType<SavegameSettingsViewModel>().First();

        _appConfiguration.ApplicationLanguage =
            languageSettings.ApplicationLanguage.CultureInfo.IetfLanguageTag;
        LocalizationManager.ChangeLanguage(languageSettings.ApplicationLanguage.CultureInfo);
        _appConfiguration.ApplicationTheme = appearanceSettings.SelectedTheme.Key.ToString();
        AppUtils.ChangeTheme(appearanceSettings.SelectedTheme);
        _appConfiguration.DefaultToGridView = appearanceSettings.DefaultToGridView;
        var mappedDownloaderConfigs =
            _mapper.Map<List<DownloaderConfiguration>>(downloaderSettings.AvailableDownloaders);
        _appConfiguration.Downloaders = mappedDownloaderConfigs;
        _appConfiguration.AskForConfirmationBeforeWalkthrough =
            gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        _appConfiguration.DocumentationPatterns = gameDetailsSettings.DocumentationPatterns.TargetCollection.ToList();
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
        return _mapper.Map<List<DownloaderViewModel>>(GetDownloaderConfigurations());
    }

    private List<DownloaderConfiguration> GetDownloaderConfigurations()
    {
        var dtos = new List<DownloaderConfiguration>();
        var downloaders = ReflectionUtils.GetImplementors<IGameDownloader>(BindingFlags.NonPublic).ToList();
        var priority = downloaders.Count();

        var configCustomizations = _appConfiguration.Downloaders.ToDictionary(d => d.ClassName);
        foreach (var downloader in downloaders)
        {
            var className = downloader.GetType().FullName;
            configCustomizations.TryGetValue(className, out var config);
            if (config == null)
            {
                config = new DownloaderConfiguration()
                {
                    ClassName = className,
                    IsChecked = true,
                    Priority = --priority
                };
            }

            var dto = new DownloaderConfiguration()
            {
                BaseUrl = downloader.BaseUrl,
                ClassName = className,
                DisplayName = downloader.DisplayName,
                IsChecked = config.IsChecked,
                Priority = config.Priority,
                SupportedFeatures = downloader.SupportedFeatures.GetDescription()
            };
            dtos.Add(dto);
        }

        return dtos.OrderBy(dto => dto.Priority).ToList();
    }

    public GameDetailsSettingsViewModel GetGameDetailsSettings(PageViewModel settingsPage)
    {
        return new GameDetailsSettingsViewModel(settingsPage)
        {
            AskForConfirmationBeforeWalkthrough =
                _appConfiguration.AskForConfirmationBeforeWalkthrough.GetValueOrDefault(),
            DocumentationPatterns = new EditablePatternListBoxViewModel(){TargetCollection = _appConfiguration.DocumentationPatterns.ToObservableCollection()},
            FolderExclusions = new EditableFolderExclusionsListBoxViewModel(){TargetCollection = _appConfiguration.DocumentationFolderExclusions.ToObservableCollection()}
        };
    }

    public SavegameSettingsViewModel GetSavegameSettings(PageViewModel settingsPage)
    {
        return new SavegameSettingsViewModel(settingsPage)
        {
            SavegameBackupEnabled = _appConfiguration.BackupSavegamesEnabled,
            LimitNumberOfVersions = _appConfiguration.NumberOfVersionsToKeep != null,
            NumberOfVersionsToKeep = _appConfiguration.NumberOfVersionsToKeep,
            SavegameProcessingDelay = _appConfiguration.SavegameProcessingDelay
        };
    }

    public List<IGameDownloader> GetActiveDownloaders()
    {
        var downloaderConfigs = GetDownloaderConfigurations().Where(dl => dl.IsChecked);
        var output = new List<IGameDownloader>();
        foreach (var config in downloaderConfigs)
        {
            var downloaderImpl = ReflectionUtils.GetTypeByName(config.ClassName);
            if (downloaderImpl == null)
            {
                continue;
            }

            var downloader = (IGameDownloader)Ioc.Default.GetRequiredService(downloaderImpl);
            output.Add(downloader);
        }

        return output;
    }

    public int GetRandomGameMaxRerolls() => _appConfiguration.RandomGameMaxRerolls.GetValueOrDefault();

    public string GetDatabasePath()
    {
        return _appConfiguration.DatabasePath;
    }

    public async Task SyncSavegames(PageViewModel settingsPage)
    {
        var savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        await savegameService.SyncSavegames(settingsPage);
    }

    public string GetGitHubLink()
    {
        return _appConfiguration.GitHubLink;
    }

    public bool IsGridViewDefault()
    {
        return _appConfiguration.DefaultToGridView;
    }

    public List<string> GetEnabledPatterns()
    {
        return _appConfiguration.DocumentationPatterns.GetCheckedItems().ToList();
    }

    public List<string> GetExcludedFolders()
    {
        return _appConfiguration.DocumentationFolderExclusions.GetCheckedItems().ToList();
    }

    public async Task CleanUpTempFiles()
    {
        _logger.LogInformation("Starting temp folder clean up");
        var tempFolder = PathUtils.GetTombLauncherTempDirectory();
        var entries = Directory.GetFileSystemEntries(tempFolder).Where(e => e != tempFolder).ToArray();
        foreach (var entry in entries)
        {
            try
            {
                _logger.LogInformation("Deleting entry {EntryName}", entry);
                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, true);
                }
                else if (File.Exists(entry))
                {
                    File.Delete(entry);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError("An IOException was ignored: {Ex}", ex);
            }
        }
        
        _logger.LogInformation("Temp folder clean up completed");

        await Task.CompletedTask;
    }

    public string GetWinePath() => _appConfiguration.WinePath;
}