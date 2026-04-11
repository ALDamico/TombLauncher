using TombLauncher.Configuration.Sections;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Configuration;

public class LayeredAppConfiguration : ILayeredAppConfiguration
{
    public AppConfiguration Defaults { get; } = new();
    public AppConfiguration User { get; } = new();

    // Merged read-only views

    public IApplicationConfig Application => new ApplicationConfig
    {
        ApplicationLanguage = User.Application.ApplicationLanguage.Coalesce(Defaults.Application.ApplicationLanguage),
        DatabasePath = User.Application.DatabasePath.Coalesce(Defaults.Application.DatabasePath),
        MinimumLogLevel = User.Application.MinimumLogLevel.Coalesce(Defaults.Application.MinimumLogLevel),
        GitHubLink = Defaults.Application.GitHubLink
    };

    public IAppearanceConfig Appearance => new AppearanceConfig
    {
        ApplicationTheme = User.Appearance.ApplicationTheme.Coalesce(Defaults.Appearance.ApplicationTheme),
        DefaultToGridView = User.Appearance.DefaultToGridView.Coalesce(Defaults.Appearance.DefaultToGridView)
    };

    public ICompatibilityConfig Compatibility => new CompatibilityConfig
    {
        CompatibilityTool = User.Compatibility.CompatibilityTool != CompatibilityTool.Unspecified
            ? User.Compatibility.CompatibilityTool
            : Defaults.Compatibility.CompatibilityTool,
        WinePath = User.Compatibility.WinePath.Coalesce(Defaults.Compatibility.WinePath),
        CompatibilityPrefixPath = User.Compatibility.CompatibilityPrefixPath.Coalesce(Defaults.Compatibility.CompatibilityPrefixPath),
        ProtonPath = User.Compatibility.ProtonPath.Coalesce(Defaults.Compatibility.ProtonPath),
    };

    public IDownloadersConfig Downloaders => new DownloadersConfig
    {
        Sources = User.Downloaders.Sources.Coalesce(Defaults.Downloaders.Sources),
        UnzipFallbackMethod = User.Downloaders.UnzipFallbackMethod?.Coalesce(Defaults.Downloaders.UnzipFallbackMethod) ?? string.Empty
    };

    public IGameDetailsConfig GameDetails => new GameDetailsConfig
    {
        AskForConfirmationBeforeWalkthrough = User.GameDetails.AskForConfirmationBeforeWalkthrough.Coalesce(Defaults.GameDetails.AskForConfirmationBeforeWalkthrough),
        DocumentationPatterns = (Defaults.GameDetails.DocumentationPatterns ?? new()).MergeWithOverrides(User.GameDetails.DocumentationPatterns ?? new()),
        DocumentationFolderExclusions = (Defaults.GameDetails.DocumentationFolderExclusions ?? new()).MergeWithOverrides(User.GameDetails.DocumentationFolderExclusions ?? new()),
        DescriptionFontSize = Defaults.GameDetails.DescriptionFontSize.Coalesce(User.GameDetails.DescriptionFontSize)
    };

    public ISavegamesConfig Savegames => new SavegamesConfig
    {
        BackupSavegamesEnabled = User.Savegames.BackupSavegamesEnabled.Coalesce(Defaults.Savegames.BackupSavegamesEnabled),
        NumberOfVersionsToKeep = User.Savegames.NumberOfVersionsToKeep.Coalesce(Defaults.Savegames.NumberOfVersionsToKeep),
        SavegameProcessingDelay = User.Savegames.SavegameProcessingDelay.Coalesce(Defaults.Savegames.SavegameProcessingDelay)
    };

    public IWelcomePageConfig WelcomePage => new WelcomePageConfig
    {
        ShowQuickStats = User.WelcomePage.ShowQuickStats.Coalesce(Defaults.WelcomePage.ShowQuickStats),
        ShowQuickActions = User.WelcomePage.ShowQuickActions.Coalesce(Defaults.WelcomePage.ShowQuickActions),
        ShowRecentlyPlayed = User.WelcomePage.ShowRecentlyPlayed.Coalesce(Defaults.WelcomePage.ShowRecentlyPlayed),
        ShowFavourites = User.WelcomePage.ShowFavourites.Coalesce(Defaults.WelcomePage.ShowFavourites),
        RecentlyPlayedCount = User.WelcomePage.RecentlyPlayedCount.Coalesce(Defaults.WelcomePage.RecentlyPlayedCount),
        FavouritesCount = User.WelcomePage.FavouritesCount.Coalesce(Defaults.WelcomePage.FavouritesCount),
        ShowRandomSuggestion = User.WelcomePage.ShowRandomSuggestion.Coalesce(Defaults.WelcomePage.ShowRandomSuggestion),
        RandomGameMaxRerolls = User.WelcomePage.RandomGameMaxRerolls.Coalesce(Defaults.WelcomePage.RandomGameMaxRerolls)
    };

    public IUpdaterConfig Updater => new UpdaterConfig
    {
        AppCastUrl = Defaults.Updater.AppCastUrl,
        AppCastPublicKey = Defaults.Updater.AppCastPublicKey,
        UpdaterUseLocalPaths = Defaults.Updater.UpdaterUseLocalPaths,
        UpdateChannelName = User.Updater.UpdateChannelName.Coalesce(Defaults.Updater.UpdateChannelName),
        GitHubRepositoryName = Defaults.Updater.GitHubRepositoryName,
        GitHubRepositoryOwner = Defaults.Updater.GitHubRepositoryOwner
    };
}