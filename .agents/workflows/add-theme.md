---
description: How to add a new application theme (dark and/or light variant)
---

# Add Theme

Each theme is a `ResourceDictionary` AXAML file that overrides the app's color tokens.

## Steps

1. **Create the theme file(s)** in `src/TombLauncher/Assets/Themes/`:
   - Dark variant: `<ThemeName>Theme.axaml`
   - Light variant (optional): `<ThemeName>Light.axaml`
   - Use an existing theme as template (e.g. `ScionTheme.axaml` for dark, `ScionLight.axaml` for light).

2. **Required color keys** — every theme MUST define all of these:

   | Key | Purpose |
   |-----|---------|
   | `CardBackgroundColor` | Color resource for card backgrounds |
   | `CardBackgroundBrush` | SolidColorBrush using CardBackgroundColor |
   | `CardBorderBrush` | Card border brush |
   | `PageBackgroundBrush` | Page background |
   | `SidebarBackgroundBrush` | Sidebar background |
   | `PrimaryColor` | Color resource for primary actions |
   | `PrimaryBrush` | SolidColorBrush using PrimaryColor |
   | `PrimaryPointerOverBrush` | Primary hover state |
   | `ColoredButtonTextBrush` | Text on colored buttons |
   | `AccentBrush` | Accent color |
   | `SuccessBrush` / `SuccessPointerOverBrush` | Success states |
   | `DangerBrush` / `DangerPointerOverBrush` | Danger states |
   | `WarningBrush` / `WarningPointerOverBrush` | Warning states |
   | `TextBrush` | Primary text color |
   | `MutedTextBrush` | Muted/secondary text |
   | `SecondaryBrush` | Secondary elements |

   Dark themes must also include Fluent overrides and interactive control states (ToggleSwitch, CheckBox, RadioButton). See `ScionTheme.axaml` for the full list.

3. **Register in `ThemeManager.cs`** (`src/TombLauncher/Services/ThemeManager.cs`):
   - Add a case in the `themeName` switch expression:
   ```csharp
   "<ThemeName>" => "avares://TombLauncher/Assets/Themes/<ThemeName>Theme.axaml",
   "<ThemeName> Light" => "avares://TombLauncher/Assets/Themes/<ThemeName>Light.axaml",
   ```

4. **Register in `AppearanceSettingsViewModel.cs`** (`src/TombLauncher/ViewModels/Pages/Settings/AppearanceSettingsViewModel.cs`):
   - Add entries to the `AvailableThemes` collection:
   ```csharp
   new ApplicationTheme("<ThemeName> (Dark)", "<ThemeName>", ThemeVariant.Dark),
   new ApplicationTheme("<ThemeName> (Light)", "<ThemeName> Light", ThemeVariant.Light),
   ```

5. **Build and verify**:
// turbo
```bash
dotnet build src/TombLauncher/TombLauncher.csproj --no-restore -v quiet
```

6. **Test manually**: run the app, go to Settings → Appearance, select the new theme, and verify all UI elements render correctly with the new colors.
