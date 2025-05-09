using System.Diagnostics;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.PlatformSpecific;

public interface IPlatformSpecificFeatures
{
    void OpenGameFolder(string gameFolder);
    void OpenUrl(string link);
    EnumerationOptions GetEnumerationOptions();

    ProcessStartInfo GetGameLaunchStartInfo(string executableFileNameOnly, string arguments,
        string compatibilityExecutable, string workingDirectory);

    NotifyFilters GetSavegameWatcherNotifyFilters();
    List<UnzipBackendDto> GetPlatformSpecificZipFallbackPrograms();
}