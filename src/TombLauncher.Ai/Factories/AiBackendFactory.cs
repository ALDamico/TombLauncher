using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Factories;

public class AiBackendFactory
{
    private readonly IAiBackendService _ollamaBackendService;
    private readonly IAiBackendService _lmStudioBackendService;

    public AiBackendFactory([FromKeyedServices(AiBackendType.Ollama)] IAiBackendService ollamaBackendService, [FromKeyedServices(AiBackendType.LmStudio)] IAiBackendService lmStudioBackendService)
    {
        _ollamaBackendService = ollamaBackendService;
        _lmStudioBackendService = lmStudioBackendService;
    }

    public IAiBackendService Create(AiBackendType backendType)
    {
        return backendType switch
        {
            AiBackendType.LmStudio => _lmStudioBackendService,
            AiBackendType.Ollama => _ollamaBackendService,
            _ => throw new ArgumentOutOfRangeException(nameof(backendType), backendType, null)
        };
    }
}