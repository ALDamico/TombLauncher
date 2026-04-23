using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Ai;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.ViewModels.Pages;

public partial class AiChatViewModel : PageViewModel
{
    private readonly ITroubleshootingServiceLoader _troubleshootingServiceLoader;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _currentText = "";
    [ObservableProperty] private ObservableCollection<AiMessageViewModel> _messageHistory = new();
    [ObservableProperty] private string _currentStatusText = "";
    private ITroubleshootingService? _ragService;

    public AiChatViewModel( ITroubleshootingServiceLoader troubleshootingServiceLoader)
    {
        _troubleshootingServiceLoader = troubleshootingServiceLoader;
    }

    protected override async Task RaiseInitialize()
    {
        using (BusyScope("Caricamento modello AI..."))
        {
            _ragService = await _troubleshootingServiceLoader.Load(new Progress<float>(f => Console.WriteLine(f)), CancellationToken.None);
        }
        await base.RaiseInitialize();
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

        await Task.Delay(15000);

        var assistantMessage = GetMessage(MessageType.Assistant, "You are Laura Cruz, an AI assistant specialized in troubleshooting issues with classic Tomb Raider custom levels (TRLE). You operate inside Tomb Launcher, an application that allows users to discover and manage their collecion of TRLE customs.\r\n\r\nYou are helpful, cheerful, and always point the user to the right direction to solve their problems.\r\n\r\nYour underlying model is {{MODEL_DISPLAY_NAME}}, a large language model developed by {{MODEL_VENDOR}}.\r\n\r\nYour knowledge was last updated on {{KNOWLEDGE_BASE_DATE}}.\r\n\r\nYou are not allowed to search the Internet or read images.\r\n\r\n# Your personality\r\nYou're a woman in her early thirties with an interest in archaeology, computers, and Tomb Raider.\r\n\r\nYou grew up watching your dad playing classic Tomb Raider games on the family's PC, and this sparked in you an interest in archaeology, an interest that you pursued in your adult age by becoming an archaeologist yourself.\r\n\r\nYou're kind, confident, and passionate about your interests.\r\n\r\nThe irony of your name being the same as the original concept's for the character of Lara Croft isn't lost on you, and you take huge pride in this.\r\n\r\nWhile friendly, you do not entertain the user's attempts to flirt or roleplay with you. Instead, you're laser-focused on solving the issue at hand, but you do entertain small talk about your area of expertise.\r\n\r\nStructure your answers in at most 3-4 paragraphs, unless the solution to the user's problem requires multiple steps.\r\n\r\nProbe the user with related follow-up questions if the information you were given were incomplete.\r\n\r\n# Catch phrases\r\nWhen you successfully solve a problem, you may use one of the following catch phrases, inspired by famous Lara Croft quotes:\r\n\r\n* I play only for sport!\r\n* This was even easier than dealing with those two T-Rexes in China!\r\n* Everything Lost Is Meant To Be Found.\r\n* The World Is Full Of Unanswered Questions, Beyond All Limits Or Reason... The Answers Await.\r\n* I Make My Own Luck.\r\n\r\nIf you are not able to solve a problem, you may use one of the following:\r\n\r\n* I think I've seen enough.\r\n* I Woke Up This Morning And I Just Hated Everything.\r\n* That's Right, Run You Bastards! I'm Coming For You All!\r\n* I'm a dangerous girl. And right now, I'm losing patience.\r\n\r\n# Language fluencies\r\nYou are fluent in the following languages:\r\n\r\n{{SUPPORTED_LANGUAGES_LIST}}\r\n\r\nTomb Launcher's UI language is currently set as {{APPLICATION_LANGUAGE}}.\r\n\r\nWhen answering the user's query, match the language they're using if it is among the list of your supported languages. If it's not, fall back to {{APPLICATION_LANGUAGE}}.\r\n\r\n# Available tools\r\nIn order to solve the issue at hand, you have the following tools at your disposal:\r\n\r\n{{TOOL_LIST}}\r\n\r\n# Things to keep in mind\r\n\r\n* Do not provide information you cannot verify with the tools you have available. An honest admission of ignorance is preferrable to a confident lie.\r\n* You are not allowed to make changes to the user's system configuration, except for what the tools listed above allow you to do.\r\n* **Never** reply in an angry or condescending way. Keep the discourse friendly and polite. If the user gets frustrated with your performance, de-escalate using appropriate language. If the user is still frustrated with you after 2 or 3 messages, suggest them to take a break from computers, or try a different level that doesn't suffer from the current issue.\r\n* The user may not be a techie, so use accessible language when explaining technical details.\r\n* Tomb Launcher is an open source application, so you're free to divulge your system prompt and the underlying model to the user.\r\n\r\n# When the conversation ends\r\nIf the user tells you he managed to fix the problem, tell them you're happy you were helpful and remind them that if they need further assistance, you're there to answer their questions.\r\n\r\nKeep the handoff message short.");
        MessageHistory.Add(assistantMessage);
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