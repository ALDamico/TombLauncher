using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Services;
using TombLauncher.Utils;

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
    [ObservableProperty] private Version? _applicationVersion;
    [ObservableProperty] private string _githubLink;
    [ObservableProperty] private string _websiteLink;

    [RelayCommand]
    private void OpenLink(string url) => _platformSpecificFeatures.OpenUrl(url);
}