# Common Issues and Solutions

**by Teone**  
*Posted: 09 Sep 2021 18:09*

---

## Why does the level fail to start with the error message "Failed to setup DirectX"?
You need DirectX 9 to play most of the Custom Levels, except for those built with TombEngine (TEN) that require newer versions of DirectX.

IMPORTANT UPDATE: Starting from 2022 (more or less) some new graphic cards dropped DirectX 9 support and switched to DirectX 12 Emulation, regardless if you have DirectX 9 Runtimes installed or not. According to most players this emulation makes the levels unplayable and the only possible solution, for now, is to use an alternative tool called dgVoodoo2. Read this guide if your PC doesn't support DirectX 9 natively and you need to use dgVoodoo2.

However, if your PC still supports DirectX 9, here is the download link: https://www.microsoft.com/en-us/download/details.aspx?id=8109

## Why does the level fail to start with the error message "Error trying to load plugin..."?
Older Custom Levels actually don't need VC++ runtimes, but Next Generation (NGLE) Custom Levels often use Plugins, and these Plugins are mostly built using Visual Studio. For this reason they need the related DLL package to load properly.

**FAQ**: Which version? There are many! And should I install the x86 or the x64 package?

Most NGLE Plugins are built using Visual Studio 2010. And since Custom Levels still work in 32 bit, the required package is the x86 one, even if your OS uses a 64 bit technology. Indeed, in most cases the package which solve the DLL errors is "vcredist2010x86", but this may change in future if NGLE Plugins will be updated.

## Why does the level fail to start with the error message "Error: missing library flep.dll"?
Even if the file flep.dll is actually in the game folder it doesn't load because it misses a Visual C++ DLL.

Older Custom Levels actually don't need VC++ runtimes, but Next Generation (NGLE) Custom Levels often use Plugins, and these Plugins are mostly built using Visual Studio. For this reason they need the related DLL package to load properly.

**FAQ**: Which version? There are many! And should I install the x86 or the x64 package?

Most NGLE Plugins are built using Visual Studio 2010. And since Custom Levels still work in 32 bit, the required package is the x86 one, even if your OS uses a 64 bit technology. Indeed, in most cases the package which solve the DLL errors is "vcredist2010x86", but this may change in future if NGLE Plugins will be updated.

## Why does the game freeze when I try to save or load a game? Why does the game show a black screen in place of the save or load screen?
This issue occurs in certain games which use a Pix Folder to customize menu background or diary screenshots. It depends on the NGLE DLL version the game use.

To solve the problem you can try playing windowed. In this way everything should work fine.

Another alternative is to play the game through the dgVoodoo2 wrapper.

if that doesn't work either, just rename/delete the Pix Folder and the game will automatically use the traditional saving menu. However, keep in mind that any diary function will not work anymore.

## Why is the window title visible when playing in full-screen mode?

This issue occurs in modern Operating Systems (Windows 8, 8.1, 10) only
with older Custom Levels. Luckily the newer ones have not this problem
anymore.

To solve the issue just download and apply this fix: https://community.pcgamingwiki.com/files/file/82-tomb-raider-series-fullscreen-border-fix/

A future version of Tomb Launcher will allow you to fix this issue without applying that fix, but as of version 1.1.0 this is not yet available.

You may want to know that this Fix doesn't patch your game. It just writes some informations in Windows. Specifically, it creates an SDB file in the folder "C:\Windows\apppatch\CustomSDB\" and also writes some data in the Registry key [HKEY\_LOCAL\_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Custom]

These informations are name-based instructions, in other words you inform your system that every time you run an EXE with those specific names, he must remove the border.

Unfortunately there are some inconveniences, for example if you decide to play in windowed mode the border is removed anyway. Luckily the Fix can be installed and uninstalled in a simple click, so you can apply or remove it at convenience. Or even better, since it's a name-based fix, you can rename the EXE to bypass it temporally.

## Why does the game appear horizontally stretched? Why does Lara appear short and large?

