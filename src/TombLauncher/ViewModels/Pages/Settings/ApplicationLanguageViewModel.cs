using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class ApplicationLanguageViewModel : ObservableObject
{
    [ObservableProperty] private CultureInfo _cultureInfo = null!;
    [ObservableProperty] private string _countryIso2Code = string.Empty;
    [ObservableProperty] private string _dictionaryName = string.Empty;
    [ObservableProperty] private string _displayName = string.Empty;

    public bool IsSystemLanguage =>
        CultureInfo != null &&
        (CultureInfo.Equals(CultureInfo.InstalledUICulture) ||
         CultureInfo.Equals(CultureInfo.InstalledUICulture.Parent));
}