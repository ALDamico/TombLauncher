using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Settings;
using TombLauncher.Contracts.SupportMatrix;

namespace TombLauncher.Contracts.PlatformSpecific;

public interface IPlatformSpecificFeatures
{
    Platform Platform { get; }
    void OpenFolder(string folder);
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
    ISupportMatrix SupportMatrix { get; }

    bool IsExecutable(string path);
}