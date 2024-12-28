using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class NotificationService
{
    public NotificationService(NotificationListViewModel notificationListViewModel)
    {
        _notificationListViewModel = notificationListViewModel;
    }

    private NotificationListViewModel _notificationListViewModel;

    public async Task AddNotification(NotificationViewModel notificationViewModel)
    {
        notificationViewModel.DismissCmd = new RelayCommand<NotificationViewModel>((thisNotification) =>
            _notificationListViewModel.Notifications.Remove(thisNotification));
        _notificationListViewModel.Notifications.Add(notificationViewModel);
    }
}