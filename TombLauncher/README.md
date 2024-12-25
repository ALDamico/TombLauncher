# Tomb Launcher
A Tomb Raider Level Editor custom levels manager and downloader.

## Compiling
Just hit the Build button.

## Adding migrations
Make sure the dotnet-ef command-line tool is installed by running 

`dotnet tool install -g dotnet-ef`

Use the command

`dotnet ef migrations add migration_name --project TombLauncher.Data`