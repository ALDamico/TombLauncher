---
description: How to create deployment packages for Linux and Windows
---

# Deploy

## Linux (PupNet)

Configuration is in `TombLauncher.pupnet.conf`. Supports AppImage, Flatpak, DEB and RPM.

1. Install PupNet Deploy (if not already installed):
```bash
dotnet tool install -g KuiperZone.PupNet
```

2. Create an AppImage package:
```bash
pupnet -k AppImage -c Release
```

3. Output will be in the `Output/` directory.

## Windows (Inno Setup)

The Windows installer script is `installer-script.iss`. Requires Inno Setup installed on Windows.

## Version

The version is defined in two places (keep them in sync):
- `TombLauncher/TombLauncher.csproj` → `<Version>`
- `TombLauncher.pupnet.conf` → `AppVersionRelease`
