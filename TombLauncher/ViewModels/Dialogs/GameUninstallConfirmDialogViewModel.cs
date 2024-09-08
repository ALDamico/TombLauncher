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

    private GameMetadataViewModel _game;

    public GameMetadataViewModel Game
    {
        get => _game;
        set => RaiseAndSetIfChanged(ref _game, value);
    }

    private void Accept()
    {
        var eventArgs = new RequestCloseDialogEventArgs(true);
        InvokeRequestCloseDialog(eventArgs);
    }
}