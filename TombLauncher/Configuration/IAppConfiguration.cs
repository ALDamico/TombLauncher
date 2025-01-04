using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Configuration;

public interface IAppConfiguration
{
    string ApplicationLanguage { get; set; }
    string DatabasePath { get; set; }
    string ApplicationTheme { get; set; }
    bool? UseInternalViewer { get; set; }
    bool? AskForConfirmationBeforeWalkthrough { get; set; }
    List<DownloaderConfigDto> Downloaders { get; set; }
    LogLevel? MinimumLogLevel { get; set; }
}