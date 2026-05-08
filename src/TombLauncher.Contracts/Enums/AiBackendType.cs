using System.ComponentModel;

namespace TombLauncher.Contracts.Enums;

public enum AiBackendType
{
    [Description("Ollama")]
    Ollama,
    [Description("LM Studio")]
    LmStudio
}