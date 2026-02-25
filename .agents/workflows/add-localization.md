---
description: How to add or update localized strings
---

# Adding/updating localized strings

## Localization files

Strings reside in:
- `TombLauncher.Localization/Localization/en-US.axaml` (English — reference language)
- `TombLauncher.Localization/Localization/it-IT.axaml` (Italian)

## Format

Strings use the Avalonia ResourceDictionary format:
```xml
<system:String x:Key="LocalizationKey">Translated text</system:String>
```

## Rules

1. **ALWAYS** add the key to **both** files (en-US and it-IT)
2. Use descriptive English keys (e.g. `Game details`, `Save game`, `Settings`)
3. For strings with parameters, use `{0}`, `{1}`, etc.
4. In C# code, use the extension method: `"LocalizationKey".GetLocalizedString()`
5. In AXAML, use the binding: `{DynamicResource LocalizationKey}`
