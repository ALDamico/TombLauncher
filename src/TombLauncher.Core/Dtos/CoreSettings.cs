using System.Globalization;

namespace TombLauncher.Core.Dtos;

public record SavegameCoreSettings(bool IsBackupEnabled, int? NumberOfVersionsToKeep, int ProcessingDelay);

public record AppearanceCoreSettings(string ApplicationTheme, bool IsGridViewDefault);

public record GameDetailsCoreSettings(string WinePath, string UnzipFallbackMethod, (string Command, string CommandLineArguments) UnzipFallbackMethodCommandLine, List<CheckableItem<string>> EnabledPatterns, List<CheckableItem<string>> ExcludedFolders, bool AskForConfirmationBeforeWalkthrough, int DescriptionFontSize = 18);

public record ApplicationCoreSettings(string GitHubLink, CultureInfo ApplicationLanguage, int RandomGameMaxRerolls, string DatabasePath);

public record AiCoreSettings(bool IsEnabled, string? ModelName, double? GpuOffloadPercentage, Dictionary<string, long> ModelSizes);
