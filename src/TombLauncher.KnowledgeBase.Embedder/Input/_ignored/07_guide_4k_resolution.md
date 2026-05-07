# [Guide] How to play at 4k resolution

**by black horse**  
*Posted: 25 Dec 2023 23:36*

---

Originally, the TR engine allows up to 1080.
This guide will help you to reach the awesomeness of 4K, now you can enjoy every pixelated pixel in its 4K glory.
> Note: There is no gain or difference in 1080 vs 4K, graphic wise for this game.
> This helps players who have 4K resolution with scaling overcome the
> messed up layout when games switches back and forward from 1080 to 4K,
> because Microsoft still can't figure how scaling should work.

- **Contents:
  1- Required files.
  2- How to setup.
  3- Side notes.**

1-Required files:
First you need to download [Dgvoodoo](https://github.com/dege-diosg/dgVoodoo2/releases) This allow older game to utilize resolutions beyond 1080.

Alternative links:
[Ufile1](https://ufile.io/2b4lrvwg)
[Ufile2](https://ufile.io/ob4gsab6)
[Mediafire](https://www.mediafire.com/file/xk18d3e8niwqke0/dgVoodoo2_8_2.zip/file)
> Note:
> It's always better to get the latest version from the official website!
> But for whatever reason the file can no longer be reached, use the
> alternative links.

2- How to setup:
Extract the file in a separated folder, not in the game folder!
Open dgVoodooCpl.exe and setup the program as the images.

**[Screenshot – dgVoodoo 2.6.2 Control Panel, scheda General]**
La finestra mostra il percorso della cartella di configurazione (`C:\Users\<User>\AppData\Roaming\dgVoodoo`). Una freccia rossa indica la scheda **DirectX** da selezionare. Annotazione: *"This is the path where the program configuration is saved. If it doesn't exist after running the program, try running it as Admin."*

(Please ignore my winter themed windows xD)

If file path not in the user folder, the configurations might not be saved.
**C:\Users\<User>\AppData\Roaming\dgVoodoo**
Ignore this screen and click on DirectX tab.

Now set the settings as in the 2nd image.

**[Screenshot – dgVoodoo 2.6.2 Control Panel, scheda DirectX]**
Impostazioni da configurare:
- Videocard → `dgVoodoo Virtual 3D Accelerated Card`
- VRAM → `4096 MB`
- Resolution → `Desktop`
- "dgVoodoo Watermark" → deselezionato
- "Disable Alt-Enter to toggle screen state" → selezionato

**Videocard >dgVoodoo Virtual 3D accelerated.
VRAM> 4096 MB
Resolution > Desktop
dgVoodoo Watermark > Uncheck**
Then click ok.

Now in dgVoodoo folder, there's a folder called MS, open it, then open x86, and copy all the files inside.
**D3D8.dll
D3D9.dll
D3DImm.dll
DDraw.dll**

Copy these file to the Tomb Raider level folder, and past them there, where the .exe file is (tomb4.exe , tomb2.exe ..etc)
You need to copy these 4 files in every Tomb raider level you want to play at 4K.

Now run the Tomb Raider level .exe and go to setting ( Press Ctrl on launch ) and set your resolution to 1080.
And that's it, run the game and enjoy.

---

Side notes:
**A-** To uninstall the mod, just delete the 4 files you just copied and the game shall return to its original setting.
**B-**
If you notice, in Tomb Raider setting (Ctrl at launch) after copying
the 4 files, you can set the resolution to + 4K , but that might results
in slowness in menu, fmv glitches, fps drops for some reasons in some
levels, it's far better and safer to set the game resolution to 1080 and
use dgVoodoo to force the game at the desired resolution.

Nothing was made by me, all thanks goes to dgVoodoo and random posts on
reddit on how setting up old games to run at 4K, I just figured the
proper setting for Tomb raider level to run smoothly since their guide
didn't work well for me.
Man, I feel I made this more complicated than it should xD
Links / images broken, help needed, let me know.
