using System.Collections.Generic;
using System.Globalization;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Services;

public interface ISettingsProvider
{
    AppearanceCoreSettings GetAppearanceSettings();
    ApplicationCoreSettings GetApplicationSettings();
    GameDetailsCoreSettings GetGameDetailsSettings();
    List<DownloaderConfiguration> GetDownloaderConfigurations();
    List<IGameDownloader> GetActiveDownloaders();
    SavegameCoreSettings GetSavegameSettings();
}
