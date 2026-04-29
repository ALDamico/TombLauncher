using TombLauncher.Configuration.Sections;

namespace TombLauncher.Configuration;

public interface IAppConfiguration
{
    IApplicationConfig Application { get; }
    IAppearanceConfig Appearance { get; }
    ICompatibilityConfig Compatibility { get; }
    IDownloadersConfig Downloaders { get; }
    IGameDetailsConfig GameDetails { get; }
    ISavegamesConfig Savegames { get; }
    IWelcomePageConfig WelcomePage { get; }
    IUpdaterConfig Updater { get; }
}