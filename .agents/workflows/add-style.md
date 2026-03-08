---
description: How to use and add style classes to AppStyles.axaml
---

# Style Classes Reference

All style classes are defined in `src/TombLauncher/Assets/AppStyles.axaml`.
Apply them with `Classes="class-name"` on the corresponding control type.

## Buttons

| Class | Applies to | Description |
|-------|-----------|-------------|
| `btn-primary` | `:is(Button)` | PrimaryBrush bg, pill shape (CornerRadius 50), white text. Has `:pointerover` with PrimaryPointerOverBrush. |
| `btn-success` | `:is(Button)`, `:is(SplitButton)`, `RoundIconButton` | SuccessBrush bg, pill shape, white text. Full pseudoclass support (pointerover, pressed, flyout-open). |
| `btn-danger` | `:is(Button)`, `:is(SplitButton)`, `RoundIconButton` | DangerBrush bg, pill shape, white text. Has `:pointerover` with DangerPointerOverBrush. |
| `round-button` | `:is(Button)` | 24×24 transparent circular button, hover highlight `#80e0e0e0`. |
| `icon-only` | `:is(Button)`, `:is(ToggleButton)` | Transparent bg, no border, hand cursor. ToggleButton variant includes `:checked` support. |
| `hyperlink` | `Button` | Custom template: underlined AccentBrush text, transparent bg, hand cursor. |
| `stretched` | `:is(ContentControl)`, `:is(SplitButton)`, `Expander` | 36px height, stretch horizontal, centered content. |

## Typography

| Class | Applies to | Description |
|-------|-----------|-------------|
| `h1` | `TextBlock` | TombRaider font, 36px, centered. |
| `settings-h1` | `TextBlock` | TombRaider font, 36px, left-aligned. *(legacy — now replaced by icon+title header)* |
| `paragraph` | `TextBlock` | Margin `0,5`. |
| `small` | `TextBlock`, `CheckBox` | 9px, DemiBold. |
| `label` | `TextBlock` | Vertically centered, margin `5,0`, padding `2`. |
| `text-muted` | `TextBlock` | SecondaryBrush foreground. |
| `italicsLabel` | `Run` | DemiBold + italic. |

## Cards & Containers

| Class | Applies to | Description |
|-------|-----------|-------------|
| `card-background` | `Border` | CardBackgroundBrush background. |
| `interactive-card` | `Border` | Scale(1.02) + box shadow on hover, hand cursor. Animated transitions. |
| `card-content` | `TextBlock` | ColoredButtonTextBrush foreground (for text inside colored cards). |
| `search-bar` | `Border` | Rounded (24), shadow, CardBackgroundBrush bg, focus/hover transitions. Inner TextBox is auto-styled transparent. |
| `filter-chip` | `Border` | CornerRadius 8, CardBackgroundBrush bg, CardBorderBrush border. |
| `stackedButtons` | `StackPanel` | Children `Button` get margin `0,5`, stretch horizontal, centered text. |

## Icons

| Class | Applies to | Description |
|-------|-----------|-------------|
| `text-muted` | `PackIconRemixIcon` | SecondaryBrush foreground. |
| `warning` | `PackIconRemixIcon` | WarningBrush foreground. |

## Layout

| Class | Applies to | Description |
|-------|-----------|-------------|
| `padded` | `:is(Control)` | Margin `5` on all sides. |

## How to Add a New Style Class

1. Open `src/TombLauncher/Assets/AppStyles.axaml`
2. Add a `<Style>` element with the appropriate selector:
   - Use `:is(Button).my-class` for styles that should apply to Button and subclasses
   - Use `Button.my-class` for styles that apply only to Button itself
3. If the style needs hover/pressed effects, add corresponding pseudoclass selectors:
   ```xml
   <Style Selector=":is(Button).my-class">
       <Setter Property="Background" Value="{DynamicResource MyBrush}" />
   </Style>
   <Style Selector=":is(Button):pointerover.my-class /template/ ContentPresenter">
       <Setter Property="Background" Value="{DynamicResource MyPointerOverBrush}" />
   </Style>
   ```
4. Use `DynamicResource` for theme-aware colors (see theme files in `Assets/Themes/`)
5. Update this workflow file with the new class documentation
