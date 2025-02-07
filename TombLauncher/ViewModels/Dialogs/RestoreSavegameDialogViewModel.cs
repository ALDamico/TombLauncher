using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using NetSparkleUpdater.UI.Avalonia.Helpers;

namespace TombLauncher.ViewModels.Dialogs;

public partial class RestoreSavegameDialogViewModel : DialogViewModel
{
    public RestoreSavegameDialogViewModel()
    {
        AcceptCommand = new RelayCommand(Accept);
        CancelCommand = new RelayCommand(() => InvokeRequestCloseDialog(new RequestCloseDialogEventArgs(false)));
    }

    private void Accept()
    {
        var eventArgs = new RequestCloseDialogEventArgs(true);
        InvokeRequestCloseDialog(eventArgs);
    }

    private ObservableCollection<SavegameSlotViewModel> _slots;

    public ObservableCollection<SavegameSlotViewModel> Slots
    {
        get => _slots;
        set => RaiseAndSetIfChanged(ref _slots, value);
    }
    private SavegameSlotViewModel _selectedSlot;

    public SavegameSlotViewModel SelectedSlot
    {
        get => _selectedSlot;
        set => RaiseAndSetIfChanged(ref _selectedSlot, value);
    }

    private byte[] _data;

    public byte[] Data
    {
        get => _data;
        set => RaiseAndSetIfChanged(ref _data, value);
    }

    private string _targetDirectory;

    public string TargetDirectory
    {
        get => _targetDirectory;
        set => RaiseAndSetIfChanged(ref _targetDirectory, value);
    }

    private string _baseFileName;

    public string BaseFileName
    {
        get => _baseFileName;
        set => RaiseAndSetIfChanged(ref _baseFileName, value);
    }
}