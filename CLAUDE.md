# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Important Rules

**Never commit without explicit user consent.** Do not run `git commit` or any auto-committing workflow step unless the user has explicitly authorized it for the current change.

## Commands

```bash
# Build
dotnet build TombLauncher.slnx

# Run
dotnet run --project src/TombLauncher

# Test (all)
dotnet test

# Test (single test)
dotnet test --filter "FullyQualifiedName~TestClassName"

# Format
dotnet format TombLauncher.slnx
```

## Architecture Overview

TombLauncher is a cross-platform (Windows/Linux) Avalonia UI desktop app for managing and launching custom Tomb Raider levels from community sites (TRLE.net, TRCustoms.org, AspideTR.com).

### Solution Layout

```
src/
  TombLauncher/           # Main app: DI wiring, Views, ViewModels, Services, App entry
  TombLauncher.Core/      # Platform-agnostic: navigation contracts, savegame processing, platform abstractions, DTOs
  TombLauncher.Contracts/ # Shared interfaces/enums: IGameDownloader, IGameMetadata, etc.
  TombLauncher.Controls/  # Reusable Avalonia controls (StatCard, RatingControl, ImagePicker, etc.)
  TombLauncher.Data/      # EF Core + SQLite: DbContext, Repositories, Migrations, Data Services
  TombLauncher.Localization/ # i18n: LocalizationManager + .axaml resource dictionaries (en-US, it-IT)
tests/
  TombLauncher.Tests/     # xUnit tests with NSubstitute mocking
```

**Dependency direction:** `TombLauncher` → `Controls`, `Core`, `Data`, `Localization` → `Contracts`

### MVVM + Navigation

- All page ViewModels extend `PageViewModel` (abstract), which implements `INavigationTarget` (busy state, top-bar commands) and `INavigableViewModel` (`OnNavigatedTo` / `OnNavigatingFrom` lifecycle hooks).
- `NavigationManager` holds a `ConcurrentStack<INavigableViewModel>` as the back-stack, serialized with a `SemaphoreSlim`. Resolves ViewModels directly from DI. Key methods: `NavigateTo<T>()`, `NavigateToRoot<T>()`, `GoBack()`.
- `ViewLocator` maps ViewModel types → View (AXAML) types.

### Dependency Injection

Bootstrapped in `App.axaml.cs` via `Microsoft.Extensions.DependencyInjection`, committed to `CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default`. Key registration helpers: `AddPageServices()`, `AddViewModels()`, `AddDownloaders()`, `AddDatabaseAccess()`, `AddTombLauncherMappings()`.

### Services Layer

Each page has a corresponding page service (e.g. `GameListService`, `GameDetailsService`) for business logic. All services receive a `ViewServiceContext` holding: `NavigationManager`, `ILocalizationManager`, `IPopupService`, and AutoMapper's `IMapper`.

### Data Layer

EF Core 10 + SQLite. `TombLauncherDbContext` manages: `Games`, `PlaySession`, `GameLink`, `GameHashes`, `SavegameMetadata`, `FileBackups`, `AppCrashes`. Migrations run on startup via `dbContext.Database.MigrateAsync()`. Repository pattern: `EfRepository<T>` / `IRepository<T>`. AutoMapper maps between EF entities, Core DTOs, and ViewModels.

### Downloader / Installer Pipeline

Each community site implements `IGameDownloader` (Contracts), providing `IGameSearchProvider`, `IGameDetailProvider`, and `IGameInstaller`. `GameDownloadManager` runs parallel searches via `Task.WhenAll` and deduplicates results using Levenshtein distance (`TombLauncherGameMerger` + `GameSearchResultMetadataDistanceCalculator`). Installation is handled by `TombRaiderLevelInstaller` (ZIP via SharpZipLib or configurable CLI fallback, or directory copy). Engine type is detected by `TombRaiderEngineDetector`.

### Platform Abstraction

`IPlatformSpecificFeatures` (Core) — implemented by `WindowsPlatformSpecificFeatures` and `LinuxPlatformSpecificFeatures` — provides app data directory, URL opening, and savegame paths.

### Configuration

`SettingsProvider` / `ISettingsProvider` layers defaults from `src/TombLauncher/appsettings.json` with user overrides from `appsettings.user.json` in the OS app data directory. Covers: log level, DB path, language, appearance/themes, downloader sources, savegame backup, welcome widgets, and updater app cast URL/public key.

### Localization

`LocalizationManager` loads `.axaml` `ResourceDictionary` files at runtime. Language changes apply without restart; missing keys fall back to `en-US`. Use `TranslateExtension` in XAML and `GetLocalizedString()` extension in C# code.

### Theming

`ThemeManager` applies Avalonia theme variants. Available themes include "Default" and custom named themes (e.g. "Horus", inspired by Tomb Raider 4). Applied at startup and from Settings.

### Auto-Update

`NetSparkleUpdater` checks `https://tomblauncher.app/updates/appcast.xml` with Ed25519 signature verification. Updates surface as dismissable in-app notifications.

### Packaging

- **Linux:** PupNet Deploy (`deploy/TombLauncher.pupnet.conf`) → AppImage / DEB / RPM
- **Windows:** InnoSetup (`deploy/installer-script.iss`)
- `Avalonia.Diagnostics` is only included in Debug builds (conditional in `.csproj`).
