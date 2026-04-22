using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Ai;
using TombLauncher.Core.Extensions;
using TombLauncher.ViewModels.Ai;

namespace TombLauncher.ViewModels.Pages;

public partial class AiChatViewModel : PageViewModel
{
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private string _currentText = "";
    [ObservableProperty] private ObservableCollection<AiMessageViewModel> _messageHistory = new();

    [RelayCommand(CanExecute = nameof(CanSendMessage))]
    private async Task SendMessage()
    {
        var currentText = CurrentText;
        IsGenerating = true;
        var userMessage = GetMessage(MessageType.User, currentText);
        MessageHistory.Add(userMessage);
        CurrentText = "";

        await Task.Delay(500);

        var fakeAiMessage = GetMessage(MessageType.Assistant, "Let me try and fix this for you...");
        MessageHistory.Add(fakeAiMessage);
        IsGenerating = false;
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