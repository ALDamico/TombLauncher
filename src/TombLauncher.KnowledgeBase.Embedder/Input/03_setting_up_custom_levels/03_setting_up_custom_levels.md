# Setting up Custom Levels (Tutorial)

**by Teone**  
*Posted: 08 Sep 2021 12:20*

---

## How do I install TRLE custom levels?

All TRLE Custom Levels are ready-to-play. No installation is needed. Just download a random game, unzip it in a folder of your choice and run the main EXE.  
Technically, you don't even need Tomb Launcher to play them: what Tomb Launcher calls "installation" is really just decompressing the game in its own data folder and keeping them organized.  
However, for a better game experience, you must adjust the settings before playing.

## How to pop up the Game Options Window for Tomb Raider?
The original Tomb Raider did not have an options menu to speak of.

Future versions of Tomb Launcher will support migrating TR1 levels to the TR1X engine, but as of version 1.1.0, this is not yet supported.

## How to pop up the Game Options Window for Tomb Raider 2?
You can show the game options window directly via Tomb Launcher by the game details page -> cog button -> Configure.

Alternatively, create a shortcut to the game EXE in your Desktop.
Don't run it yet but right click the shortcut instead and select Properties.
Add the command line -setup at the end of the EXE path (Example: "C:\My Folder\My Game\Tomb4.exe" -setup).
Apply changes and run the shortcut.

Lastly, you can create a batch file to launch the options window directly.
In the following example we assume to create a batch file for TR4.
Open Notepad and write: "Start Tomb4.exe -setup" (without quotes!).
Save the file as batch file (Example: Setup.bat).
Place the batch file in the game folder and run it.
NOTE: Obviously, for Non-TR4 games, just write the appropriate EXE name!


## How to pop up the Game Options Window for Tomb Raider 3?
You can show the game options window directly via Tomb Launcher by the game details page -> cog button -> Configure.

Alternatively, create a shortcut to the game EXE in your Desktop.
Don't run it yet but right click the shortcut instead and select Properties.
Add the command line -setup at the end of the EXE path (Example: "C:\My Folder\My Game\Tomb4.exe" -setup).
Apply changes and run the shortcut.

Lastly, you can create a batch file to launch the options window directly.
In the following example we assume to create a batch file for TR4.
Open Notepad and write: "Start Tomb4.exe -setup" (without quotes!).
Save the file as batch file (Example: Setup.bat).
Place the batch file in the game folder and run it.
NOTE: Obviously, for Non-TR4 games, just write the appropriate EXE name!

## How to pop up the Game Options Window for Tomb Raider 4?
You can show the game options window directly via Tomb Launcher by the game details page -> cog button -> Configure.

Alternatively, create a shortcut to the game EXE in your Desktop.
Don't run it yet but right click the shortcut instead and select Properties.
Add the command line -setup at the end of the EXE path (Example: "C:\My Folder\My Game\Tomb4.exe" -setup).
Apply changes and run the shortcut.

Lastly, you can create a batch file to launch the options window directly.
In the following example we assume to create a batch file for TR4.
Open Notepad and write: "Start Tomb4.exe -setup" (without quotes!).
Save the file as batch file (Example: Setup.bat).
Place the batch file in the game folder and run it.
NOTE: Obviously, for Non-TR4 games, just write the appropriate EXE name!

## How to pop up the Game Options Window for Tomb Raider 5?
You can show the game options window directly via Tomb Launcher by the game details page -> cog button -> Configure.

Alternatively, create a shortcut to the game EXE in your Desktop.
Don't run it yet but right click the shortcut instead and select Properties.
Add the command line -setup at the end of the EXE path (Example: "C:\My Folder\My Game\Tomb4.exe" -setup).
Apply changes and run the shortcut.

Lastly, you can create a batch file to launch the options window directly.
In the following example we assume to create a batch file for TR4.
Open Notepad and write: "Start Tomb4.exe -setup" (without quotes!).
Save the file as batch file (Example: Setup.bat).
Place the batch file in the game folder and run it.
NOTE: Obviously, for Non-TR4 games, just write the appropriate EXE name!

## How to pop up the Game Options Window for TRX?
TRX versions prior to 4.13 shipped with an external program to configure their options.  
You can launch this configuration app directly via Tomb Launcher by the game details page -> cog button -> Configure.

## How to pop up the Game Options Window for TRX?
TRX versions starting from 4.13 allow you to customize their settings directly inside the game. No external programs needed!

## What are the recommended settings for Tomb Raider 2?
TR2 Options are divided in 5 Tabs.

### Options
This tab only shows a summary of the current configuration. There are no settings to edit here.

### Graphics settings
Graphics Card: Primary Video Driver or dgVoodoo2-specific graphics adapter

Graphics Output Method: Hardware 3D Acceleration

Graphics options:
 - Z Buffer: CHECKED
 - Dither: CHECKED
 - Bilinear Filter: CHECKED - Uncheck if you prefer a more-pixellated look, or if the level author aims for a pixellated aesthetic
 - Perspective Correct: CHECKED
 - Triple Buffer: UNCHECKED - Enabling this introduces slight input lag (most of the time unnoticeable)

