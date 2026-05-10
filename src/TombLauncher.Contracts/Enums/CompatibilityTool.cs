namespace TombLauncher.Contracts.Enums;

public enum CompatibilityTool
{
    Unspecified = 0,  // Use global settings (C# default)
    Wine = 1,
    Proton = 2,
    WindowsNative = 3,
    LinuxNative = 4
}
