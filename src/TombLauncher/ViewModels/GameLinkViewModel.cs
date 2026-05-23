using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class GameLinkViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public partial LinkType LinkType { get; set; }

    [ObservableProperty]
    public partial string Link { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BaseUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;
}