### Sound settings
This allows you to enable or disable sound. You generally need to leave these as they are.

### Controls

This tab allows you to add a joystick. However, these older games may not play very well with newer input devices.

Future versions of Tomb Launcher will support improved controller integration through AntiMicroX, but as of version 1.1.0 this is not yet implemented.

### Advanced
You rarely need to edit this page.

Various options:
 - Disable 16 bit textures: UNCHECKED
 - Don't sort transparent polys: UNCHECKED
 - Disable FMV: UNCHECKED

Texel adjustment: Always Adjust, OR Adjust when bilinear filtering if not available

## What are the recommended settings for Tomb Raider 3?

Graphics Adapter: Primary video driver or dgVoodoo2-specific graphics adapter

Output Settings: Microsoft Direct3D Hardware acceleration through Direct3D HAL

Resolution: 1920×1080 True Colour (32 Bit)

Software Mode: UNCHECKED

Hardware Acceleration: CHECKED

Dither: CHECKED

ZBuffer: CHECKED

Bilinear Filter: CHECKED

8 Bit Textures: UNCHECKED - This option will most likely be disabled anyway

Use AGP Mem: UNCHECKED - This option will most likely be disabled anyway

Sound Adapter: Leave it as is

Joystick Adapter: these older games may not play very well with newer input devices.

Future versions of Tomb Launcher will support improved controller integration through AntiMicroX, but as of version 1.1.0 this is not yet implemented.

Tomb Raider 3 does not allow you to play windowed natively.

## What are the recommended settings for Tomb Raider 4?

Graphics Adapter: Your system's graphics card, dgVoodoo2-specific graphics adapter

Output Settings: Microsoft Direct3D Hardware acceleration through Direct3D HAL

Output Resolution:
 - 1920x1080 32 Bit
 - Windowed: OPTIONAL

Render Options:
 - Hardware Acceleration: CHECKED
 - Software Mode: UNCHECKED
 - Volumetric FX: UNCHECKED - In recent NVidia hardware, checking this causes strange fogging artifacts
 - Bilinear Filtering: CHECKED
 - Bump Mapping: CHECKED
 - Low Resolution Textures: UNCHECKED
 - Low Resolution Bump Maps: UNCHECKED - this option will most likely be disabled anyway
 - No FMW: UNCHECKED

Sound device:
Leave as-is.

Emergency Settings:
 - Disabled: CHECKED
 - Soft Full Screen: UNCHECKED - In case of FMV videos or images issues, the real solution is using the dgVoodoo2 wrapper
 - No waiting for refresh: UNCHECKED - In case of slow-downs or flickering pictures, the real solution is using the dgVoodoo2 wrapper

## What are the recommended settings for Tomb Raider 5?

Graphics Adapter: Your system's graphics card, dgVoodoo2-specific graphics adapter

Output Settings: Microsoft Direct3D Hardware acceleration through Direct3D HAL

Output Resolution:
 - 1920x1080 32 Bit
 - Windowed: OPTIONAL

Render Options:
 - Hardware Acceleration: CHECKED
 - Software Mode: UNCHECKED
 - Volumetric FX: UNCHECKED - In recent NVidia hardware, checking this causes strange fogging artifacts
 - Bilinear Filtering: CHECKED
 - Bump Mapping: CHECKED
 - Low Resolution Textures: UNCHECKED
 - Low Resolution Bump Maps: UNCHECKED - this option will most likely be disabled anyway
 - No FMW: UNCHECKED

Sound device:
Leave as-is.

Emergency Settings:
 - Disabled: CHECKED
 - Soft Full Screen: UNCHECKED - In case of FMV videos or images issues, the real solution is using the dgVoodoo2 wrapper
 - No waiting for refresh: UNCHECKED - In case of slow-downs or flickering pictures, the real solution is using the dgVoodoo2 wrapper

## Where are the settings stored for Tomb Raider 1?
Tomb Raider 1 saves its settings in the game folder. The file is SETTINGS.DAT or ATISET.DAT, depending on the emulator you are using. This means that settings are not shared between different TR1 custom levels.

## Where are the settings stored for Tomb Raider 2?
Tomb Raider 2 saves its settings in the Windows Registry under [HKCU\SOFTWARE\Core Design\Tomb Raider II]. After you save the settings the first time, you don't have to do it again when you play another TR2 custom level.

## Where are the settings stored for Tomb Raider 3?
Tomb Raider 3 saves its settings in the game folder in a file called config.txt. This means that settings are not shared between different TR3 custom levels.

## Where are the settings stored for Tomb Raider 4?
Tomb Raider 4 saves its settings in the Windows Registry under [HKCU\SOFTWARE\Core Design\Tomb Raider Level Editor]. After you save the settings the first time, you don't have to do it again when you play another TR4 custom level.

## Where are the settings stored for Tomb Raider 5?
Tomb Raider 5 saves its settings in the Windows Registry under [HKCU\SOFTWARE\Core Design\Tomb Raider Chronicles]. After you save the settings the first time, you don't have to do it again when you play another TR5 custom level.

