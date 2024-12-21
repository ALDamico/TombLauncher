using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Styling;
using Avalonia.Threading;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Utils;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Pages;
using TombLauncher.ViewModels.Pages.Settings;
using ApplicationLanguageViewModel = TombLauncher.ViewModels.Pages.Settings.ApplicationLanguageViewModel;

namespace TombLauncher.Services;

public class SettingsService : IViewService
{
    public SettingsService(ILocalizationManager localizationManager, 
        NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService, SettingsUnitOfWork settingsUnitOfWork, MapperConfiguration mapperConfiguration)
    {
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _settingsUnitOfWork = settingsUnitOfWork;
        _mapper = mapperConfiguration.CreateMapper();
    }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private SettingsUnitOfWork _settingsUnitOfWork;
    private IMapper _mapper;

    public List<ApplicationLanguageViewModel> GetSupportedLanguages()
    {
        var supportedLanguages = LocalizationManager.GetSupportedLanguages();
        return _mapper.Map<List<ApplicationLanguageViewModel>>(supportedLanguages);
    }

    public CultureInfo GetApplicationLanguage()
    {
        return _settingsUnitOfWork.GetApplicationLanguage();
    }

    public ThemeVariant GetApplicationTheme()
    {
        var targetSetting = _settingsUnitOfWork.GetThemeName();
        var themeToApply = ReflectionUtils.GetStaticInstanceByName<ThemeVariant>(targetSetting);
        return themeToApply;
    }

    public async Task Save(SettingsPageViewModel viewModel)
    {
        viewModel.SetBusy(true, "Saving application settings...".GetLocalizedString());
        var tf = new TaskFactory();
        await tf.StartNew(() => _settingsUnitOfWork.UpdateApplicationLanguage(viewModel.LanguageSettings.ApplicationLanguage.CultureInfo))
            .ContinueWith(_ => Dispatcher.UIThread.Invoke(() =>  MessageBoxService.Show("Language changed", "The language has changed. Restart the application for this change to take effect properly.", MsgBoxButton.Ok, MsgBoxImage.Information)));
        var mappedDownloaderConfigs =
            _mapper.Map<List<DownloaderConfigDto>>(viewModel.DownloaderSettings.AvailableDownloaders);
        await tf.StartNew(() => _settingsUnitOfWork.UpdateDownloaderConfigurations(mappedDownloaderConfigs));
        await tf.StartNew(() =>
            _settingsUnitOfWork.UpdateAppTheme(viewModel.AppearanceSettings.SelectedTheme.Key.ToString()));
        viewModel.ClearBusy();
    }

    public List<DownloaderViewModel> GetDownloaders()
    {
        return _mapper.Map<List<DownloaderViewModel>>(_settingsUnitOfWork.GetDownloaderConfigurations());
    }
}