using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class SettingsSectionViewModelBase : ObservableValidator, IChangeTracking
{
    protected SettingsSectionViewModelBase(string sectionTitle, PageViewModel settingsPage)
    {
        SectionTitle = sectionTitle.GetLocalizedString();
        SettingsPage = settingsPage;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IsChanged) or nameof(SectionTitle) or nameof(InfoTipContent) or nameof(InfoTipHeader))
            return;
        if (e.PropertyName != nameof(EditInProgress) || !EditInProgress)
            IsChanged = true;
        base.OnPropertyChanged(e);
    }

    [ObservableProperty] private string _sectionTitle;
    [ObservableProperty] private string _infoTipHeader;
    [ObservableProperty] private string _infoTipContent;
    [ObservableProperty] private PageViewModel _settingsPage;
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

    public bool IsChanged { get; protected set; }
    private bool _editInProgress;

    public bool EditInProgress
    {
        get => _editInProgress; 
        protected set
        {
            _editInProgress = value;
            OnPropertyChanged();
        }
    }
}