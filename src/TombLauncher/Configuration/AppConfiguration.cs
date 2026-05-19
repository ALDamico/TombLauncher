using TombLauncher.Ai.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Gamepad.Configuration;

namespace TombLauncher.Configuration;

public class AppConfiguration : IAppConfiguration
{
    public ApplicationConfig Application { get; set; } = new();
    public AppearanceConfig Appearance { get; set; } = new();
    public CompatibilityConfig Compatibility { get; set; } = new();
    public DownloadersConfig Downloaders { get; set; } = new();
    public GameDetailsConfig GameDetails { get; set; } = new();
    public SavegamesConfig Savegames { get; set; } = new();
    public WelcomePageConfig WelcomePage { get; set; } = new();
    public UpdaterConfig Updater { get; set; } = new();
    public AiConfig Ai { get; set; } = new();
    public IntegrationsConfig Integrations { get; set; } = new();
    public GamepadConfig Gamepad { get; set; } = new();

    // Explicit interface implementation — returns the same objects as read-only interfaces
    IApplicationConfig IAppConfiguration.Application => Application;
    IAppearanceConfig IAppConfiguration.Appearance => Appearance;
    ICompatibilityConfig IAppConfiguration.Compatibility => Compatibility;
    IDownloadersConfig IAppConfiguration.Downloaders => Downloaders;
    IGameDetailsConfig IAppConfiguration.GameDetails => GameDetails;
    ISavegamesConfig IAppConfiguration.Savegames => Savegames;
    IWelcomePageConfig IAppConfiguration.WelcomePage => WelcomePage;
    IUpdaterConfig IAppConfiguration.Updater => Updater;
    IAiConfig IAppConfiguration.Ai => Ai;
    IIntegrationsConfig IAppConfiguration.Integrations => Integrations;
    IGamepadConfig IAppConfiguration.Gamepad => Gamepad;
}