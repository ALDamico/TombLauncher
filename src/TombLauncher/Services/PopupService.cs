using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace TombLauncher.Services;

public class PopupService : IPopupService
{
    private readonly IMessageBoxService _messageBoxService;
    private readonly IDialogService _dialogService;

    public PopupService(IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        _messageBoxService = messageBoxService;
        _dialogService = dialogService;
    }

    // IMessageBoxService
    public Task<MsgBoxResult> Show(
        string caption,
        string messageBoxText,
        MsgBoxButton button,
        MsgBoxImage icon = MsgBoxImage.None,
        string? noButtonText = null,
        string? yesButtonText = null,
        string? cancelButtonText = null,
        string? checkBoxText = null)
        => _messageBoxService.Show(caption, messageBoxText, button, icon,
            noButtonText, yesButtonText, cancelButtonText, checkBoxText);

    public Task<MsgBoxResult> Show(IMsgBoxViewModel viewModel)
        => _messageBoxService.Show(viewModel);

    // IDialogService — ShowDialog (explicit to match interface constraints exactly)
    void IDialogService.ShowDialog<TViewModel>(TViewModel viewModel, Action<TViewModel> callback)
        => _dialogService.ShowDialog(viewModel, callback);

    void IDialogService.ShowDialog<TViewModel, TView>(TView view, TViewModel viewModel, Action<TViewModel> callback)
        => _dialogService.ShowDialog(view, viewModel, callback);

    // IDialogService — ShowChildWindow (explicit)
    void IDialogService.ShowChildWindow<TViewModel>(TViewModel viewModel, Action<TViewModel>? callback)
        => _dialogService.ShowChildWindow(viewModel, callback);

    void IDialogService.ShowChildWindow<TViewModel, TView>(TView view, TViewModel viewModel, Action<TViewModel>? callback)
        => _dialogService.ShowChildWindow(view, viewModel, callback);

    // IDialogService — StartWizard (explicit)
    void IDialogService.StartWizard<TViewModel>(TViewModel viewModel, Action<TViewModel>? callback)
        => _dialogService.StartWizard(viewModel, callback);

    // IDialogService — convenience wrappers usable without casting
    public void ShowDialog<TViewModel>(TViewModel viewModel, Action<TViewModel> callback)
        where TViewModel : IDialogViewModel
        => _dialogService.ShowDialog(viewModel, callback);

    // IDialogService — File / Folder pickers
    public Task<string?> OpenFolder(string title, string? startDirectory = null)
        => _dialogService.OpenFolder(title, startDirectory);

    public Task<string?> SaveFile(
        string title,
        IEnumerable<FilePickerFileType>? filters = null,
        string? defaultExtension = null,
        string? suggestedFileName = null)
        => _dialogService.SaveFile(title, filters, defaultExtension, suggestedFileName);

    public Task<string?> OpenFile(string title, IEnumerable<FilePickerFileType>? filters = null)
        => _dialogService.OpenFile(title, filters);

    public async Task<string[]?> OpenFiles(string title, IEnumerable<FilePickerFileType>? filters = null)
        => await _dialogService.OpenFiles(title, filters);
}
