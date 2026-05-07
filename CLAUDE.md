# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tooling constraints

- The `gh` CLI is available and can be used for GitHub operations (creating issues, PRs, etc.).

## Code review

When asked to do a review without further context, review the **uncommitted local changes** (`git diff` / `git diff --staged`). Use `git diff <base>...HEAD` only if the user explicitly asks to review a branch or a specific range of commits.

## Collaboration style

The primary role of the agent in this project is **code reviewer and rubber duck**: discuss design choices, raise issues, flag risks, reason through problems together with the developer.

Direct code interventions are limited to **repetitive or tedious tasks** (renaming, formatting, adding boilerplate, mechanical refactors, git operations). The developer writes the actual logic and makes all architectural decisions.

When in doubt, discuss first and act only when explicitly asked.

## Commands

```bash
# Build
dotnet build TombLauncher.slnx

# Run
dotnet run --project src/TombLauncher

# Test
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~TombLauncher.Tests.ClassName"
```

No linting/formatting tooling is configured. The project uses Rider/ReSharper conventions. All projects have `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

## Architecture

### Project layout

| Project | Role |
|---------|------|
| `TombLauncher` | Main WinExe — Views, ViewModels, Services, DI bootstrap |
| `TombLauncher.Contracts` | Shared enums, interfaces, and contracts (no external dependencies) |
| `TombLauncher.Controls` | Reusable Avalonia UI controls |
| `TombLauncher.Core` | Platform-agnostic business logic — DTOs, launchers, savegame parsing, installers |
| `TombLauncher.Data` | EF Core + SQLite — entities, migrations, data services |
| `TombLauncher.Localization` | AXAML resource dictionaries for en-US and it-IT |
| `TombLauncher.Tests` | xUnit tests with NSubstitute for mocking |

Dependency direction: `TombLauncher` → `Controls / Core / Data / Localization` → `Contracts`. `TombLauncher.Core` has no Avalonia dependency and can be tested in isolation.

### DI and startup

`App.axaml.cs` wires up the DI container in `InitializeServices()`. The static `Ioc.Default` (CommunityToolkit.Mvvm) is the root container. Key extension methods:

- `AddViewModels()` — registers all page ViewModels with appropriate lifetimes (singleton for long-lived pages, scoped/transient for the rest)
- `AddPageServices()` — registers application-layer services
- `AddDatabaseAccess(config, appDataDir)` — registers `TombLauncherDbContext` (SQLite), repositories, and data services
- `AddTombLauncherMappings()` — registers all manual mapper singletons
- `AddDownloaders()` — registers the three community-site downloaders

EF Core migrations run automatically at startup via `dbContext.Database.MigrateAsync()`.

### MVVM pattern

**ViewLocator** (`src/TombLauncher/ViewLocator.cs`) resolves Views from ViewModels by convention: `FooViewModel` → `FooView` (UserControl), sets `DataContext` automatically.

**ViewModelBase** → **PageViewModel** is the ViewModel hierarchy. `PageViewModel` provides:
- `OnNavigatedTo(parameter)` / `OnNavigatingFrom()` lifecycle hooks
- `BusyScope()` — disposable that sets the busy indicator while active
- `SaveCmd` / `CancelCmd` base commands

Properties use CommunityToolkit.Mvvm source generators:
```csharp
[ObservableProperty] private string _title;
// Generates: public string Title { get => ...; set => SetProperty(...); }
```

### Navigation

`NavigationManager` (`src/TombLauncher/Services/NavigationManager.cs`) manages a `ConcurrentStack<INavigableViewModel>` history. Navigation is serialized with a `SemaphoreSlim`. Key methods:
- `NavigateTo<TViewModel>(parameter)` — push onto history, call lifecycle hooks
- `NavigateToRoot<TViewModel>(parameter)` — clear history first (used by the main sidebar menu)
- `GoBack()` — pop the stack

### Game launching

`IGameLauncher` (in `TombLauncher.Core`) has a single method:
```csharp
ProcessStartInfo GetLaunchStartInfo(GameLaunchContext context);
```

`GameLaunchContext` encapsulates `ExecutableFileName`, `Arguments`, `WorkingDirectory`, `PrefixPath`, and `ExtraEnvVars`. Implementations:
- `WindowsGameLauncher` — direct execution
- `WineGameLauncher` — wraps with `bash -c "wine ...; wineserver -w"` so process tracking waits for the Windows process to exit
- `ProtonGameLauncher` — sets all required `STEAM_COMPAT_*` env vars, strips NVIDIA PRIME vars (`__NV_PRIME_RENDER_OFFLOAD`, `__GLX_VENDOR_LIBRARY_NAME`, `__VK_LAYER_NV_optimus`) that cause a black screen when inherited, uses `bash -lc` (login shell) to pick up user env

`GameWithStatsService` selects the launcher based on the per-game `CompatibilityTool` (falling back to the global setting) and builds a `GameLaunchContext`.

### Database

`TombLauncherDbContext` (`src/TombLauncher.Data/Database/TombLauncherDbContext.cs`) has these `DbSet`s: `Games`, `PlaySession`, `GameLink`, `GameHashes`, `SavegameMetadata`, `FileBackups`, `AppCrashes`, `GameEnvironmentVariables`.

Data access goes through scoped service classes (`GameDataService`, `PlaySessionDataService`, etc.) rather than direct repository access from ViewModels.

### Mapping

Manual mapper classes (`XMapper`) registered as DI singletons via `AddTombLauncherMappings()`. Split by layer:

- `src/TombLauncher/Mappers/` — DTO↔ViewModel mappers (e.g. `GameMetadataMapper`, `SearchMapper`)
- `src/TombLauncher.Data/Mapping/` — entity↔DTO mappers (e.g. `GameMapper`, `FileBackupMapper`)

Methods follow explicit naming: `ToDto`, `ToViewModel`, `ToViewModels`, `ToObservableCollection`. When a mapping requires a service (e.g. `GameWithStatsViewModel` needs `GameWithStatsService`), the service is passed as a method parameter rather than injected into the mapper.

### Multi-source downloader

Three downloaders (TRLE.net, TRCustoms.org, AspideTR.com) implement `GameDownloaderBase`, which exposes `IGameSearchProvider`, `IGameDetailProvider`, and `IGameInstaller` as sub-interfaces via composition. `GameDownloadManager` routes requests across all active downloaders. `TombLauncherGameMerger` deduplicates results using `GameSearchResultMetadataDistanceCalculator` configured with `UseAuthor = true, IgnoreSubTitle = true`.

### Configuration

`LayeredAppConfiguration` merges three JSON layers: `appsettings.json` (defaults, committed), `appsettings.Development.json` (dev overrides), and `appsettings.user.json` in the OS app-data folder (user overrides, not committed). Access settings via `ISettingsProvider` or typed config sections from `ILayeredAppConfiguration`.

### Localization

Call `"STRING_KEY".GetLocalizedString()` (extension method). AXAML resource dictionaries are in `src/TombLauncher.Localization/Localization/`. Add new keys to both `en-US.axaml` and `it-IT.axaml`. The app auto-detects system language; the user can override in settings.

### Icons

UI icons use `PackIconRemixIconKind` (Material Design Icons / Remix Icon set). Pass enum values via `{x:Static}` in AXAML for `ConverterParameter`.
