using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;

namespace TombLauncher.Tests;

public class CompatibilityConfigTests
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

    // ─── CompatibilityTool ────────────────────────────────────────────────

    [Fact]
    public void CompatibilityTool_DefaultsToWine_WhenBothUnset()
    {
        var config = CreateConfig();
        Assert.Equal(CompatibilityTool.Wine, config.Compatibility.CompatibilityTool);
    }

    [Fact]
    public void CompatibilityTool_UserProton_Overrides_DefaultWine()
    {
        var config = CreateConfig(
            d => d.Compatibility.CompatibilityTool = CompatibilityTool.Wine,
            u => u.Compatibility.CompatibilityTool = CompatibilityTool.Proton);

        Assert.Equal(CompatibilityTool.Proton, config.Compatibility.CompatibilityTool);
    }

    [Fact]
    public void CompatibilityTool_DefaultProton_UsedWhenUserUnset()
    {
        var config = CreateConfig(
            d => d.Compatibility.CompatibilityTool = CompatibilityTool.Proton);

        Assert.Equal(CompatibilityTool.Proton, config.Compatibility.CompatibilityTool);
    }

    // ─── ProtonPath ───────────────────────────────────────────────────────

    [Fact]
    public void ProtonPath_IsNull_WhenBothUnset()
    {
        var config = CreateConfig();
        Assert.Null(config.Compatibility.ProtonPath);
    }

    [Fact]
    public void ProtonPath_UserOverrides_Default()
    {
        var config = CreateConfig(
            d => d.Compatibility.ProtonPath = "/default/proton",
            u => u.Compatibility.ProtonPath = "/user/proton");

        Assert.Equal("/user/proton", config.Compatibility.ProtonPath);
    }

    [Fact]
    public void ProtonPath_FallsBackToDefault_WhenUserNull()
    {
        var config = CreateConfig(
            d => d.Compatibility.ProtonPath = "/default/proton");

        Assert.Equal("/default/proton", config.Compatibility.ProtonPath);
    }

    // ─── WinePath (regression) ───────────────────────────────────────────

    [Fact]
    public void WinePath_UserOverrides_Default()
    {
        var config = CreateConfig(
            d => d.Compatibility.WinePath = "/usr/bin/wine",
            u => u.Compatibility.WinePath = "/opt/wine/bin/wine");

        Assert.Equal("/opt/wine/bin/wine", config.Compatibility.WinePath);
    }

    [Fact]
    public void WriteToUser_CompatibilityTool_ReflectedInMergedView()
    {
        var config = CreateConfig(
            d => d.Compatibility.CompatibilityTool = CompatibilityTool.Wine);

        Assert.Equal(CompatibilityTool.Wine, config.Compatibility.CompatibilityTool);

        config.User.Compatibility.CompatibilityTool = CompatibilityTool.Proton;

        Assert.Equal(CompatibilityTool.Proton, config.Compatibility.CompatibilityTool);
    }
}
