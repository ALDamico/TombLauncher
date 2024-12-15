using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettings : ViewModelBase
{
    [ObservableProperty] private ApplicationLanguageViewModel _applicationLanguage;
    [ObservableProperty] private ObservableCollection<ApplicationLanguageViewModel> _availableLanguages;
}