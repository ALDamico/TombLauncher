---
description: How to create a reusable custom control
---

# Adding a custom control

Reusable custom controls reside in the **TombLauncher.Controls** project.

## 1. Create the control

Create in the root of `TombLauncher.Controls/`:
- `NewControl.axaml` — XAML template
- `NewControl.axaml.cs` — code-behind with `StyledProperty` or `DirectProperty`

## 2. Conventions

- Use `StyledProperty<T>` for externally configurable properties (support binding and styling)
- Use `DirectProperty<T>` for read-only or internal-use properties
- Follow the style of existing controls (`IconButton`, `LabeledTextBox`, `RatingControl`, etc.)
- If the control needs value converters, place them in `TombLauncher.Controls/ValueConverters/`

## 3. Dependencies

`TombLauncher.Controls` depends on:
- `Avalonia` + `Avalonia.Controls.DataGrid`
- `Material.Icons.Avalonia`
- `JamSoft.AvaloniaUI.Dialogs`
- `TombLauncher.Core`

Do not add dependencies on the `TombLauncher` (main app) project — this would cause a circular reference.
