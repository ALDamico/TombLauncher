using System.Diagnostics;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Core.PlatformSpecific;

public interface IPlatformSpecificFeatures
{
    void OpenGameFolder(string gameFolder);
    void OpenUrl(string link);
    EnumerationOptions GetEnumerationOptions();

    NotifyFilters GetSavegameWatcherNotifyFilters();
    List<UnzipBackendDto> GetPlatformSpecificZipFallbackPrograms();
    string GetAppDataDirectory();

    string ExpandPath(string path);

    bool IsWineSupported { get; }
    string? FindWineExecutable();
    string? GetWineVersion(string winePath);
    List<ProtonInstallationDto> FindAvailableProtonInstallations();
    string? GetProtonVersion(string protonPath);
}