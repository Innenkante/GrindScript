# GrindScript

A project dedicated to the modding of the game [Secrets of Grindea](http://www.secretsofgrindea.com).

# Getting Started

## Installing GrindScript

To install GrindScript, download the latest release and unpack the archive's contents inside Secrets of Grindea.exe's folder.

From there, you can launch modded game instances using GrindScriptLauncher.exe, and vanilla instances using Secrets of Grindea.exe.

The modded game's save files can be found in `%appdata%/GrindScript`. You will have to transfer any vanilla saves that you want to use in that folder.

## Compiling GrindScript

You will need Visual Studio with .NET Desktop Development for this:

1. Clone this repo somewhere on your machine.
2. Copy all vanilla dependencies listed [here](vanilla/README.md) inside `vanilla/` folder.
3. Copy all xna dependencies into `vanilla\xna`, instructions can be found [here](vanilla/README.md).
4. Adjust the path to the game folder in `copy_files_to_steam_dir.bat` so all resulting assemblies are copied directly into the game folder
5. Build the solution.
5. Copy any mods you have to the `Mods/` folder. If the directory does not exist yet, create it.
6. For each mod added, copy its asset folder inside the `Content/ModContent/` folder. If ModContent does not exist yet, create it.
7. Start a modded game instance using GrindScriptLauncher.exe.

## Creating mods

You will need Visual Studio .NET Desktop Development for this:

1. Create a New Project as a Class Library (.NET Framework).
2. Add any necessary references:
   - GrindScript.dll, for the modding API
   - Addon assemblies, if you use those (such as ModGoodies.dll)
   - Secrets of Grindea.exe, for the game's API
   - Lidgren.Network.dll, for networking
   - Steamworks.NET.dll, if you plan on working with that
   - References to XNA 4.0 assemblies
3. Apply your changes to the project and build it.
4. Copy the mod assembly to the game path's `Mods/` folder.
5. If you mod uses external assemblies like `ModGoodies.dll` make sure to copy them into the `Mods/` folder too.

