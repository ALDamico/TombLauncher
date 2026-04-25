Bump the TombLauncher version to $ARGUMENTS.

Update the version in exactly two files:
1. `src/TombLauncher/TombLauncher.csproj`: change `<Version>...</Version>` to `<Version>$ARGUMENTS</Version>`
2. `deploy/TombLauncher.pupnet.conf`: change the `AppVersionRelease = ...` line to `AppVersionRelease = $ARGUMENTS`

After making the changes, show me a brief diff of what changed.
