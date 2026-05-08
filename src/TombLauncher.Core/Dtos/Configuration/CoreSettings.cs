using System.Globalization;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos.Configuration;

public record SavegameCoreSettings(bool IsBackupEnabled, int? NumberOfVersionsToKeep, int ProcessingDelay);

public record AppearanceCoreSettings(string ApplicationTheme, bool IsGridViewDefault);

public record GameDetailsCoreSettings(string UnzipFallbackMethod, (string Command, string CommandLineArguments) UnzipFallbackMethodCommandLine, List<CheckableItem<string>> EnabledPatterns, List<CheckableItem<string>> ExcludedFolders, bool AskForConfirmationBeforeWalkthrough, int DescriptionFontSize = 18);

public record ApplicationCoreSettings(string GitHubLink, string WebsiteLink, CultureInfo ApplicationLanguage, int RandomGameMaxRerolls, string DatabasePath);

public record AiCoreSettings(bool IsEnabled, string ModelId, AiBackendType BackendType, string Endpoint, string? ApiKey, string EmbeddingModelId);
