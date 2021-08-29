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
2. Create a folder called VanillaDependencies and copy `Secrets Of Grindea.exe` in there
3. Build the solution.
   - If a project fails, try to `Clean` the project and then `Rebuild` it. You can see these options by right clicking the project in Visual Studio.
   - Mod Examples require a reference to `GrindScript.dll`. The default project searches for it in `Build/Debug/`
4. Copy the following files from `SoG-Modding/Build/Debug` to the same directory as your `Secrets of Grindea.exe` file:
   - 0Harmony.dll
   - GrindScript.dll
   - GrindScriptLauncher.exe
   - ModLauncher.exe
5. Add any desired mods (eg. `FeatureExample.dll`) to the `Mods` directory. This should be in the same directory as the `Secrets of Grindea.exe` file. (If the directory does not yet exist, create it.)
6. Add any asset folders in the `ModContent` directory (for example: `ModContent\FeatureExample\<assets>`). (NOTE: Custom context is currently broken)
7. Run ModLauncher.exe and click Load, or run GrindScriptLauncher.exe directly.

## Creating Your Own Mod

To start making your own mod, perform theses steps:

1. Open Visual Studio
2. Create a New `Project` as a `Class Library (.NET Framework)`. (To find this option more quickly, you can use the search bar or filter under C# and Windows.)
   - Naming convention: `SoG.<MOD_NAME>`.
3. Add any necessary references.
   - You'll need a reference to `SoG.GrindScript` and the `Secrets Of Grindea` executable.
   - You'll likely need references to `Microsoft.Xna.Framework` and `Microsoft.Xna.Framework.Graphics`.
4. Apply your changes to the project and then `build` it.
   - You can ignore `There was a mismatch between the processor architecture...` warning, if it appears.
5. Copy the `.dll` file for your project from the appropriate `Debug` folder, and place it in the `Mods` directory (located in the same location as your `Secrets of Grindea.exe` file).
