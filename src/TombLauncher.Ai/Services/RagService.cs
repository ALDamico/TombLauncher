using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Plugins;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.Ai.Services;

public class RagService : ITroubleshootingService
{
    private readonly Kernel _kernel;
    private readonly VectorSearchService _vectorSearchService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly PromptExecutionSettings _promptExecutionSettings;
    private readonly ILogger<RagService> _logger;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly GamepadSupportMatrix _gamepadSupportMatrix;
    private bool _disposed;

    public RagService(Kernel kernel,
        VectorSearchService vectorSearchService,
        IChatCompletionService chatCompletionService,
        PromptExecutionSettings promptExecutionSettings,
        ILogger<RagService> logger, 
        IPlatformSpecificFeatures platformSpecificFeatures,
        GamepadSupportMatrix gamepadSupportMatrix)
    {
        _kernel = kernel;
        _vectorSearchService = vectorSearchService;
        _chatCompletionService = chatCompletionService;
        _promptExecutionSettings = promptExecutionSettings;
        _logger = logger;
        _platformSpecificFeatures = platformSpecificFeatures;
        _gamepadSupportMatrix = gamepadSupportMatrix;
    }

    public async IAsyncEnumerable<(MessageType, string)> AskAsync(string query, TroubleshootingContext troubleshootingContext,
        ChatHistory chatHistory,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var kernel = _kernel.Clone();
        kernel.Plugins.AddFromObject(new GameDiagnosticsPlugin(troubleshootingContext));
        kernel.Plugins.AddFromObject(new SupportMatrixPlugin(_platformSpecificFeatures.SupportMatrix, troubleshootingContext));
        kernel.Plugins.AddFromObject(new GamepadSupportPlugin(_gamepadSupportMatrix, troubleshootingContext));
        var knowledgeBaseItems = await _vectorSearchService.SearchAsync(query, cancellationToken: cancellationToken);

        var documentationSection = string.Join("\n---\n", knowledgeBaseItems.Select(WriteDocumentation));

        query = @$"{query}
----
Relevant documentation:
{documentationSection}";

        chatHistory.AddUserMessage(query);
        await foreach (var chunk in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory,
                           kernel: kernel, executionSettings: _promptExecutionSettings,
                           cancellationToken: cancellationToken))
        {
            _logger.LogDebug("Chunk received — Role: {Role}, Content: {Content}, Items: [{Items}]",
                chunk.Role,
                chunk.Content,
                string.Join(", ", chunk.Items.Select(i => i.GetType().Name)));

            var functionCalls = chunk.Items.OfType<StreamingFunctionCallUpdateContent>()
                .Where(f => !string.IsNullOrEmpty(f.Name))
                .ToList();

            if (functionCalls.Any())
            {
                _logger.LogInformation("FunctionCallContent detected: {Calls}",
                    string.Join(", ", functionCalls.Select(f => f.Name)));
                foreach (var call in functionCalls)
                    yield return (MessageType.ToolUse, call.Name! + Environment.NewLine);
            }
            else if (chunk.Content != null)
            {
                yield return (MessageType.Assistant, chunk.Content);
            }
        }
    }

    private string WriteDocumentation(KnowledgeBaseItemDto knowledgeBaseItemDto)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine(knowledgeBaseItemDto.SectionTitle);
        stringBuilder.AppendLine(knowledgeBaseItemDto.Text);
        stringBuilder.AppendLine().AppendLine(knowledgeBaseItemDto.Source);

        var additionalData = new
        {
            knowledgeBaseItemDto.AppVersionRange, knowledgeBaseItemDto.EngineVersions, knowledgeBaseItemDto.AppliesTo,
            knowledgeBaseItemDto.Platforms
        };
        stringBuilder.AppendLine("Additional information:  ").AppendLine(JsonConvert.SerializeObject(additionalData));

        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (_chatCompletionService is IDisposable disposable)
            disposable.Dispose();
    }
}