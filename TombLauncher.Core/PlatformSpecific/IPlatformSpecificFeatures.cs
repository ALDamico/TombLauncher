namespace TombLauncher.Core.PlatformSpecific;

public interface IPlatformSpecificFeatures
{
    void OpenGameFolder(string gameFolder);
    void OpenUrl(string link);
    EnumerationOptions GetEnumerationOptions();
}