using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class AboutPageViewModel : PageViewModel
{
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public AboutPageViewModel(ISettingsProvider settingsProvider, IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _platformSpecificFeatures = platformSpecificFeatures;
        var coreSettings = settingsProvider.GetApplicationSettings();
        ApplicationVersion = VersionUtils.GetApplicationVersion();
        GithubLink = coreSettings.GitHubLink;
        WebsiteLink = coreSettings.WebsiteLink;
    }
    [ObservableProperty]
    public partial Version? ApplicationVersion { get; set; }

    [ObservableProperty]
    public partial string GithubLink { get; set; }

    [ObservableProperty]
    public partial string WebsiteLink { get; set; }

    [RelayCommand]
    private void OpenLink(string url) => _platformSpecificFeatures.OpenUrl(url);
}