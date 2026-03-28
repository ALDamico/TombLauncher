using TombLauncher.Contracts.Localization;

namespace TombLauncher.Services;

public interface IViewService
{
    ViewServiceContext ViewContext { get; }
    ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    NavigationManager NavigationManager => ViewContext.NavigationManager;
}