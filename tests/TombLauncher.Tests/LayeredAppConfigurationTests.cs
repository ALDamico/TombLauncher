using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;

namespace TombLauncher.Tests;

public class LayeredAppConfigurationTests
{
    private LayeredAppConfiguration CreateConfig(
        Action<AppConfiguration>? configureDefaults = null,
        Action<AppConfiguration>? configureUser = null)
    {
        var config = new LayeredAppConfiguration();
        configureDefaults?.Invoke(config.Defaults);
        configureUser?.Invoke(config.User);
        return config;
    }

    // --- Merge: User overrides Defaults ---

    [Fact]
    public void Application_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Application.ApplicationLanguage = "en-US",
            u => u.Application.ApplicationLanguage = "it-IT");

        Assert.Equal("it-IT", config.Application.ApplicationLanguage);
    }

    [Fact]
    public void Appearance_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Appearance.ApplicationTheme = "Dark",
            u => u.Appearance.ApplicationTheme = "Light");

        Assert.Equal("Light", config.Appearance.ApplicationTheme);
    }

    [Fact]
    public void Appearance_DefaultToGridView_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Appearance.DefaultToGridView = false,
            u => u.Appearance.DefaultToGridView = true);

        Assert.True(config.Appearance.DefaultToGridView);
    }

    [Fact]
    public void WelcomePage_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.WelcomePage.ShowQuickStats = true,
            u => u.WelcomePage.ShowQuickStats = false);

        Assert.False(config.WelcomePage.ShowQuickStats);
    }

    [Fact]
    public void Savegames_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Savegames.NumberOfVersionsToKeep = 5,
            u => u.Savegames.NumberOfVersionsToKeep = 10);

        Assert.Equal(10, config.Savegames.NumberOfVersionsToKeep);
    }

    // --- Fallback: User null → Defaults used ---

    [Fact]
    public void Application_FallsBackToDefaults_WhenUserNull()
    {
        var config = CreateConfig(
            d => d.Application.ApplicationLanguage = "en-US");

        Assert.Equal("en-US", config.Application.ApplicationLanguage);
    }

    [Fact]
    public void WelcomePage_FallsBackToDefaults_WhenUserNull()
    {
        var config = CreateConfig(
            d => d.WelcomePage.RandomGameMaxRerolls = 15);

        Assert.Equal(15, config.WelcomePage.RandomGameMaxRerolls);
    }

    [Fact]
    public void Savegames_FallsBackToDefaults_WhenUserNull()
    {
        var config = CreateConfig(
            d => d.Savegames.BackupSavegamesEnabled = true);

        Assert.True(config.Savegames.BackupSavegamesEnabled);
    }

    // --- Both null → null ---

    [Fact]
    public void Application_ReturnsNull_WhenBothNull()
    {
        var config = CreateConfig();
        Assert.Null(config.Application.ApplicationLanguage);
    }

    [Fact]
    public void WelcomePage_ReturnsNull_WhenBothNull()
    {
        var config = CreateConfig();
        Assert.Null(config.WelcomePage.ShowQuickStats);
    }

    // --- Updater: always from Defaults (except UpdateChannelName) ---

    [Fact]
    public void Updater_AppCastUrl_AlwaysFromDefaults()
    {
        var config = CreateConfig(
            d => d.Updater.AppCastUrl = "https://default.url");

        Assert.Equal("https://default.url", config.Updater.AppCastUrl);
    }

    [Fact]
    public void Updater_UpdateChannelName_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Updater.UpdateChannelName = "stable",
            u => u.Updater.UpdateChannelName = "beta");

        Assert.Equal("beta", config.Updater.UpdateChannelName);
    }

    // --- GitHubLink: always from Defaults ---

    [Fact]
    public void Application_GitHubLink_AlwaysFromDefaults()
    {
        var config = CreateConfig(
            d => d.Application.GitHubLink = "https://github.com/test",
            u => u.Application.GitHubLink = "https://user-override.com");

        Assert.Equal("https://github.com/test", config.Application.GitHubLink);
    }

    // --- Write isolation: writing to merged view doesn't persist ---

    [Fact]
    public void MergedView_IsImmutable_ByTypeSystem()
    {
        var config = CreateConfig(
            d => d.Application.ApplicationLanguage = "en-US");

        // IAppConfiguration returns IApplicationConfig (read-only) — no setter available
        IAppConfiguration readOnly = config;
        var app = readOnly.Application;

        // Verify it's the read-only interface (can't set)
        Assert.IsAssignableFrom<IApplicationConfig>(app);
        Assert.Equal("en-US", app.ApplicationLanguage);
    }

    // --- Write to User is visible in merged view ---

    [Fact]
    public void WriteToUser_ReflectedInMergedView()
    {
        var config = CreateConfig(
            d => d.Application.ApplicationLanguage = "en-US");

        Assert.Equal("en-US", config.Application.ApplicationLanguage);

        config.User.Application.ApplicationLanguage = "fr-FR";

        Assert.Equal("fr-FR", config.Application.ApplicationLanguage);
    }

    [Fact]
    public void WriteToUser_DoesNotAffectDefaults()
    {
        var config = CreateConfig(
            d => d.Application.ApplicationLanguage = "en-US");

        config.User.Application.ApplicationLanguage = "fr-FR";

        Assert.Equal("en-US", config.Defaults.Application.ApplicationLanguage);
    }

    // --- Downloaders merge ---

    [Fact]
    public void Downloaders_UnzipFallbackMethod_FallsBackToEmpty_WhenBothNull()
    {
        var config = CreateConfig();
        Assert.Equal(string.Empty, config.Downloaders.UnzipFallbackMethod);
    }

    [Fact]
    public void Downloaders_UnzipFallbackMethod_UserOverridesDefaults()
    {
        var config = CreateConfig(
            d => d.Downloaders.UnzipFallbackMethod = "7zip",
            u => u.Downloaders.UnzipFallbackMethod = "unzip");

        Assert.Equal("unzip", config.Downloaders.UnzipFallbackMethod);
    }

    // --- Each access returns a fresh merged snapshot ---

    [Fact]
    public void MergedView_ReturnsFreshInstance_EachAccess()
    {
        var config = CreateConfig(
            d => d.Appearance.ApplicationTheme = "Dark");

        var first = config.Appearance;
        var second = config.Appearance;

        Assert.NotSame(first, second);
        Assert.Equal(first.ApplicationTheme, second.ApplicationTheme);
    }
}
