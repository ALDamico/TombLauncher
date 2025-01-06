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
If this is the first time you're generating a new version, you'll need to make sure you have the `netsparkle-generate-appcast` utility installed.

To install, use the command `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`

With the tool installed, you'll need to copy the NetSparkle_Ed25519.priv and NetSparkle_Ed25519.pub files to `%HOMEPATH%\AppData\Local\netsparkle`.

> **Note**
> 
> For security reasons, these files aren't stored in the repository.

With the public/private key pair in order, you will need to edit the file `installer-script.iss` by bumping up its version number.

The TombLauncher project version number will also need to be bumped up.

Now, run the _Publish Self-Contained_ task to generate the new version.

Once done, compile the script using InnoSetup. By default, the application will generate the installer in a
directory named **Output** (it will be created if it doesn't exist).

When InnoSetup has finished generating the installer, use the following command to generate the appcast:

`netsparkle-generate-appcast -b .\Output\ -p .\TombLauncher\Data -u "YOUR_APPCASTS_PATH" -l "YOUR_CHANGELOGS_PATH" -a "YOUR_DESTINATION_APPCAST_PATH" -n "Tomb Launcher"`