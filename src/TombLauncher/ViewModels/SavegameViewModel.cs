using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class SavegameViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial int Id { get; set; }
    [ObservableProperty]
    public partial string Filename { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int SlotNumber { get; set; }
    [ObservableProperty]
    public partial int? SaveNumber { get; set; }
    [ObservableProperty]
    public partial string LevelName { get; set; } = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteSavegameCommand))]
    public partial bool IsStartOfLevel { get; set; }
    [ObservableProperty]
    public partial DateTime? BackedUpOn { get; set; }
    [ObservableProperty]
    public partial long Length { get; set; }
    
    [ObservableProperty]
    public partial Bitmap? Screenshot { get; set; }
    public ICommand? UpdateStartOfLevelStateCommand { get; set; }
    public IRelayCommand? DeleteSavegameCommand { get; set; }
    public ICommand? RestoreSavegameCommand { get; set; }
}