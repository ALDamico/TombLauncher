using System;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Ai;

namespace TombLauncher.ViewModels.Ai;

public partial class AiMessageViewModel : ObservableObject
{
    public MessageType MessageType { get; set; }
    [ObservableProperty] private string _text = "";
    [ObservableProperty] private DateTime _sentDate;
}