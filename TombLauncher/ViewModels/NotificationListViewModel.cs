using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class NotificationListViewModel : ViewModelBase
{
    public NotificationListViewModel()
    {
        Notifications = new ObservableCollection<NotificationViewModel>();
        Notifications.CollectionChanged += OnNotificationsChanged;
        ClearAllCmd = new RelayCommand(() => Notifications.Clear(), () => Notifications.Any());
        MarkNoNewElementsCmd = new RelayCommand(MarkNoNewElements);
    }

    private void OnNotificationsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            HasNewItems = true;
        }
        OnPropertyChanged(nameof(Notifications));
        RaiseCanExecuteChanged(ClearAllCmd);
    }

    [ObservableProperty] private ObservableCollection<NotificationViewModel> _notifications;
    [ObservableProperty] private bool _hasNewItems;
    public ICommand ClearAllCmd { get; }
    public ICommand MarkNoNewElementsCmd { get; }

    private void MarkNoNewElements()
    {
        HasNewItems = false;
    }
}