using AutoMapper;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Services;

public record ViewServiceContext(
    ILocalizationManager LocalizationManager,
    NavigationManager NavigationManager,
    IMessageBoxService MessageBoxService,
    IDialogService DialogService,
    IMapper Mapper);
