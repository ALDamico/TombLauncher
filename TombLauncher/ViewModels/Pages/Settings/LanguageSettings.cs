using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LanguageSettings : ViewModelBase
{
    [ObservableProperty] private CultureInfo _applicationLanguage;
    [ObservableProperty] private ObservableCollection<CultureInfo> _availableLanguages;
}