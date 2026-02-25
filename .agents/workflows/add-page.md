---
description: How to add a new page (View + ViewModel + Service)
---

# Adding a new page

When creating a new page in the application, follow these steps:

## 1. Create the ViewModel

Create `TombLauncher/ViewModels/Pages/NewPageViewModel.cs`:
- Extend `PageViewModel`
- Inject the corresponding service via constructor
- Override `OnNavigatedTo(object parameter)` to load data
- Use `SetBusy()` / `ClearBusy()` for loading state
- Use `[ObservableProperty]` for reactive properties
- Use `RelayCommand` / `AsyncRelayCommand` for commands

## 2. Create the Service

Create `TombLauncher/Services/NewPageService.cs`:
- Inject `NavigationManager`, `NotificationService`, and the required `UnitOfWork` classes
- Contain all business logic (the ViewModel only orchestrates)

## 3. Create the View

Create the file pair:
- `TombLauncher/Views/Pages/NewPagePage.axaml` (note the **Page** suffix, not View)
- `TombLauncher/Views/Pages/NewPagePage.axaml.cs` (empty code-behind)

> **IMPORTANT**: The `ViewLocator` automatically resolves Views by replacing "ViewModel" with "Page" in the type name. So `NewPageViewModel` → looks for `NewPagePage`.

## 4. Register in DI Container

In `App.axaml.cs`:
- Add the service in `ConfigurePageServices()`:
  ```csharp
  serviceCollection.AddScoped<NewPageService>();
  ```
- Add the ViewModel in `ConfigureViewModels()`:
  ```csharp
  serviceCollection.AddScoped<NewPageViewModel>();
  ```

## 5. Localization

Add all user-facing strings to:
- `TombLauncher.Localization/Localization/en-US.axaml`
- `TombLauncher.Localization/Localization/it-IT.axaml`

Use `"LocalizationKey".GetLocalizedString()` in C# code.

## 6. Navigation

To navigate to the page, use the `NavigationManager`:
```csharp
await navigationManager.NavigateTo<NewPageViewModel>(optionalParameter);
```
