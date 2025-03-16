using System.Diagnostics;

namespace TombLauncher.Core.PlatformSpecific;

public class WindowsPlatformSpecificFeatures : IPlatformSpecificFeatures
{
    public void OpenGameFolder(string gameFolder)
    {
        Process.Start("explorer", gameFolder);
    }
}