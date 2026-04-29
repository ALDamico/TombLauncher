using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
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

    /// <summary>
    /// Returns true if this section has any visible content.
    /// Override in sections that may be empty depending on platform or configuration.
    /// </summary>
    public virtual bool HasContent => true;

    public bool IsChanged { get; protected set; }

    public bool EditInProgress
    {
        get;
        protected set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}