using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Utils;
using TombLauncher.Core.Dtos;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
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
    }

    private readonly IAppConfigurationWrapper _appConfiguration;
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private readonly IMapper _mapper;

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

        _appConfiguration.ApplicationLanguage =
            viewModel.LanguageSettings.ApplicationLanguage.CultureInfo.IetfLanguageTag;
        LocalizationManager.ChangeLanguage(viewModel.LanguageSettings.ApplicationLanguage.CultureInfo);
        _appConfiguration.ApplicationTheme = viewModel.AppearanceSettings.SelectedTheme.Key.ToString();
        Application.Current.RequestedThemeVariant = viewModel.AppearanceSettings.SelectedTheme;
        var mappedDownloaderConfigs =
            _mapper.Map<List<DownloaderConfiguration>>(viewModel.DownloaderSettings.AvailableDownloaders);
        _appConfiguration.Downloaders = mappedDownloaderConfigs;
        _appConfiguration.AskForConfirmationBeforeWalkthrough =
            viewModel.GameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        _appConfiguration.UseInternalViewer = viewModel.GameDetailsSettings.UseInternalViewerIfAvailable;
        await File.WriteAllTextAsync("appsettings.user.json", JsonConvert.SerializeObject(_appConfiguration.User, Formatting.Indented, new JsonSerializerSettings(){NullValueHandling = NullValueHandling.Ignore}));
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
                    IsEnabled = true,
                    Priority = --priority
                };
            }
            var dto = new DownloaderConfiguration()
            {
                BaseUrl = downloader.BaseUrl,
                ClassName = className,
                DisplayName = downloader.DisplayName,
                IsEnabled = config.IsEnabled,
                Priority = config.Priority,
                SupportedFeatures = downloader.SupportedFeatures.GetDescription()
            };
            dtos.Add(dto);
        }

        return dtos;
    }

    public GameDetailsSettingsViewModel GetGameDetailsSettings()
    {
        return new GameDetailsSettingsViewModel()
        {
            AskForConfirmationBeforeWalkthrough = _appConfiguration.AskForConfirmationBeforeWalkthrough.GetValueOrDefault(),
            UseInternalViewerIfAvailable = _appConfiguration.UseInternalViewer.GetValueOrDefault()
        };
    }

    public List<IGameDownloader> GetActiveDownloaders()
    {
        var downloaderConfigs = GetDownloaderConfigurations().Where(dl => dl.IsEnabled);
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
}