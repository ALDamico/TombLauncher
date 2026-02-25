# TombLauncher – Agent Rules

## Project Architecture

| Project | Responsibility |
|---------|---------------|
| `TombLauncher` | Main Avalonia app (Views, ViewModels, Services, DI) |
| `TombLauncher.Contracts` | Interfaces, enums, shared contracts |
| `TombLauncher.Core` | DTOs, navigation, utils, platform-specific, savegames |
| `TombLauncher.Data` | EF Core + SQLite, repository, unit of work, migrations |
| `TombLauncher.Controls` | Reusable custom Avalonia controls |
| `TombLauncher.Localization` | Localization management (en-US, it-IT) |
| `TombLauncher.Tests` | xUnit tests |

### Project Dependencies (allowed direction)
```
Contracts ← Core ← Data
Contracts ← Localization
Core ← Controls
Data, Core, Controls, Localization ← TombLauncher (main)
TombLauncher ← Tests
```
**NEVER create circular dependencies** (e.g. Controls cannot depend on TombLauncher main).

---

## MVVM Pattern

- **ViewModelBase** → `ObservableObject` (CommunityToolkit.Mvvm)
- **PageViewModel** → base class for pages, implements `INavigableViewModel`, manages `IsBusy`, `SetBusy()`, `ClearBusy()`, `TopBarCommands`
- **ViewLocator** → automatically resolves: `NameViewModel` → looks for `NamePage` then `NameView` in the `TombLauncher.Views` namespace
- **Reactive properties**: use `[ObservableProperty]` with `_fieldName` backing fields
- **Commands**: use `RelayCommand`, `AsyncRelayCommand`, `RelayCommand<T>`, `AsyncRelayCommand<T>`

---

## Code Rules

### General
- **Target framework**: .NET 10
- **Nullable**: `disable` (except `TombLauncher.Tests` where it is `enable`)
- **Code language**: English (class names, methods, variables, comments)
- **UX language**: all user-facing strings must be localized (en-US + it-IT)

### ViewModels
- Business logic belongs in **Services** (e.g. `GameListService`), NOT in ViewModels
- ViewModels orchestrate: call the service, update reactive properties, manage busy state
- Data initialization goes in `OnNavigatedTo()`, not in the constructor
- Commands are initialized in `OnNavigatedTo()`

### Services  
- Inject dependencies via constructor
- Register in DI in `App.axaml.cs` (in the `Configure*()` methods)
- Lifetime: `Scoped` for page-level services, `Singleton` for app-wide services, `Transient` for stateless services

### Data Layer
- Pattern: **UnitOfWork** → **EfRepository<T>** → **DbContext**
- UnitOfWork classes extend `UnitOfWorkBase`
- Migrations are applied automatically at startup (`MigrateAsync()`)

### Localization
- ALWAYS add keys to **both** files (`en-US.axaml` and `it-IT.axaml`)
- C#: `"LocalizationKey".GetLocalizedString()`
- AXAML: `{DynamicResource LocalizationKey}`

### UI and Styling
- Icons: **Material.Icons.Avalonia** (`MaterialIconKind`)
- Custom controls: in the `TombLauncher.Controls` project
- Animated loader: `RingLoader` (custom)
- Themes: managed via `ThemeManager` (light/dark)

---

## Commits

Use **Conventional Commits**: `type(scope): description`

Types: `feat`, `fix`, `refactor`, `style`, `docs`, `test`, `chore`

---

## Build and Verification

Before every commit or PR:
1. `dotnet build TombLauncher.sln` must pass without errors
2. `dotnet test TombLauncher.Tests/TombLauncher.Tests.csproj` must pass

---

## Deployment

- **Linux**: PupNet (config in `TombLauncher.pupnet.conf`), supports AppImage/Flatpak/DEB/RPM
- **Windows**: Inno Setup (`installer-script.iss`)
- Version must be kept in sync between `TombLauncher.csproj` and `TombLauncher.pupnet.conf`
