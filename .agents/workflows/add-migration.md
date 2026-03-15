---
description: How to add an EF Core database migration
---

# Adding a database migration

The project uses **Entity Framework Core** with **SQLite**. The DbContext is `TombLauncherDbContext` in the `TombLauncher.Data` project.

## 1. Modify the model

Add or modify entities in `TombLauncher.Data/Models/`.

## 2. Update the DbContext (if needed)

If adding a new entity, register the `DbSet<T>` in `TombLauncher.Data/Database/TombLauncherDbContext.cs`.

## 3. Create the migration

```bash
dotnet ef migrations add MigrationName --project TombLauncher.Data --startup-project TombLauncher
```

## 4. Verification

Migrations are applied automatically at app startup via `DbContext.Database.MigrateAsync()` in `App.axaml.cs`.

## 5. Data access

- For new queries, add methods directly to the relevant `*DataService` class (e.g., `GameDataService`, `GameLinkDataService`) in `TombLauncher.Data/Database/Services/`.
- The pattern is: **`*DataService`** → accesses the **`TombLauncherDbContext`** directly via EF Core.
