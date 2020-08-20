# SoG-Modding

A project dedicated to the modding of the game [Secrets of Grindea](http://www.secretsofgrindea.com).

To know:

- Any prealpha releases will be posted in the "Release" section on github
- If you have any issues, open up an issue on github
- Enjoy and have fun

### The current state is extremly pre-alpha, so don't expect anything groundbreaking

Disclaimer: The master branch can be broken sometimes.

# Getting Started

To get the mod loader running on your local machine, perform these steps:

1. Clone this repo and open the solution in Visual Studio.
   - You might be asked to install extensions for C++ development in Visual Studio.
2. Build the solution.
   - Build the `SoG.ModLoader` project separately if needed.
   - If a project fails, try to `Clean` the project and then `Rebuild` it. You can see these options by right clicking the project in Visual Studio.
3. Copy the following files from `SoG-Modding/Build/Debug` to the same directory as your `Secrets of Grindea.exe` file:
   - 0Harmony.dll
   - GrindScript.dll
   - ModLauncher.exe
   - ModLoader.dll
4. Add any desired mods (eg. `ChaosMod.dll`) to the `Mods` directory. This should be in the same directory as the `Secrets of Grindea.exe` file. (If the directory does not yet exist, create it.)
5. Run the game.
6. Run `ModLauncher.exe`, select `LOAD`, and enjoy your mods! 😃

## Creating Your Own Mod

To start making your own mod, perform theses steps:

1. Open Visual Studio
2. Create a New `Project` as a `Class Library (.NET Framework)`. (To find this option more quickly, you can use the search bar or filter under C# and Windows.)
   - Naming convention: `SoG.<MOD_NAME>`.
3. Add any necessary references.
   - You'll need a reference to `SoG.GrindScript`.
   - You'll likely need references to `Microsoft.Xna.Framework` and `Microsoft.Xna.Framework.Graphics`.
4. Apply your changes to the project and then `build` it.
   - Removing the `There was a mismatch between the processor architecture...` warning (harmless). (UNRESOLVED)
5. Copy the `.dll` file for your project from the appropriate `Debug` folder, and place it in the `Mods` directory (located in the same location as your `Secrets of Grindea.exe` file).
