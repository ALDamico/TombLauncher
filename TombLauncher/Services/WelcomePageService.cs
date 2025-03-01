﻿using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(AppCrashUnitOfWork appCrashUnitOfWork, ILocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        AppCrashUnitOfWork = appCrashUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = AppCrashUnitOfWork.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        var appCrashHostService = Ioc.Default.GetRequiredService<AppCrashHostService>();
        var appCrashHostViewModel = new AppCrashHostViewModel(appCrashHostService) { Crash = unnotifiedCrash };

        async void MarkAsNotified(AppCrashHostViewModel model)
        {
            await appCrashHostService.MarkAsNotified(model.Crash);
        }

        DialogService.ShowDialog(appCrashHostViewModel, MarkAsNotified);
    }
}