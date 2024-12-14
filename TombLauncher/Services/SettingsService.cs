using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Pages;
using TombLauncher.Views.Pages;

namespace TombLauncher.Services;

public class SettingsService : IViewService
{
    public SettingsService(ILocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService, SettingsUnitOfWork settingsUnitOfWork, ISettingsVisitor settingsVisitor)
    {
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _settingsUnitOfWork = settingsUnitOfWork;
        _settingsVisitor = settingsVisitor;
    }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private SettingsUnitOfWork _settingsUnitOfWork;
    private ISettingsVisitor _settingsVisitor;

    public List<CultureInfo> GetSupportedLanguages()
    {
        return LocalizationManager.GetSupportedLanguages();
    }

    public async Task Save(SettingsPageViewModel viewModel)
    {
        viewModel.IsBusy = true;
        viewModel.BusyMessage = "Saving application settings...".GetLocalizedString();
        var tf = new TaskFactory();
        await tf.StartNew(() => _settingsUnitOfWork.UpdateApplicationLanguage(viewModel.LanguageSettings.ApplicationLanguage))
            .ContinueWith(t => _settingsVisitor.Visit(LocalizationManager))
            .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>  MessageBoxService.Show("Language changed", "The language has changed. Restart the application for this change to take effect properly.", MsgBoxButton.Ok, MsgBoxImage.Information)));
        viewModel.IsBusy = false;
        viewModel.BusyMessage = null;
    }
}