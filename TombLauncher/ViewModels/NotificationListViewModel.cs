using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class NotificationListViewModel : ViewModelBase
{
    public NotificationListViewModel()
    {
        Notifications = new ObservableCollection<NotificationViewModel>();
        Notifications.CollectionChanged += OnNotificationsChanged;
    }

    private void OnNotificationsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            HasNewItems = true;
        }
    }

    [ObservableProperty] private ObservableCollection<NotificationViewModel> _notifications;
    [ObservableProperty] private bool _hasNewItems;
}