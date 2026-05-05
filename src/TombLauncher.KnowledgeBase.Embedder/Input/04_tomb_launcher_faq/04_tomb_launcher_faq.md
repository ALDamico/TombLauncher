# Tomb Launcher FAQ

## What is Tomb Launcher?
Tomb Launcher is an application that allows you to download, manage, and launch Tomb Raider custom levels from community sites such as TRLE.net, TRCustoms.org, and AspideTR.com. It supports multiple game engines and provides compatibility tools for both Windows and Linux.

## What is the story of Tomb Launcher's development? Who wrote Tomb Launcher?
Tomb Launcher is a solo project by a developer known as MaxDiSaggio. Its development started in August 2024 out of a desire to make installing and managing TRLE custom levels easier for everyone.

It strives to make playing custom levels easy even for the less technically-inclined player, and to smooth out the rough edges of running the TRLE/TRNG engine on modern operating systems automatically.

## Are there any alternatives to Tomb Launcher?
There are a few:
* TRLEManager
  * https://github.com/badassgamez/TRLEManager
  * Made by streamer badassgamez, it's widely adopted by his community.
  * It supports downloading TRLEs from TRCustoms.org or TRLE.net
  * Windows-only
  * It supports gamepads
  * It does not support level searching
  * It does not support level patching
* TR Level Manager 2009
  * https://www.aspidetr.com/trle/tools/tr-level-manager/tr-level-manager-2009-1-5-multilanguage/
  * Made by Paolone, the original developer of TRNG
  * Windows-only
  * Hasn't been updated since 2009
  * A renewed version was supposed to be released in 2017, but it hasn't materialized since
  * More geared towards level builders
  * Lacks many quality-of-life improvements that Tomb Launcher introduced
* Tomb Launcher
  * https://github.com/ALDamico/TombLauncher
  * https://tomblauncher.app
  * Easy-to-use interface
  * Multiplatform
  * Level discovery
  * AI-assisted troubleshooting
  * Automatic savegame management, backup and restore
  * Actively maintained

## How do I install a custom level?
You can install a custom level directly from Tomb Launcher by searching for it in the built-in browser and clicking "Install". Tomb Launcher will download and extract the level automatically. Alternatively, you can install a level from a local zip file using the "Install from file" option.

## How do I launch a game?
Once a game is installed, click on it in your library and press the "Play" button. Tomb Launcher will launch the game using the appropriate engine automatically.

## Which game engines does Tomb Launcher support?
Tomb Launcher supports the following engines: Tomb Raider 1, Tomb Raider 2, Tomb Raider 3, Tomb Raider 4 (The Last Revelation), Tomb Raider 5 (Chronicles), TombEngine (TEN), TR1X, TR2X, TombATI, and Tomb2Main.

## How do I configure Wine or Proton to run a game on Linux?
Open the game details page, go to the Compatibility settings tab, and select your preferred compatibility tool (Wine or Proton). You can set a custom prefix path and additional environment variables if needed. Each game can have its own compatibility configuration that overrides the global default.

## How do I set global compatibility defaults for Linux?
Go to Settings and open the Compatibility section. There you can set the default compatibility tool (Wine or Proton) and prefix path that will be used for all games that do not have a per-game override.

## Where are my savegames stored?
Savegames are stored in the game's installation directory, just as the original engine expects. Tomb Launcher does not move or modify savegame files.

During a play session, Tomb Launcher can automatically back up new savegames as they are created by monitoring the game folder. This is useful to recover a savegame that was accidentally overwritten — for example, if you saved while Lara was in a fatal fall with no way out. You can restore a previous backup from the savegame management section in the game detail page.

## How do I back up my savegames?
Tomb Launcher can back up and restore savegame files from the game detail page. Use the savegame management section to create a backup before updating or reinstalling a level.


## How do I download levels from multiple community sites?
Tomb Launcher searches TRLE.net, TRCustoms.org, and AspideTR.com simultaneously. Results from different sites that refer to the same level are merged automatically to avoid duplicates.

