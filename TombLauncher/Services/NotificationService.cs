using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Notifications;

namespace TombLauncher.Services;

public class NotificationService
{
    public NotificationService(NotificationListViewModel notificationListViewModel)
    {
        _notificationListViewModel = notificationListViewModel;
        GenericDismissNotificationCmd = new RelayCommand<NotificationViewModel>(DismissNotification);
    }

    private NotificationListViewModel _notificationListViewModel;

    public async Task AddNotificationAsync(NotificationViewModel notificationViewModel)
    {
        notificationViewModel.DismissCmd = GenericDismissNotificationCmd;
        _notificationListViewModel.Notifications.Add(notificationViewModel);
        await Task.CompletedTask;
    }

    public void AddNotification(NotificationViewModel notificationViewModel)
    {
        AddNotificationAsync(notificationViewModel).GetAwaiter().GetResult();
    }

    public async Task AddErrorNotificationAsync(string title, string errorMessage, MaterialIconKind icon)
    {
        var notificationViewModel = new NotificationViewModel()
        {
            IsOpenable = false,
            IsCancelable = false,
            IsDismissable = true,
            Title = title.GetLocalizedString(),
            Content = new StringIconNotificationViewModel(){Icon = icon, Text = errorMessage}
        };
        await AddNotificationAsync(notificationViewModel);
    }

    public void AddErrorNotification(string title, string errorMessage, MaterialIconKind icon)
    {
        AddErrorNotificationAsync(title, errorMessage, icon).GetAwaiter().GetResult();
    }

    public async Task AddErrorNotificationAsync(string errorMessage, MaterialIconKind icon)
    {
        await AddErrorNotificationAsync("An error occurred".GetLocalizedString(), errorMessage, icon);
    }

    public void AddErrorNotification(string errorMessage, MaterialIconKind icon)
    {
        AddErrorNotificationAsync(errorMessage, icon).GetAwaiter().GetResult();
    }

    private void DismissNotification(NotificationViewModel thisNotification)
    {
        _notificationListViewModel.Notifications.Remove(thisNotification);
    }

    public void RemoveNotification(NotificationViewModel notificationViewModel)
    {
        _notificationListViewModel.Notifications.Remove(notificationViewModel);
    }

    private ICommand GenericDismissNotificationCmd { get; }
}