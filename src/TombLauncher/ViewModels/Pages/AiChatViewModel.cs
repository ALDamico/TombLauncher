using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Ai;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Utils;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels.Ai;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace TombLauncher.ViewModels.Pages;

public partial class AiChatViewModel : PageViewModel
{
    private readonly ITroubleshootingServiceLoader _troubleshootingServiceLoader;
    private readonly SystemPromptConfiguration _systemPromptConfiguration;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))] private bool _isGenerating;
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))] private string _currentText = "";
    [ObservableProperty] private ObservableCollection<AiMessageViewModel> _messageHistory = new();
    [ObservableProperty] private string _currentStatusText = "";
    private ITroubleshootingService? _ragService;
    private readonly ChatHistory _chatHistory = new();
    private TroubleshootingContext _troubleshootingContext;

    public bool IsHistoryEmpty => MessageHistory.Count == 0;

    public AiChatViewModel(ITroubleshootingServiceLoader troubleshootingServiceLoader, SystemPromptConfiguration systemPromptConfiguration)
    {
        _troubleshootingServiceLoader = troubleshootingServiceLoader;
        _systemPromptConfiguration = systemPromptConfiguration;
        _troubleshootingContext = new();
        MessageHistory.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsHistoryEmpty));
    }

    protected override async Task RaiseInitialize()
    {
        using (BusyScope("Caricamento modello AI..."))
        {
            _ragService = await _troubleshootingServiceLoader.Load(new Progress<float>(f => Console.WriteLine(f)), CancellationToken.None);
            _chatHistory.AddSystemMessage(AiConfigUtils.LoadSystemPrompt(_systemPromptConfiguration));
        }
        await base.RaiseInitialize();
    }

    public override Task OnNavigatedTo(object parameter)
    {
        if (parameter is TroubleshootingContext context)
        {
            _troubleshootingContext = context;
        }

        return Task.CompletedTask;
    }

    private readonly List<string> _statusTexts =
    [
        "Laura is shooting up some T-Rexes",
        "Laura is trespassing on federal property",
        "Laura is damaging some Venetian families' livelihood",
        "Laura is trying to get Lost City of Tinnos' last secret",
        "Laura is trying to get Red Alert not to bug out",
        "Laura is picking up a small medipack",
        "Laura is picking up a large medipack",
        "Laura has found a secret",
        "Laura is leaving a Frenchman to his own devices",
        "Laura is exterminating critically endangered species",
        "Laura is running away from beetles",
        "Laura is travelling on a skidoo",
        "Laura is travelling on a speedboat",
        "Laura is travelling on a dinghy",
        "Laura is travelling on a jeep",
        "Laura is chasing an offroader",
        "Laura is kayaking",
        "Laura is lining up a jump",
        "Laura is not having daddy issues"
    ];

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessage()
    {
        using var cts = new CancellationTokenSource();
        _ = PickRandomStatusText(cts.Token);
        var currentText = CurrentText;
        IsGenerating = true;
        var userMessage = GetMessage(MessageType.User, currentText);
        MessageHistory.Add(userMessage);
        CurrentText = "";
        var responseAdded = false;
        var enumerable = _ragService!.AskAsync(currentText,
            _troubleshootingContext, _chatHistory, cts.Token);
        var response = new AiMessageViewModel()
            { Text = "", MessageType = MessageType.Assistant, SentDate = DateTime.Now };
        var toolUse = new AiMessageViewModel()
            { Text = "", MessageType = MessageType.ToolUse, SentDate = DateTime.Now };
        var toolUsed = false;
        var fullResponse = new StringBuilder();
        await foreach (var messageChunk in enumerable)
        {
            if (messageChunk.Item1 == MessageType.ToolUse)
            {
                if (!toolUsed)
                {
                    MessageHistory.Insert(MessageHistory.Count - 1, toolUse);
                }
                toolUsed = true;
                toolUse.Text += messageChunk.Item2;
            }
            else
            {
                response.Text += messageChunk.Item2;
                fullResponse.Append(messageChunk);
                if (!responseAdded)
                {
                    MessageHistory.Add(response);
                    responseAdded = true;
                }
            }
        }
        _chatHistory.AddAssistantMessage(fullResponse.ToString());
        await cts.CancelAsync();
        IsGenerating = false;
    }

    private async Task PickRandomStatusText(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            CurrentStatusText = _statusTexts.PickOneAtRandom();
            try { await Task.Delay(5000, cancellationToken); }
            catch (OperationCanceledException) { break; }
        }
        
        CurrentStatusText = "";
    }

    private AiMessageViewModel GetMessage(MessageType messageType, string text)
    {
        return new AiMessageViewModel()
        {
            Text = text,
            MessageType = messageType,
            SentDate = DateTime.Now
        };
    }

    private bool CanSendMessage() => !IsGenerating && CurrentText.IsNotNullOrEmpty();
}