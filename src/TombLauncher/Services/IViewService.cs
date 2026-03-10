using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Navigation;

namespace TombLauncher.Services;

public interface IViewService
{
    ViewServiceContext ViewContext { get; }
    ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    NavigationManager NavigationManager => ViewContext.NavigationManager;
}