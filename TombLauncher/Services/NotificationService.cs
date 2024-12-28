using System.Threading.Tasks;
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
        _notificationListViewModel.Notifications.Add(notificationViewModel);
    }
}