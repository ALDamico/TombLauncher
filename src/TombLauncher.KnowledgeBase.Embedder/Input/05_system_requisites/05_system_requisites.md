# System Requisites for playing Custom Levels

**by Teone**  
*Posted: 08 Sep 2021 12:13*

---

This tutorial is for newbies who want to start
playing Custom Levels, but also for veteran players who don't remember
how to set up the Operating System again after buying a new PC or after
making a fresh installation of Windows.
Here is what your system needs in order to play Custom Levels.

DirectX 9 Runtimes

You need DirectX 9 to play most of the Custom Levels, except for those
built with TombEngine (TEN) that require newer versions of DirectX.

**IMPORTANT UPDATE**:
Starting from 2022 (more or less) some new graphic cards dropped
DirectX 9 support and switched to DirectX 12 Emulation, regardless if
you have DirectX 9 Runtimes installed or not. According to most players
this emulation makes the levels unplayable and the only possible
solution, for now, is to use an alternative tool called dgVoodoo2. Read [this guide](http://forum.trle.net/viewtopic.php?f=78&t=34478) if your PC doesn't support DirectX 9 natively and you need to use dgVoodoo2.

However, if your PC still supports DirectX 9, here is the download link:

[https://www.microsoft.com/en-us/downloa ... px?id=8109](https://www.microsoft.com/en-us/download/details.aspx?id=8109)

**Installation Notes:**
the application you download (directx\_Jun2010\_redist.exe) is a self
extracting package. When you launch it you'll be prompted to type/browse
a location where you want to place the extracted files. Choose an empty
folder or create a new one. Extract all files there, then go in that
folder and find the file DXSETUP.exe and run it. When the setup has
finished, you can delete all the files.

Visual C++ Redistributable Runtimes

Microsoft download page: [https://learn.microsoft.com/en-us/cpp/w ... w=msvc-170](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170)

Alternatively, you can download the All-in-One Package from here: [https://www.techpowerup.com/download/vi ... ll-in-one/](https://www.techpowerup.com/download/visual-c-redistributable-runtime-package-all-in-one/)

Older Custom Levels actually don't need VC++ runtimes, but Next
Generation (NGLE) Custom Levels often use Plugins, and these Plugins are
mostly built using Visual Studio. For this reason they need the related
DLL package to load properly.

FAQ: Which version? There are many! And should I install the x86 or the x64 package?

In my opinion it's a good idea to install them all. They will not harm
your system. They just add some DLLs in your system folder, and these
DLLs stay there quietly waiting for some application calling them. Also
consider that these files are used by a lot of programs, not only Custom
Levels. Having them all installed, you can avoid many application
errors.

However, if you really want to install them selectively, in my
experience I noticed that most NGLE Plugins are built using Visual
Studio 2010. And since Custom Levels still work in 32 bit, the required
package is the x86 one, even if your OS uses a 64 bit technology.
Indeed, in most cases the package which solve the DLL errors is
"vcredist2010x86", but this may change in future if NGLE Plugins will be
updated.

Video Codecs

<http://www.codecguide.com/download_kl.htm>

Some Custom Levels also have FMV Videos. For this reason I recommend to
install K-Lite Codec Pack. The Standard version is a good choice for
average users. It contains all the codecs you need to display any kind
of video format. In this way you should avoid possible video issues in
certain Custom Levels.

-----

HAPPY RAIDING!!!
