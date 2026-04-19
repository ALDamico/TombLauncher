using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Dtos;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos.Configuration;

namespace TombLauncher.Services;

public class SettingsProvider : ISettingsProvider
{
    private readonly ILayeredAppConfiguration _appConfiguration;
    private readonly IServiceProvider _serviceProvider;

    public SettingsProvider(
        ILayeredAppConfiguration appConfiguration,
        IServiceProvider serviceProvider,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _appConfiguration = appConfiguration;
        _serviceProvider = serviceProvider;
        PlatformSpecificFeatures = platformSpecificFeatures;
    }

    public ApplicationCoreSettings GetApplicationSettings()
    {
        var app = _appConfiguration.Application;
        var wp = _appConfiguration.WelcomePage;
        return new ApplicationCoreSettings(
            app.GitHubLink ?? string.Empty,
            app.WebsiteLink ?? string.Empty,
            new CultureInfo(app.ApplicationLanguage ?? "en-US"),
            wp.RandomGameMaxRerolls.GetValueOrDefault(),
            app.DatabasePath ?? "TombLauncher.sqlite"
        );
    }

    public AppearanceCoreSettings GetAppearanceSettings()
    {
        var appearance = _appConfiguration.Appearance;
        return new AppearanceCoreSettings(
            appearance.ApplicationTheme ?? string.Empty,
            appearance.DefaultToGridView
        );
    }

    public List<DownloaderConfiguration> GetDownloaderConfigurations()
    {
        var dtos = new List<DownloaderConfiguration>();
        var downloaders = _serviceProvider.GetServices<IGameDownloader>().ToList();
        var priority = downloaders.Count;

        var configCustomizations = _appConfiguration.Downloaders.Sources?.Where(d => d.ClassName != null).ToDictionary(d => d.ClassName!) ?? new Dictionary<string, DownloaderConfiguration>();
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
        var activeClassNames = GetDownloaderConfigurations()
            .Where(dl => dl.IsChecked)
            .Select(dl => dl.ClassName)
            .ToHashSet();

        return _serviceProvider.GetServices<IGameDownloader>()
            .Where(d => activeClassNames.Contains(d.GetType().FullName))
            .ToList();
    }

    public GameDetailsCoreSettings GetGameDetailsSettings()
    {
        var gd = _appConfiguration.GameDetails;
        var dl = _appConfiguration.Downloaders;
        var methodToUse = PlatformSpecificFeatures.GetPlatformSpecificZipFallbackPrograms()
            .FirstOrDefault(m => m.Name == dl.UnzipFallbackMethod);
        return new GameDetailsCoreSettings(
            dl.UnzipFallbackMethod ?? string.Empty,
            methodToUse != null ? (methodToUse.Command, methodToUse.CommandLineArguments) : (string.Empty, string.Empty),
            gd.DocumentationPatterns?.ToList() ?? new List<CheckableItem<string>>(),
            gd.DocumentationFolderExclusions?.ToList() ?? new List<CheckableItem<string>>(),
            gd.AskForConfirmationBeforeWalkthrough.GetValueOrDefault(),
            gd.DescriptionFontSize ?? 18
        );
    }

    public SavegameCoreSettings GetSavegameSettings()
    {
        var sg = _appConfiguration.Savegames;
        return new SavegameCoreSettings(
            sg.BackupSavegamesEnabled.GetValueOrDefault(),
            sg.NumberOfVersionsToKeep,
            sg.SavegameProcessingDelay
        );
    }

    public AiCoreSettings GetAiCoreSettings()
    {
        var aiSettings = _appConfiguration.Ai;
        return new AiCoreSettings(aiSettings.IsAiEnabled, aiSettings.ModelName!,
            aiSettings.GpuOffloadPercentage, aiSettings.ModelSizes ?? new Dictionary<string, long>());
    }

    public IPlatformSpecificFeatures PlatformSpecificFeatures { get; }
}
