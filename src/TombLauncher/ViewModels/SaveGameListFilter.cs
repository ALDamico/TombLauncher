using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SaveGameListFilter : ObservableObject
{
    [ObservableProperty]
    public partial int? SlotNumber { get; set; }

    [ObservableProperty]
    public partial bool StartOfLevelOnly { get; set; }
}