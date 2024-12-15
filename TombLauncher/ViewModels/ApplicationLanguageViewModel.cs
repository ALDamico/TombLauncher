using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class ApplicationLanguageViewModel : ObservableObject
{
    [ObservableProperty] private CultureInfo _cultureInfo;
    [ObservableProperty] private string _countryIso2Code;
    [ObservableProperty] private string _dictionaryName;
    [ObservableProperty] private string _displayName;
}