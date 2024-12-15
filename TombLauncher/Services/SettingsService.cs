using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Threading;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Settings;
using ApplicationLanguageViewModel = TombLauncher.ViewModels.Pages.Settings.ApplicationLanguageViewModel;

namespace TombLauncher.Services;

public class SettingsService : IViewService
{
    public SettingsService(ILocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService, SettingsUnitOfWork settingsUnitOfWork, ISettingsVisitor settingsVisitor, MapperConfiguration mapperConfiguration)
    {
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _settingsUnitOfWork = settingsUnitOfWork;
        _settingsVisitor = settingsVisitor;
        _mapper = mapperConfiguration.CreateMapper();
    }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private SettingsUnitOfWork _settingsUnitOfWork;
    private ISettingsVisitor _settingsVisitor;
    private IMapper _mapper;

    public List<ApplicationLanguageViewModel> GetSupportedLanguages()
    {
        var supportedLanguages = LocalizationManager.GetSupportedLanguages();
        return _mapper.Map<List<ApplicationLanguageViewModel>>(supportedLanguages);
    }

    public async Task Save(SettingsPageViewModel viewModel)
    {
        viewModel.IsBusy = true;
        viewModel.BusyMessage = "Saving application settings...".GetLocalizedString();
        var tf = new TaskFactory();
        await tf.StartNew(() => _settingsUnitOfWork.UpdateApplicationLanguage(viewModel.LanguageSettings.ApplicationLanguage.CultureInfo))
            .ContinueWith(t => _settingsVisitor.Visit(LocalizationManager))
            .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>  MessageBoxService.Show("Language changed", "The language has changed. Restart the application for this change to take effect properly.", MsgBoxButton.Ok, MsgBoxImage.Information)));
        viewModel.IsBusy = false;
        viewModel.BusyMessage = null;
    }

    public List<DownloaderViewModel> GetDownloaders()
    {
        return _mapper.Map<List<DownloaderViewModel>>(_settingsUnitOfWork.GetDownloaderConfigurations());
    }
}