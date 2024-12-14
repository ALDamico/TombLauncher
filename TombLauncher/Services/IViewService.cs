using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.Services;

public interface IViewService
{
    ILocalizationManager LocalizationManager { get; }
    NavigationManager NavigationManager { get; }
    IMessageBoxService MessageBoxService { get; }
    IDialogService DialogService { get; }
}