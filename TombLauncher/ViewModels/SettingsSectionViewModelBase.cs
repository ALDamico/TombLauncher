using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class SettingsSectionViewModelBase : ObservableValidator, IChangeTracking
{
    protected SettingsSectionViewModelBase(string sectionTitle)
    {
        SectionTitle = sectionTitle.GetLocalizedString();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        IsChanged = true;
    }

    [ObservableProperty] private string _sectionTitle;
    [ObservableProperty] private string _infoTipHeader;
    [ObservableProperty] private string _infoTipContent;
    internal void RaiseCanExecuteChanged<T>(ICommand command)
    {
        if (command is RelayCommand<T> relayCommand)
            relayCommand.NotifyCanExecuteChanged();
        else if (command is AsyncRelayCommand<T> asyncRelayCommand)
            asyncRelayCommand.NotifyCanExecuteChanged();
    }

    internal void RaiseCanExecuteChanged(ICommand command)
    {
        if (command is RelayCommand relayCommand)
        {
            relayCommand.NotifyCanExecuteChanged();
        }
        else if (command is AsyncRelayCommand asyncRelayCommand)
        {
            asyncRelayCommand.NotifyCanExecuteChanged();
        }
    }

    public void AcceptChanges()
    {
        if (HasErrors)
            IsChanged = false;
        IsChanged = false;
    }

    public bool IsChanged { get; internal set; }
}