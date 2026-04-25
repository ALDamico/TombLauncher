using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class SavegameSlotViewModel : ObservableObject
{
    [ObservableProperty] private string _header = string.Empty;
    [ObservableProperty] private int? _saveSlot;
    [ObservableProperty] private bool _isEnabled;
    public ICommand? FilterCommand { get; set; }
}