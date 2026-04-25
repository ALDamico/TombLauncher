using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SaveGameListFilter : ObservableObject
{
    [ObservableProperty] private int? _slotNumber;
    [ObservableProperty] private bool _startOfLevelOnly;
}