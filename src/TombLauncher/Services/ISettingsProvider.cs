using System.Collections.Generic;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Services;

public interface ISettingsProvider
{
    AppearanceCoreSettings GetAppearanceSettings();
    ApplicationCoreSettings GetApplicationSettings();
    GameDetailsCoreSettings GetGameDetailsSettings();
    List<DownloaderConfiguration> GetDownloaderConfigurations();
    List<IGameDownloader> GetActiveDownloaders();
    SavegameCoreSettings GetSavegameSettings();
    AiCoreSettings GetAiCoreSettings();
    IPlatformSpecificFeatures PlatformSpecificFeatures { get; }
}
