using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.Services;

public interface IViewService
{
    LocalizationManager LocalizationManager { get; }
    NavigationManager NavigationManager { get; }
}