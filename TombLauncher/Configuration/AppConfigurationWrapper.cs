using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Configuration;

public class AppConfigurationWrapper : IAppConfigurationWrapper
{
    public IAppConfiguration Defaults { get; set; } = new AppConfiguration();
    public IAppConfiguration User { get; set; } = new AppConfiguration();

    public string ApplicationLanguage
    {
        get => User.ApplicationLanguage.Coalesce(Defaults.ApplicationLanguage);
        set => User.ApplicationLanguage = value.NullIfEquals(Defaults.ApplicationLanguage);
    }

    public string DatabasePath
    {
        get => User.DatabasePath.Coalesce(Defaults.DatabasePath);
        set => User.DatabasePath = value.NullIfEquals(Defaults.DatabasePath);
    }

    public string ApplicationTheme
    {
        get => User.ApplicationTheme.Coalesce(Defaults.ApplicationLanguage);
        set => User.ApplicationTheme = value.NullIfEquals(Defaults.ApplicationLanguage);
    }
    public bool? UseInternalViewer
    {
        get => User.UseInternalViewer.Coalesce(Defaults.UseInternalViewer);
        set => User.UseInternalViewer = value.NullIfEquals(Defaults.UseInternalViewer);
    }
    public bool? AskForConfirmationBeforeWalkthrough
    {
        get => User.AskForConfirmationBeforeWalkthrough.Coalesce(Defaults.AskForConfirmationBeforeWalkthrough);
        set => User.AskForConfirmationBeforeWalkthrough = value.NullIfEquals(Defaults.AskForConfirmationBeforeWalkthrough);
    }
    public List<DownloaderConfigDto> Downloaders
    {
        get => User.Downloaders.Coalesce(Defaults.Downloaders);
        set
        {
            if (value.SequenceEqual(Defaults.Downloaders))
                User.Downloaders = null;
                return;
            User.Downloaders = value;
        }
    }
    public LogLevel? MinimumLogLevel
    {
        get => User.MinimumLogLevel.Coalesce(Defaults.MinimumLogLevel);
        set => User.MinimumLogLevel = value.NullIfEquals(Defaults.MinimumLogLevel);
    }
}