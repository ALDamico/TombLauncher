# Gamepad Support

**by Tomb Launcher Developers**  
*Last Edited: 19 May 2026*

---

## Which game engines natively support gamepads?

The following engines have built-in gamepad support and do not require any additional tools:

- **TEN** (Tomb Engine Next)
- **TR1X** (open-source re-implementation of Tomb Raider 1)
- **TR2X** (open-source re-implementation of Tomb Raider 2)
- **Tomb2Main** (community Tomb Raider 2 engine)
- **Tomb Raider III Community Edition**
- **TRX** (generic open-source engine)

The following engines do **not** natively support gamepads and require AntiMicroX to use a controller:

- Tomb Raider (original)
- Tomb Raider (DOS version)
- Tomb Raider II
- Tomb Raider III
- Tomb Raider: The Last Revelation (TR4)
- Tomb Raider Chronicles (TR5)
- TombATI

## What is AntiMicroX and why does Tomb Launcher use it?

AntiMicroX is a free, open-source application that maps gamepad buttons and axes to keyboard keys and mouse controls. Because the classic Tomb Raider engines (TR1–TR5) were designed for keyboard and older gamepads only, AntiMicroX acts as a bridge, translating gamepad input into the key presses those engines understand.

Tomb Launcher integrates with AntiMicroX automatically: when you launch a game whose engine does not natively support gamepads, Tomb Launcher starts AntiMicroX with the appropriate profile in the background. When you quit the game, Tomb Launcher unloads the profile (or closes AntiMicroX if it was not already running before the game started).

## How do I enable AntiMicroX integration?

Open Tomb Launcher's Settings and navigate to the **Gamepad** section. In the **Gamepad support tool** dropdown, select **AntiMicroX**. The rest of the settings will become available.

## How do I configure the AntiMicroX executable path?

In the **Gamepad** settings section, enter the path to the AntiMicroX executable in the **Tool path** field. If AntiMicroX is installed system-wide and available on your system's PATH (e.g. installed via your Linux package manager), you can leave the default value `antimicrox` as-is. Otherwise, enter the full path to the executable (e.g. `/home/user/antimicrox/antimicrox` on Linux or `C:\Program Files\AntiMicroX\antimicrox.exe` on Windows).

The indicator next to the field turns green when the executable is found and valid, and red otherwise.

## What are gamepad profiles and how do I use them?

A gamepad profile (`.amgp` file) is a configuration file that tells AntiMicroX how to map your controller's buttons and axes to specific keyboard keys for a given game engine.

Tomb Launcher ships with bundled profiles for TR1/TR2 and TR3/TR4/TR5. These are loaded automatically when you enable AntiMicroX integration — you do not need to do anything.

If you prefer to use your own custom profile, open the **Gamepad** settings section, find the engine you want to customise, and click the folder icon next to its profile path to select your `.amgp` file. To revert to the bundled profile, click the reset icon.

## Can I use AntiMicroX with my own custom profile for all engines?

Yes. In the **Gamepad** settings section, each engine that requires AntiMicroX has its own profile entry. You can independently set a custom profile for each engine. Engines with native gamepad support are not listed, as they do not need AntiMicroX.

## Does Tomb Launcher bundle AntiMicroX?

No, you need to install it separately.

## What are the limitations of the bundled gamepad profiles?

The bundled profiles aim to replicate the original PlayStation controls as faithfully as possible, but there are some known limitations:

- **Exiting the pause menu (TR4 and TR5 only):** On PSX, pressing Start exits the pause menu. On PC, this is done with Escape, which is mapped to the Select button. As a result, you must press Select (not Start) to exit the pause menu in TR4 and TR5.

- **Going back in menus (all profiles):** On PSX, pressing Triangle (Y on Xbox-style controllers) navigates back in menus. On PC this is done with Escape, so you must use Select to go back in menus rather than Y/Triangle.

- **Side steps and flares in Tomb Raider 2:** AntiMicroX does not support mapping a combination of controller buttons (e.g. R1 + D-pad Left/Right) to a single keyboard key. On PSX, TR2 uses R1 + D-pad Left/Right for side steps, while on PC these are Page Up and Page Down. Because this combination cannot be replicated, the TR1/TR2 bundled profile uses the same control scheme as TR1 for TR2 as well. As a side effect, it is not possible to light flares with L2 in Tomb Raider 2 using the bundled profile. You need to light them through the inventory.
