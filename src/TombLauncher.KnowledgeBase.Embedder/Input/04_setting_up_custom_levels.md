# Setting up Custom Levels (Tutorial)

**by Teone**  
*Posted: 08 Sep 2021 12:20*

---

All TRLE Custom Levels are ready-to-play. No
installation is needed. Just download a random game, unzip it in a
folder of your choice and run the main EXE.
However, for a better game experience, you must adjust the settings before playing.

In this tutorial we will learn how to modify Custom Levels Options and which are the recommended settings.

How to pop up the Game Options Window?

If your System is virgin, I mean... if you have never run a Custom Level
before, the Options Window will shows up automatically just running a
game.
Otherwise, there are 3 methods to show it up.

**Method 1: The CTRL Key** (it works only for Next Generation TR4 Custom Levels)
Simply hold the CTRL key while running the game and the Options will shows up automatically.

**Method 2: The Shortcut Link** (it works for TR2, TR3, TR4 and TR5 types)
Create a shortcut to the game EXE in your Desktop.
Don't run it yet but right click the shortcut instead and select Properties.
Add the command line -setup at the end of the EXE path (Example: "C:\My Folder\My Game\Tomb4.exe" -setup).
Apply changes and run the shortcut.

**Method 3: The Batch File** (it works for TR2, TR3, TR4 and TR5 types)
In the following example we assume to create a batch file for TR4.
Open Notepad and write: "Start Tomb4.exe -setup" (without quotes!).
Save the file as batch file (Example: Setup.bat).
Place the batch file in the game folder and run it.
NOTE: Obviously, for Non-TR4 games, just write the appropriate EXE name!

IMPORTANT NOTE ABOUT TR1 CUSTOM LEVELS:
TR1 is a world apart. These type of Custom Levels always came with an
Emulator (DOSBox or Tombati). To modify the options for TR1 Levels you
should modify the settings of the Emulator itself, and I don't recommend
to do this, unless you are an advanced user and you know what you are
doing.

Which are the recommended settings?

Leaving TR1 alone, lets proceed in order, starting from...

TR2 Settings

**[Screenshot – Tomb Raider II, configurazione grafica]**
Scheda Graphics: Hardware 3D Acceleration selezionata, modalità Full Screen, risoluzione 1920×1080 True Color.

TR2 Options are divided in 5 Tabs.

**Options ->**
In the first Tab you can only see a summary of the chosen options

**Graphics** (It's the Tab shown in the image above)
Select Hardware 3D Acceleration. Let the default Graphic Options as
shown in the image and choose if you want to play in fullscreen or
windowed mode.
In fullscreen mode, I recommend to select the highest resolution you find in the list.
In windowed mode, choose the window dimension you like more.

**Sound**
Here you can only enable/disable sound, of course you have to enable it.

**Controls**
Here you can add a Joystick, if you wish.

**Advanced**
Here you can set some additional option, but I recommend to let them all
disabled and set Texel adjustment option to "Always adjust"

---

TR3 Settings

**[Screenshot – Tomb Raider III Setup, configurazione grafica]**
Output Settings: Direct3D HAL, risoluzione 1920×1080 True Colour (32 Bit), Hardware Acceleration attiva, Bilinear Filter e Dither abilitati.

The important thing is to set Microsoft Direct3D Hardware acceleration
in Output Settings and choose Hardware Acceleration instead of Software
Mode.
Then just select the highest resolution you find in the list and let the other options as shown in the image.
And... No, you can't play in windowed mode. Deal with it!

(Actually you could use some thirdy part software to force the windowed
mode in TR3 but this goes beyond the scope of this tutorial)

---

TR4 Settings

**[Screenshot – Tomb Raider: The Last Revelation, configurazione grafica]**
Graphics Adapter: Intel HD Graphics 620; Direct3D HAL; Output Resolution: 1920×1080 32 Bit; Hardware Acceleration, Bilinear Filtering e Bump Mapping attivi; Emergency Settings: Disabled.

**Graphic Adapter**. It's your Graphic Card, so leave it there.

**Output Settings**. This is very important! Be sure to set "Microsoft Direct3D Hardware acceleration..."

**Output Resolution**. If you want to play in fullscreen mode use the highest resolution available.
If you want to play in windowed mode instead it's better to choose a slightly lower resolution.

**Texture Bit Depth**. Use the highest.

**Render Options**. The most important thing is to choose Hardware Acceleration and NOT Software Mode. This is categoric.
Then enable Bilinear Filter and Bump Mapping and let the other options disabled.

Note 1: The **Volumetric FX** option was used by old Custom Levels, before the NGLE Era, mostly for distance fog effect.
Newer Custom Levels don't care of this option anymore because certain parameters are automatically controlled by plugins.
However, if you are going to play an old Level, always take a look to
the readme file included, because builders used to advise players to
turn this option ON or OFF, for a better game experience.

Note 2: About the **No FMV**
option, if checked (as you can easily imagine) all the in-game videos
will be skipped, so I recommend to let it unchecked, unless you have a
good reason.

Note 3: The **Widescreen** option

**[Screenshot – Tomb Raider: The Last Revelation, configurazione grafica]**
Stessa schermata precedente, con la checkbox **Widescreen** evidenziata in rosso: va spuntata per il corretto rapporto d'aspetto.

In some old TR4 Custom Levels (before the NGLE Era) you could also find an option called "Widescreen".
Newer Custom Levels don't need it anymore because the screen ratio is automatically detected by plugins.
However, if you are playing an old Level in fullscreen mode and you find this option you probably want to enable it.
All modern monitors are widescreen (16:9 aspect ratio) then, if you
don't enable this option, the game would appear horizontal stretched.
If you are playing in windowed mode instead, you can let this option
disabled BUT you must choose a resolution which is 4:3 in aspect ratio
(800x600, 1024x768, and so on...).

---

TR5 Settings

It's almost identical to TR4 Settings, so follow the tips above

---

Where are the settings stored?

TR2, TR4 and TR5 Custom Levels use Windows Registry to store their
settings. For this reason, after you save the settings the first time,
you don't have to do it again when you play another game of the same
type.
TR1 and TR3 Custom Levels save their settings in the game folder
instead. This means that settings are not shared between Custom Levels
of this type.

More in details...

TR1. In Game folder (The file SETTINGS.DAT or ATISET.DAT, depending on the Emulator you are using)
TR2. In Registry [HKCU\SOFTWARE\Core Design\Tomb Raider II]
TR3. In Game folder (the file config.txt)
TR4. In Registry [HKCU\SOFTWARE\Core Design\Tomb Raider Level Editor]
TR5. In Registry [HKCU\SOFTWARE\Core Design\Tomb Raider Chronicles]

-----

HAPPY RAIDING!!!
