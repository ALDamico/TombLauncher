using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class SettingsSectionViewModelBase : ViewModelBase
{
    protected SettingsSectionViewModelBase(string sectionTitle)
    {
        SectionTitle = sectionTitle.GetLocalizedString();
    }
    [ObservableProperty] private string _sectionTitle;
}