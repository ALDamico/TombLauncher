using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace TombLauncher.ViewModels.Dialogs;

public class GameUninstallConfirmDialogViewModel : DialogViewModel
{
    public GameUninstallConfirmDialogViewModel()
    {
        AcceptCommand = new RelayCommand(Accept);
        CancelCommand = new RelayCommand(() => InvokeRequestCloseDialog(new RequestCloseDialogEventArgs(false)));
    }

    public GameMetadataViewModel Game
    {
        get;
        init => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    private void Accept()
    {
        var eventArgs = new RequestCloseDialogEventArgs(true);
        InvokeRequestCloseDialog(eventArgs);
    }
}