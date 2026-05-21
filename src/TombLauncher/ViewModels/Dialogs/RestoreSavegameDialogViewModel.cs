using System.Collections.ObjectModel;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using NetSparkleUpdater.UI.Avalonia.Helpers;

namespace TombLauncher.ViewModels.Dialogs;

public class RestoreSavegameDialogViewModel : DialogViewModel
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

    public ObservableCollection<SavegameSlotViewModel> Slots
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    public SavegameSlotViewModel SelectedSlot
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    public byte[] Data
    {
        get;
        init => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    public string TargetDirectory
    {
        get;
        init => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    public string BaseFileName
    {
        get;
        init => RaiseAndSetIfChanged(ref field, value);
    } = null!;
}