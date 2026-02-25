---
description: How to add a new game source downloader
---

# Adding a new game downloader

The application supports multiple sources for downloading Tomb Raider custom levels. Each source is implemented as a separate downloader.

## 1. Create the downloader directory

Create a new directory under `TombLauncher/Installers/Downloaders/NewSource/`.

## 2. Implement the downloader

Follow the structure of existing downloaders (`TRCustoms.org/`, `TRLE.net/`, `AspideTR.com/`):
- Implement the main downloader class
- Use `HtmlAgilityPack` for HTML parsing if needed
- Handle download and result merging via `IGameMerger`

## 3. Register in DI

In `App.axaml.cs`, in the `ConfigureDownloaders()` method:
```csharp
serviceCollection.AddTransient<NewDownloader>();
```

## 4. Add to settings

Update `SettingsService` to allow the user to enable/disable the new source.
