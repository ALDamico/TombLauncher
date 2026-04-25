using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class NotificationListViewModel : ViewModelBase
{
    public NotificationListViewModel()
    {
        Notifications = [];
        Notifications.CollectionChanged += OnNotificationsChanged;
    }

    private void OnNotificationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems.Count: > 0 })
        {
            HasNewItems = true;
        }
        OnPropertyChanged(nameof(Notifications));
        RaiseCanExecuteChanged(ClearAllCommand);
    }

    [ObservableProperty] private ObservableCollection<NotificationViewModel> _notifications;
    [ObservableProperty] private bool _hasNewItems;

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void ClearAll() => Notifications.Clear();

    private bool CanClear => Notifications.Any();

    [RelayCommand]
    private void MarkNoNewElements()
    {
        HasNewItems = false;
    }
}