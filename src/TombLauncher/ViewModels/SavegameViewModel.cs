using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class SavegameViewModel : ViewModelBase
{
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _filename = string.Empty;
    [ObservableProperty] private int _slotNumber;
    [ObservableProperty] private int? _saveNumber;
    [ObservableProperty] private string _levelName = string.Empty;
    [ObservableProperty][NotifyCanExecuteChangedFor(nameof(DeleteSavegameCommand))] private bool _isStartOfLevel;
    [ObservableProperty] private DateTime? _backedUpOn;
    [ObservableProperty] private long _length;
    public ICommand? UpdateStartOfLevelStateCommand { get; set; }
    public IRelayCommand? DeleteSavegameCommand { get; set; }
    public ICommand? RestoreSavegameCommand { get; set; }
}