# Tomb Launcher
A Tomb Raider Level Editor custom levels manager and downloader.

## Compiling
Just hit the Build button.

## Adding migrations
Use the command

`dotnet ef migrations add migration_name --project TombLauncher.Data`

You can use the -v or --verbose switch to get a more comprehensive output in case you're encountering issues while 
creating a migration.

## Releasing a new version

The release process is fully automated via GitHub Actions and triggers on merge of a `release/x.y.z` branch into `master`.

1. **Bump the version** using the `/bump-version` slash command in Claude Code:
   ```
   /bump-version 1.0.3
   ```
   This updates `TombLauncher.csproj` and `deploy/TombLauncher.pupnet.conf` in one step.

2. **Commit and push** the version bump:
   ```bash
   git add src/TombLauncher/TombLauncher.csproj deploy/TombLauncher.pupnet.conf
   git commit -m "chore: bump version to 1.0.3"
   git push
   ```

3. **Create and push the release branch**:
   ```bash
   git checkout -b release/1.0.3
   git push origin release/1.0.3
   ```

4. **Open a PR** from `release/1.0.3` → `master`. CI (build + test) runs automatically on the PR.

5. **Merge the PR**. GitHub Actions then automatically:
   - Builds the Windows installer (Inno Setup) and Linux packages (AppImage, DEB, RPM)
   - Generates release notes from conventional commits since the previous tag
   - Creates the git tag `v1.0.3` and publishes a GitHub Release with all packages
   - Signs and deploys the updated `appcast.xml` to `tomblauncher.app` for in-app updates

> **Note:** The CI validates that the branch version matches `TombLauncher.csproj`. If they differ, the pipeline fails — run `/bump-version` before creating the release branch.