---
description: How to add a new settings section to the Settings page
---

# Adding a settings section

A settings section consists of changes across 6 areas. Use an existing simple section (e.g. `WelcomePageSettingsViewModel`) as a template.

## 1. Configuration model

### 1a. Config POCO + read-only interface

Create two files in `Configuration/Sections/`:

**`INewSectionConfig.cs`** — read-only interface:
```csharp
namespace TombLauncher.Configuration.Sections;

public interface INewSectionConfig
{
    bool? MyProperty { get; }
    int? MyCount { get; }
}
```

**`NewSectionConfig.cs`** — POCO with setters:
```csharp
namespace TombLauncher.Configuration.Sections;

public class NewSectionConfig : INewSectionConfig
{
    public bool? MyProperty { get; set; }
    public int? MyCount { get; set; }
}
```

### 1b. Register in IAppConfiguration and AppConfiguration

In **`IAppConfiguration.cs`**, add the read-only interface property:
```csharp
INewSectionConfig NewSection { get; }
```

In **`AppConfiguration.cs`**, add the concrete property + explicit interface implementation:
```csharp
public NewSectionConfig NewSection { get; set; } = new();

// In the explicit interface implementation block:
INewSectionConfig IAppConfiguration.NewSection => NewSection;
```

### 1c. Add merged view in LayeredAppConfiguration

In **`LayeredAppConfiguration.cs`**, add a merged property:
```csharp
public INewSectionConfig NewSection => new NewSectionConfig
{
    MyProperty = User.NewSection.MyProperty.Coalesce(Defaults.NewSection.MyProperty),
    MyCount = User.NewSection.MyCount.Coalesce(Defaults.NewSection.MyCount)
};
```

### 1d. Default values

Add the section to **`appsettings.json`** with sensible defaults:
```json
"NewSection": {
    "MyProperty": true,
    "MyCount": 5
}
```

## 2. ViewModel

Create `ViewModels/Pages/Settings/NewSectionSettingsViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class NewSectionSettingsViewModel : SettingsSectionViewModelBase
{
    public NewSectionSettingsViewModel(PageViewModel settingsPage)
        : base("SECTION_TITLE_KEY", settingsPage, PackIconRemixIconKind.SomeIcon) { }

    [ObservableProperty] private bool _myProperty;
    [ObservableProperty] private int _myCount;

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.NewSection.MyProperty = MyProperty;
        userConfig.NewSection.MyCount = MyCount;
    }
}
```

Key points:
- Constructor's first argument is a **localization key**
- Use `[ObservableProperty]` for simple properties
- **`ApplyTo` is mandatory** — it writes the ViewModel's values to the User configuration
- Properties that should NOT trigger `IsChanged` get the `[IgnoreChanges]` attribute

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

1. Read the merged config section via `_appConfiguration.NewSection`
2. Create a VM instance and populate it
3. Add it to `Sections`:
```csharp
var ns = _appConfiguration.NewSection;
var newSection = new NewSectionSettingsViewModel(this)
{
    MyProperty = ns.MyProperty.GetValueOrDefault(true),
    MyCount = ns.MyCount.GetValueOrDefault(5)
};
Sections.Add(newSection);
```

## 5. Save logic

**No changes needed in `SettingsPageService.Save()`!**

The `Save()` method iterates all sections with `section.ApplyTo(_appConfiguration.User)`. Your new section's `ApplyTo` override handles the data writes automatically.

If your section has **side effects** (e.g. changing language, applying a theme at runtime), add them in `SettingsPageService.ApplySideEffects()`:

```csharp
private void ApplySideEffects(SettingsPageViewModel viewModel)
{
    // ... existing side effects ...

    // Your new side effect:
    var newSection = viewModel.Sections.OfType<NewSectionSettingsViewModel>().First();
    if (newSection.SomeProperty)
        _someService.DoSomething();
}
```

**Important:** Side effects belong in the service, NOT in the ViewModel's `ApplyTo()`.

## 6. Localization

Add the section title key (used in the constructor) to both `en-US.axaml` and `it-IT.axaml`. Also add keys for any labels used in the view.

## Key conventions

- Properties that fall back to defaults use **nullable types** (`bool?`, `int?`) with `.Coalesce()` in `LayeredAppConfiguration`
- Config sections have a **read-only interface** (`INewSectionConfig`) returned by `IAppConfiguration` and a **mutable POCO** (`NewSectionConfig`) used in `AppConfiguration`
- The `SettingsSectionViewBase.axaml` wraps each section with a `settings-h1` header — you only provide the content
- `IsChanged` tracking is automatic via `OnPropertyChanged` in `SettingsSectionViewModelBase`
- Properties decorated with `[IgnoreChanges]` are excluded from change tracking
- Save button enables only when `IsChanged && !HasPendingEdits && !HasErrors`
