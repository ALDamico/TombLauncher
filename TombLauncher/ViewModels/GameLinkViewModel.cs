using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class GameLinkViewModel : ViewModelBase
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private LinkType _linkType;
    [ObservableProperty] private string _link = string.Empty;
    [ObservableProperty] private string _baseUrl = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;
}