using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.Services;

public interface IViewService
{
    LocalizationManager LocalizationManager { get; }
    NavigationManager NavigationManager { get; }
    IMessageBoxService MessageBoxService { get; }
    IDialogService DialogService { get; }
}