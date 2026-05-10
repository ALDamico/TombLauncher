# Linux Compatibility
**by Tomb Launcher Developers**  
*Last edited: 13 Apr 2026*

Tomb Launcher enables you to play TRLE custom levels easily using either Wine or Proton.

Here are some common issues you may encounter:

## Why does Wine fail to start?
If you've installed your distribution's Wine package, there is a chance it won't work with Tomb Raider custom levels. 
This is due to the fact that, more often than not, Linux distributions ship with *broken* versions of Wine. 

Wine 9 is **known not to work**, while Wine 11 is known to work.

In order to download Wine, go to https://gitlab.winehq.org/wine/wine/-/wikis/Download and follow the instructions there.

## Why do TEN levels crash on start?
TEN levels will crash on startup, showing a long list of errors related to *.fx files.

This is a known issue with Tomb Launcher, as of version 1.1.0. We cannot guarantee that TEN levels will ever be 
supported under Linux.

## Do TR1X/TR2X levels support running with the native Linux executable?
This is a feature that we will probably implement in a future version of Tomb Launcher. As of version 1.1.0, TR1X and 
TR2X levels run under Linux through Wine or Proton.

## Do TR1X/TR2X levels support running with the native Linux executable?
As of version 1.4.0, Tomb Launcher allows you to patch TR1X, TR2X and TRX levels to run natively under Linux.  
To do this, open the game details in Tomb Launcher and select the "Convert to native executable" option under the button with the nurse icon.  
When you start the process, Tomb Launcher downloads the native Linux executable from the TRX repository, backs up the original executable, and replaces it with the executable it just downloaded.  
The process is designed to be reversible.

## Can Tomb Raider 1 and Tomb Raider 2 levels be patched to run via TR1X/TR2X?
This is a planned feature. We do know that it is possible to use TR1X to play Tomb Raider 1 custom levels without having 
to use DosBox, but as of version 1.1.0 this is not supported yet.