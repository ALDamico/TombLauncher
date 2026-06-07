using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Ai;

namespace TombLauncher.ViewModels.Ai;

public partial class AiMessageViewModel : ObservableObject
{
    public MessageType MessageType { get; set; }
    [ObservableProperty]
    public partial string Text { get; set; } = "";

    [ObservableProperty]
    public partial DateTime SentDate { get; set; }
}