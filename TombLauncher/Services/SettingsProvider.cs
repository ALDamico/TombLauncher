using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Core.Dtos;
using TombLauncher.Contracts.Utils;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Services;

public class SettingsProvider : ISettingsProvider
{
    private readonly IAppConfigurationWrapper _appConfiguration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public SettingsProvider(
        IAppConfigurationWrapper appConfiguration,
        IServiceProvider serviceProvider,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _appConfiguration = appConfiguration;
        _serviceProvider = serviceProvider;
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    public ApplicationCoreSettings GetApplicationSettings()
    {
        return new ApplicationCoreSettings(
            _appConfiguration.GitHubLink ?? string.Empty,
            new CultureInfo(_appConfiguration.ApplicationLanguage ?? "en-US"),
            _appConfiguration.RandomGameMaxRerolls.GetValueOrDefault(),
            _appConfiguration.DatabasePath ?? "TombLauncher.sqlite"
        );
    }

    public AppearanceCoreSettings GetAppearanceSettings()
    {
        return new AppearanceCoreSettings(
            _appConfiguration.ApplicationTheme ?? string.Empty,
            _appConfiguration.DefaultToGridView
        );
    }

    public List<DownloaderConfiguration> GetDownloaderConfigurations()
    {
        var dtos = new List<DownloaderConfiguration>();
        var downloaders = ReflectionUtils.GetImplementors<IGameDownloader>(BindingFlags.NonPublic).ToList();
        var priority = downloaders.Count();

        var configCustomizations = _appConfiguration.Downloaders?.Where(d => d.ClassName != null).ToDictionary(d => d.ClassName!) ?? new Dictionary<string, DownloaderConfiguration>();
        foreach (var downloader in downloaders)
        {
            var className = downloader.GetType().FullName;
            configCustomizations.TryGetValue(className!, out var config);
            if (config == null)
            {
                config = new DownloaderConfiguration()
                {
                    ClassName = className,
                    IsChecked = true,
                    Priority = --priority,
                    BaseUrl = string.Empty,
                    DisplayName = string.Empty
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

    public List<IGameDownloader> GetActiveDownloaders()
    {
        var downloaderConfigs = GetDownloaderConfigurations().Where(dl => dl.IsChecked);
        var output = new List<IGameDownloader>();
        foreach (var config in downloaderConfigs)
        {
            if (config.ClassName == null) continue;
            var downloaderImpl = ReflectionUtils.GetTypeByName(config.ClassName);
            if (downloaderImpl == null)
            {
                continue;
            }

            var downloader = (IGameDownloader)_serviceProvider.GetRequiredService(downloaderImpl);
            output.Add(downloader);
        }

        return output;
    }

    public GameDetailsCoreSettings GetGameDetailsSettings()
    {
        var methodToUse = _platformSpecificFeatures.GetPlatformSpecificZipFallbackPrograms()
            .FirstOrDefault(m => m.Name == _appConfiguration.UnzipFallbackMethod);
        return new GameDetailsCoreSettings(
            _appConfiguration.WinePath ?? string.Empty,
            _appConfiguration.UnzipFallbackMethod ?? string.Empty,
            methodToUse != null ? (methodToUse.Command, methodToUse.CommandLineArguments) : (string.Empty, string.Empty),
            _appConfiguration.DocumentationPatterns?.ToList() ?? new List<CheckableItem<string>>(),
            _appConfiguration.DocumentationFolderExclusions?.ToList() ?? new List<CheckableItem<string>>(),
            _appConfiguration.AskForConfirmationBeforeWalkthrough.GetValueOrDefault()
        );
    }

    public SavegameCoreSettings GetSavegameSettings()
    {
        return new SavegameCoreSettings(
            _appConfiguration.BackupSavegamesEnabled.GetValueOrDefault(),
            _appConfiguration.NumberOfVersionsToKeep,
            _appConfiguration.SavegameProcessingDelay
        );
    }
}
