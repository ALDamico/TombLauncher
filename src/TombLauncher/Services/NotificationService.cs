using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Notifications;

namespace TombLauncher.Services;

public class NotificationService
{
    public NotificationService(NotificationListViewModel notificationListViewModel)
    {
        _notificationListViewModel = notificationListViewModel;
        GenericDismissNotificationCmd = new AsyncRelayCommand<NotificationViewModel>(DismissNotification);
    }

    private NotificationListViewModel _notificationListViewModel;

    public async Task AddNotificationAsync(NotificationViewModel notificationViewModel)
    {
        notificationViewModel.DismissCmd = GenericDismissNotificationCmd;
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _notificationListViewModel.Notifications.Add(notificationViewModel);
        });
    }

    public async Task AddErrorNotificationAsync(string title, string errorMessage, PackIconRemixIconKind icon)
    {
        var notificationViewModel = new NotificationViewModel()
        {
            IsOpenable = false,
            IsCancelable = false,
            IsDismissable = true,
            Title = title.GetLocalizedString(),
            Content = new StringIconNotificationViewModel() { Icon = icon, Text = errorMessage },
            Type = NotificationType.Error
        };
        await AddNotificationAsync(notificationViewModel);
    }

    public void AddErrorNotification(string title, string errorMessage, PackIconRemixIconKind icon)
    {
        AddErrorNotificationAsync(title, errorMessage, icon).GetAwaiter().GetResult();
    }

    public async Task AddErrorNotificationAsync(string errorMessage, PackIconRemixIconKind icon)
    {
        await AddErrorNotificationAsync("AN_ERROR_OCCURRED".GetLocalizedString(), errorMessage, icon);
    }

    public void AddErrorNotification(string errorMessage, PackIconRemixIconKind icon)
    {
        AddErrorNotificationAsync(errorMessage, icon).GetAwaiter().GetResult();
    }

    public async Task AddSuccessNotification(string title, string message)
    {
        var notificationViewModel = new NotificationViewModel()
        {
            IsOpenable = false,
            IsCancelable = false,
            IsDismissable = true,
            Title = title.GetLocalizedString(),
            Content = new StringNotificationViewModel() { Text = message },
            Type = NotificationType.Success
        };
        await AddNotificationAsync(notificationViewModel);
    }

    private async Task DismissNotification(NotificationViewModel? thisNotification)
    {
        if (thisNotification == null) return;
        await Dispatcher.UIThread.InvokeAsync(() => thisNotification.IsClosing = true);
        await Task.Delay(350);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _notificationListViewModel.Notifications.Remove(thisNotification);
        });
    }

    public void RemoveNotification(NotificationViewModel notificationViewModel)
    {
        if (notificationViewModel == null) return;
        Dispatcher.UIThread.Invoke(() =>
        {
            _notificationListViewModel.Notifications.Remove(notificationViewModel);
        });
    }

    private ICommand GenericDismissNotificationCmd { get; }
}