The folder "vanilla" is where the game assemblies should be placed, so that they can be referenced by the project.

Additionally the "vanilla" folder should contain one subfolder "xna"(`vanialla\xna`) to store the xna libraries.

List of assemblies needed in vanilla folder:
* Secrets of Grindea.exe
* Lidgren.Network.dll
* Steamworks.NET.dll

To the folder xna:

Copy all `Microsoft.Xna.Framework.*` dlls into `\vanilla\xna\` and the projects will pick them up. 

The assemblies(dlls) are usually located at:

`C:\Program Files (x86)\Microsoft XNA\XNA Game Studio\v4.0\References\Windows\x86\ `

If you cannot find them you might have to install the Microsoft package for XNA, which can be found here:

https://www.microsoft.com/en-us/download/details.aspx?id=20914

