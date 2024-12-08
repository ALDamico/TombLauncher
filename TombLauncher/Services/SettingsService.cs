using System.Collections.Generic;
using System.Globalization;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.Services;

public class SettingsService : IViewService
{
    public SettingsService(LocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public LocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public List<CultureInfo> GetSupportedLanguages()
    {
        return LocalizationManager.GetSupportedLanguages();
    }
}