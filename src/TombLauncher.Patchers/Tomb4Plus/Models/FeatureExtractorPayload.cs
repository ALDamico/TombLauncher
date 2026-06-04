using TombLauncher.Patchers.Tomb4Plus.Enums;

namespace TombLauncher.Patchers.Tomb4Plus.Models;

public record FeatureExtractorPayload(string InstallDirectory, string? ExeFile, BasicGameMetadata GameMetadata, SyntaxFile SyntaxFile);