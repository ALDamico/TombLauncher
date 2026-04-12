# Common Issues and Solutions

**by Teone**  
*Posted: 09 Sep 2021 18:09*

---

Hello Raiders!

In this topic are listed the most common technical issues you may encounter playing Custom Levels and relative solutions.
Keep in mind that some issues could evolve in future, because they
depend on the environment in which they occur. Certain issues may vanish
in a Windows Update or show up a slightly different behavior.
The thread will be updated if new issues will rise in future, but of course we all hope the list will remain short.
Feel free to point out other issues and solutions you know, if they are
"common" and if you think they can happen to the average players, just
to not weigh down the thread with specific and rare issues.

And PLEASE...

**DON'T USE THIS THREAD TO ASK HELP. IF YOU HAVE AN ISSUE AND IT'S NOT LISTED BELOW, JUST OPEN A NEW THREAD.
POST HERE ONLY IF YOU FOUND A SOLUTION WHICH COULD BE HELPFUL FOR EVERYBODY.**

---

Error running a game

Description: The game doesn't run and shows an error message.

Most common error messages are:

**Failed to setup DirectX**
If you get this error message, read [this topic](http://forum.trle.net/viewtopic.php?f=78&t=33218) and be sure to have DirectX 9 Runtimes installed.
Then follow [this tutorial](http://forum.trle.net/viewtopic.php?f=78&t=33219) and adjust the game settings as recommended.

**Error trying to load plugin...**
If you get this error message, read [this topic](http://forum.trle.net/viewtopic.php?f=78&t=33218) and install Visual C++ Redistributable Runtimes.

**Error: missing library flep.dll**
Even if the file flep.dll is actually in the game folder it doesn't load because it misses a Visual C++ DLL.
So, it's the same issue as above.

NOTE: If the game doesn't run but no message is shown, check the Windows
Task Manager. You could find a stuck process of the game in background
(If you don't know how to use the Task Manager just restart your PC and
all the stuck processes will be cleared). Then follow [this tutorial](http://forum.trle.net/viewtopic.php?f=78&t=33219) to enter the game options and adjust the settings as recommended.

---

Antivirus warnings

Description: Your Antivirus block/delete/quarantine the game.

Don't worry. It's a false positive. Follow [these tips](http://forum.trle.net/viewtopic.php?f=78&t=33085) to avoid dumb and annoying antivirus warnings.

---

Save/Load Issue

Description: Save/Load functions don't work. The game freezes trying to save or load, or it shows a black screen or just nothing is shown.

This issue occurs in certain games which use a Pix Folder to customize
menu background or diary screenshots. It depends on the NGLE DLL version
the game use.
To solve the problem you can try playing windowed. In this way everything should work fine.
if that doesn't work either, just rename/delete the Pix Folder and the
game will automatically use the traditional saving menu. However, keep
in mind that any diary function will not work anymore.

---

The Border Issue

Description: Windows forces to show a border at the top of the screen even if you are playing in fullscreen mode.

This issue occurs in modern Operating Systems (Windows 8, 8.1, 10) only
with older Custom Levels. Luckily the newer ones have not this problem
anymore.
To solve the issue just download and apply this fix:

[https://community.pcgamingwiki.com/file ... order-fix/](https://community.pcgamingwiki.com/files/file/82-tomb-raider-series-fullscreen-border-fix/)

You may want to know that this Fix doesn't patch your game. It just
writes some informations in Windows. Specifically, it creates an SDB
file in the folder "C:\Windows\apppatch\CustomSDB\" and also writes some
data in the Registry key [HKEY\_LOCAL\_MACHINE\SOFTWARE\Microsoft\Windows
NT\CurrentVersion\AppCompatFlags\Custom]

These informations are name-based instructions, in other words you
inform your system that every time you run an EXE with those specific
names, he must remove the border.
Unfortunately there are some inconveniences, for example if you decide
to play in windowed mode the border is removed anyway. Luckily the Fix
can be installed and uninstalled in a simple click, so you can apply or
remove it at convenience. Or even better, since it's a name-based fix,
you can rename the EXE to bypass it temporally.

---

The Stretched Aspect

Description: The game appears like horizontal stretched and the proportions are not good. Lara appears short and large.

This issue occurs playing old Custom Levels in widescreen monitors. The
newer Custom Levels have not this problem anymore because they
automatically detect the suitable screen resolution.
If you experience this issue in a TR4 type game, you may check if the
widescreen option is available for that game. If not available, I
recommend to patch the game using this Tool:

[https://tombraiders.net/stella/download ... creen.html](https://tombraiders.net/stella/downloads/widescreen.html)

---

The Zoomed Aspect

Description: The game appears like zoomed-in and exceedes the screen/window boders.

This issue occurs in laptops with HD resolution monitors. By default
Windows set the "Scale and Layout" option to 125% or 150%. To solve the
issue, just go in Windows Display Control Panel and set the option to
100%

Another solution is right click on the application > Properties Tab
> Compatibility > Change High DPI settings > Click on Override
High DPI scaling behavior > Choose Application

This second solution works for single game. It means that you have to repeat the setting for every custom level you play.

---

Keyboard Issues

Description: Some keyboard commands don't work correctly in-game.
Possible variant: The screen flips using certain key combinations in-game.

This issue mostly depends on your graphic card settings. Some Graphic
Cards set by default keyboard shortcuts to perform certain actions. This
could cause conflict with your game commands. If you experience this
issue, search in the Start Menu your Graphic Card Control Panel and
disable the shortcut keys.

---

Savegame Crash (I)

Description: You download a savegame from the forum but it doesn't work and it makes the game crash.
Possible variant: You download again a level you played time ago but your old savegames don't work anymore.

Assuming the savegames are not corrupted, probably they don't work
because they were created with a previous version of the game.
Unfortunately after a game revision/update the old savegames don't work
anymore.
There is no solution for this issue, I'm sorry.

---

Savegame Crash (II) - The Monkey Bug

Description: After starting a new game session, the savegames created in the previous session don't work anymore.

This issue could be the famous "Monkey Bug". It occurs only in modern
Operating Systems and only if there are monkeys in the level (Note: Actually, I also experienced this bug in a level with cats, probably because the builder used the monkeys slot for cats).
Those innocent, harmless animals who normally you avoid to kill, just
to save bullets and time. Well, believe it or not, they are guilty and
they deserve death penalty!

Why this odd bug occurs?

I'm not sure about this, but I read somewhere that monkeys position in
the level, after they are triggered, is not saved correctly because of
an engine bug. So the game doesn't know exactly where the monkeys must
be loaded. Old Operating Systems probably ignored the error without
fatal results, while in modern Systems the error always give a fatal
result.

To solve the problem you can try this solution: Start a New Game and
(after triggering the monkeys again from scratch) load one of the bugged
savegames. Sometimes it works.
If it doesn't work, you unfortunately have to restart your game from a
previous working savegame and going on from there, killing mercilessly
all the monkeys you meet, or playing the level without ever exit the
session.

---

