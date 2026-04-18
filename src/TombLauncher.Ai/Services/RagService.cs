using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Plugins;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Services;

public class RagService : ITroubleshootingService
{
    private readonly Kernel _kernel;
    private readonly VectorSearchService _vectorSearchService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly GameDiagnosticsPlugin _gameDiagnosticsPlugin;
    private readonly PromptExecutionSettings _promptExecutionSettings;
    private bool _disposed;

    public RagService(Kernel kernel,
        VectorSearchService vectorSearchService,
        IChatCompletionService chatCompletionService,
        GameDiagnosticsPlugin gameDiagnosticsPlugin,
        PromptExecutionSettings promptExecutionSettings)
    {
        _kernel = kernel;
        _vectorSearchService = vectorSearchService;
        _chatCompletionService = chatCompletionService;
        _gameDiagnosticsPlugin = gameDiagnosticsPlugin;
        _promptExecutionSettings = promptExecutionSettings;
    }

    public async IAsyncEnumerable<string> AskAsync(string query, TroubleshootingContext? troubleshootingContext,
        ChatHistory chatHistory,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _gameDiagnosticsPlugin.TroubleshootingContext = troubleshootingContext;
        var knowledgeBaseItems = await _vectorSearchService.SearchAsync(query, cancellationToken: cancellationToken);

        var documentationSection = string.Join("\n---\n", knowledgeBaseItems.Select(WriteDocumentation));

        var systemPrompt = @$"
Relevant documentation:
{documentationSection}";

        chatHistory.AddSystemMessage(systemPrompt);
        chatHistory.AddUserMessage(query);
        await foreach (var chunk in _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory,
                           kernel: _kernel, executionSettings: _promptExecutionSettings,
                           cancellationToken: cancellationToken))
        {
            if (chunk.Content != null)
                yield return chunk.Content;
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
        if (_chatCompletionService is IDisposable disposable)
            disposable.Dispose();
    }
}