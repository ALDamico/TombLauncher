Remove all bin and obj folders recursively, run dotnet clean, then restore NuGet packages.

```powershell
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -Confirm:$false
dotnet clean TombLauncher.slnx
dotnet restore TombLauncher.slnx
```
