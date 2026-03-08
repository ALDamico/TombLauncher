---
description: How to add a new settings section to the Settings page
---

# Adding a settings section

A settings section consists of 5 changes across the codebase. Use the simplest existing section (`RandomGameSettingsView`) as a template.

## 1. Configuration model

Add the new properties to three files (they share the same interface):

1. **`Configuration/IAppConfiguration.cs`** — add `bool? NewProperty { get; set; }` (or appropriate type)
2. **`Configuration/AppConfiguration.cs`** — add matching property
3. **`Configuration/AppConfigurationWrapper.cs`** — add Coalesce wrapper:
```csharp
public bool? NewProperty
{
    get => User.NewProperty.Coalesce(Defaults.NewProperty);
    set => User.NewProperty = value.DefaultIfEquals(Defaults.NewProperty);
}
```

4. **`appsettings.json`** — add default value (this is the `Defaults` configuration)

## 2. ViewModel

Create `ViewModels/Pages/Settings/NewSectionSettingsViewModel.cs`:

```csharp
public partial class NewSectionSettingsViewModel : SettingsSectionViewModelBase
{
    public NewSectionSettingsViewModel(PageViewModel settingsPage)
        : base("SECTION_TITLE_KEY", settingsPage) { }

    [ObservableProperty] private bool _myProperty;
}
```

- The constructor's first argument is a **localization key** (resolved via `GetLocalizedString()`)
- Use `[ObservableProperty]` for simple properties or manual `SetProperty(ref ..., value, true)` for validated ones

## 3. View

Create `Views/NewSectionSettingsView.axaml`:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:loc="using:TombLauncher.Localization"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:settings="clr-namespace:TombLauncher.ViewModels.Pages.Settings"
             mc:Ignorable="d"
             x:Class="TombLauncher.Views.NewSectionSettingsView"
             x:DataType="settings:NewSectionSettingsViewModel">
    <StackPanel Orientation="Vertical">
        <!-- Controls bound to ViewModel properties -->
    </StackPanel>
</UserControl>
```

The ViewLocator will auto-discover this view for the matching ViewModel. No registration needed.

## 4. Wire into SettingsPageViewModel

In `ViewModels/Pages/SettingsPageViewModel.cs`, inside `OnNavigatedTo()`:

1. Create an instance of the new ViewModel
2. Populate it from `_settingsProvider` or `_appConfiguration`
3. Add it to `Sections`:
```csharp
var newSection = new NewSectionSettingsViewModel(this) { MyProperty = ... };
Sections.Add(newSection);
```

## 5. Save logic

In `Services/SettingsPageService.cs`, inside `Save()`:

1. Extract the section: `var newSection = viewModel.Sections.OfType<NewSectionSettingsViewModel>().First();`
2. Map values back to configuration: `_appConfiguration.NewProperty = newSection.MyProperty;`

The save method already serializes `_appConfiguration.User` to `appsettings.user.json`.

## 6. Localization

Add the section title key (used in the constructor) to both `en-US.axaml` and `it-IT.axaml`. Also add keys for any labels used in the view.

## Key conventions

- Properties that should fall back to defaults use **nullable types** (`bool?`, `int?`) with `.Coalesce()`
- The `SettingsSectionViewBase.axaml` wraps each section with a `settings-h1` header — you only need to provide the content
- `IsChanged` tracking is automatic via `OnPropertyChanged` in `SettingsSectionViewModelBase`
- Save button enables only when `IsChanged && !HasPendingEdits && !HasErrors`
