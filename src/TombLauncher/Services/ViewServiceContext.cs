using TombLauncher.Contracts.Localization;

namespace TombLauncher.Services;

public record ViewServiceContext(
    ILocalizationManager LocalizationManager,
    NavigationManager NavigationManager,
    IPopupService PopupService);
