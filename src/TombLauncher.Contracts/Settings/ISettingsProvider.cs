using TombLauncher.Contracts.Downloaders;

namespace TombLauncher.Contracts.Settings;

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
