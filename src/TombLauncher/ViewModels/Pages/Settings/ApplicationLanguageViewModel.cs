using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class ApplicationLanguageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial CultureInfo CultureInfo { get; set; } = null!;

    [ObservableProperty]
    public partial string CountryIso2Code { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DictionaryName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DisplayName { get; set; } = string.Empty;

    public bool IsSystemLanguage =>
        CultureInfo != null &&
        (CultureInfo.Equals(CultureInfo.InstalledUICulture) ||
         CultureInfo.Equals(CultureInfo.InstalledUICulture.Parent));
}