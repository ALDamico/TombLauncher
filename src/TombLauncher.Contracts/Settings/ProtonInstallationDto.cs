namespace TombLauncher.Contracts.Settings;

/// <summary>
/// Represents a Proton installation discovered on the system.
/// </summary>
/// <param name="DisplayName">Human-readable name, e.g. "Proton 9.0".</param>
/// <param name="ExecutablePath">Absolute path to the proton binary.</param>
public record ProtonInstallationDto(string DisplayName, string ExecutablePath);