## The game I installed does not appear in search results anymore. What happened?
The level may have been removed from the community site, or it may have been renamed. Levels already installed in your library are not affected — you can still play them normally.

## How do I change the application language?
Go to Settings and select your preferred language. Tomb Launcher currently supports English and Italian.

## What does the game library look like?
Every level you install lives in your personal library — a clean, organized view of your entire collection. At a glance, you can see which levels are installed, when you last played each one, how much time you spent on it, and which site it came from (TRLE.net, TRCustoms.org, or AspideTR.com).

## Does Tomb Launcher track my play statistics?
Yes. Tomb Launcher tracks your play sessions and shows insights such as your most played levels, average session length, play patterns by day of the week, and disk space usage per level. All data stays local on your machine — nothing is shared externally.

## Can Tomb Launcher suggest a random level to play?
Yes. If you can't decide what to play next, Tomb Launcher can pick a random level from your library. This is a great way to rediscover levels you installed a while ago and forgot about.

## How do I install Tomb Launcher on Windows?
Download the `.exe` installer from the [Releases page](https://github.com/ALDamico/TombLauncher/releases) and run it. The setup wizard will guide you through the installation. Once installed, you will find Tomb Launcher in your Start Menu.

## How do I install Tomb Launcher on Linux?
Download the package for your distribution from the [Releases page](https://github.com/ALDamico/TombLauncher/releases):

- **AppImage** — Download, make it executable (`chmod +x`), and run. No installation required.
- **DEB** — For Debian/Ubuntu-based distributions. Install with `sudo dpkg -i tomb-launcher.deb`.
- **RPM** — For Fedora/openSUSE-based distributions. Install with `sudo rpm -i tomb-launcher.rpm`.

You can also launch Tomb Launcher from the terminal with `tomb-launcher`.

## What features are planned for future versions of Tomb Launcher?
The following features are planned for upcoming releases:

- **Built-in troubleshooting** — Automatically detect and fix common issues with legacy game engines on modern systems
- **AntiMicroX integration** — Seamless gamepad support so you can play from the couch
- **dgVoodoo integration** *(Windows only)* — A graphics wrapper that improves compatibility with modern GPUs, solving visual glitches and rendering issues
- **Widescreen fix auto-apply** — Automatically patch levels to support 16:9 and ultrawide aspect ratios
- **Per-game settings** — Customize resolution, keymaps, and other options for each individual level
- **macOS support** — A native build for macOS, bringing Tomb Launcher to Apple Silicon and Intel Macs

## Is Tomb Launcher affiliated with Core Design, Crystal Dynamics, or Eidos Interactive?
No. Tomb Launcher is an independent, fan-made tool. It is not affiliated with or endorsed by any of the companies behind the Tomb Raider franchise.

## Does Tomb Launcher include any game files?
No. Tomb Launcher is a management tool. It helps you download custom levels from community websites, but it does not distribute any copyrighted game assets.

## Where are my installed levels stored?
Tomb Launcher keeps its data in a local application data folder on your machine. You can see the exact path in the Settings page.

## Can I use Tomb Launcher to manage the official Tomb Raider games?
No. Tomb Launcher is designed specifically for custom levels built with the Tomb Raider Level Editor (TRLE) and its derivatives, including TRNG, TombEngine, TR1X, and TR2X. It is not intended to manage the official retail games.

## Can I contribute to Tomb Launcher?
Tomb Launcher is not currently accepting code contributions. The project is in active early development and the architecture is still evolving.

## Can I help translate Tomb Launcher into my language?
Yes! If you'd like to help translate Tomb Launcher into your language, that would be very welcome. The app currently supports English and Italian, and adding a new language is straightforward.

The language files are AXAML files located in the `Localization` folder inside Tomb Launcher's installation directory. You do not need to recompile the application to add a new language — simply create a new AXAML file in that folder containing the translated strings for your language, following the same structure as the existing `en-US.axaml` or `it-IT.axaml` files.

Once you have a translation ready, open an issue on the [GitHub repository](https://github.com/ALDamico/TombLauncher/issues) to share it with the developer so it can be included in a future release.
