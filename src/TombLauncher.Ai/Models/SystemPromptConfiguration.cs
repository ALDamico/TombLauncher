using System.Globalization;

namespace TombLauncher.Ai.Models;

public class SystemPromptConfiguration
{
    public required CultureInfo ApplicationLanguage { get; set; }
    public required string ModelName { get; set; }
    public DateTimeOffset? KnowledgeBaseGenerationDate { get; set; }
}