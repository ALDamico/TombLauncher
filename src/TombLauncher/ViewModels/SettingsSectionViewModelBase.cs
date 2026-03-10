using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class SettingsSectionViewModelBase : ObservableValidator, IChangeTracking
{
    protected SettingsSectionViewModelBase(string sectionTitle, PageViewModel settingsPage, PackIconRemixIconKind sectionIcon = default)
    {
        SectionTitle = sectionTitle.GetLocalizedString();
        SettingsPage = settingsPage;
        SectionIcon = sectionIcon;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsChanged) || e.PropertyName == nameof(EditInProgress))
        {
            base.OnPropertyChanged(e);
            return;
        }

        if (!IgnoreChangesHelper.IsIgnored(GetType(), e.PropertyName))
        {
            if (e.PropertyName != nameof(EditInProgress) || !EditInProgress)
                IsChanged = true;
        }

        base.OnPropertyChanged(e);
    }

    [ObservableProperty]
    [IgnoreChanges]
    private string _sectionTitle;

    [ObservableProperty]
    [IgnoreChanges]
    private PackIconRemixIconKind _sectionIcon;

    [ObservableProperty]
    [IgnoreChanges]
    private string _infoTipHeader = string.Empty;

    [ObservableProperty]
    [IgnoreChanges]
    private string _infoTipContent = string.Empty;

    [ObservableProperty]
    [IgnoreChanges]
    private PageViewModel _settingsPage;
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
            return;
        IsChanged = false;
    }

    /// <summary>
    /// Writes this section's ViewModel properties to the user configuration.
    /// Override in each section to handle its specific properties.
    /// Side effects (e.g. theme apply, language change) are NOT handled here.
    /// </summary>
    public virtual void ApplyTo(AppConfiguration userConfig) { }

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