This issue occurs playing old Custom Levels in widescreen monitors. The newer Custom Levels have not this problem anymore because they
automatically detect the suitable screen resolution.

If you experience this issue in a TR4 type game, you may check if the widescreen option is available for that game. If not available, I recommend to patch the game using this Tool: https://tombraiders.net/stella/downloads/widescreen.html

Note that a future version of Tomb Launcher will allow you to apply the widescreen patch without using external tools, but as of version 1.1.0 this is not yet implemented.

## Why does the game appear horizontally stretched? Why does Lara appear short and large?

This issue occurs playing old Custom Levels in widescreen monitors. The newer Custom Levels have not this problem anymore because they
automatically detect the suitable screen resolution.

If you experience this issue in a TR4 type game, you may check if the widescreen option is available for that game. If not available, you can apply the widescreen patch by opening the game details in Tomb Launcher and selecting the "Widescreen patch" option under the button with the nurse icon.

Here, you can specify a few parameters for the widescreen patch:
 * Aspect ratio width and height: these should correspond to your desired resolution's aspect ratio. For example, for 1920 x 1080 they should be 16 and 9 respectively.
 * Camera distance: use this if you feel the camera follows Lara too closely. You can choose between a few presets (from 1 block to 3 blocks) in increments of half blocks, or specify your own camera distance, expressed in in-game units.
 * FOV: use this to apply FOV correction, if you feel the field of view is too cramped.
 * 60 FPS: (Tomb Raider II and III only) use this if you want to unlock the framerate from 30 FPS to 60 FPS.

The widescreen patch in Tomb Launcher is a port of Mr. Blackfour's widescreen patch:  https://tombraiders.net/stella/downloads/widescreen.html

## Why does the game appear zoomed-in? Why does the game exceed the screen/window borders?
This issue occurs in laptops with HD resolution monitors. By default Windows set the "Scale and Layout" option to 125% or 150%. To solve the issue, just go in Windows Display Control Panel and set the option to 100%

Another solution is right click on the application > Properties Tab > Compatibility > Change High DPI settings > Click on Override High DPI scaling behavior > Choose Application

This second solution works for single game. It means that you have to repeat the setting for every custom level you play.

## Why do some keyboard commands not work correctly? Why does the screen flips using certain key combinations?
This issue mostly depends on your graphic card settings. Some Graphic Cards set by default keyboard shortcuts to perform certain actions. This could cause conflict with your game commands. If you experience this issue, search in the Start Menu your Graphic Card Control Panel and disable the shortcut keys.

## Why does the game crash when loading a savegame I downloaded from the Internet? Why do my savegames from a while ago not work anymore?
Assuming the savegames are not corrupted, probably they don't work because they were created with a previous version of the game.
Unfortunately after a game revision/update the old savegames don't work anymore.

Although some sources, like TRCustoms.org, do allow you to download previous versions of a level, Tomb Launcher always installs the latest version by default. As of version 1.1.0, this is unlikely to change in the future.

## Why don't my savegames work anymore from one play session to the next?
This issue could be the famous "Monkey Bug". It occurs only in modern Operating Systems and only if there are monkeys in the level (Note: Actually, I also experienced this bug in a level with cats, probably because the builder used the monkeys slot for cats).  
Those innocent, harmless animals who normally you avoid to kill, just to save bullets and time. Well, believe it or not, they are guilty and they deserve death penalty!

Why this odd bug occurs?

I'm not sure about this, but I read somewhere that monkeys position in the level, after they are triggered, is not saved correctly because of an engine bug. So the game doesn't know exactly where the monkeys must be loaded. Old Operating Systems probably ignored the error without fatal results, while in modern Systems the error always give a fatal result.

To solve the problem you can try this solution: Start a New Game and (after triggering the monkeys again from scratch) load one of the bugged
savegames. Sometimes it works.  
If it doesn't work, you unfortunately have to restart your game from a previous working savegame and going on from there, killing mercilessly all the monkeys you meet, or playing the level without ever exit the session.
