Rem Copy the required files

Rem Set your paths here
Rem The first path should point to the build directory (don't modifiy it unless you modify all .csproj too)
Rem The second path points to your game directory
set build_dir=..\build\Debug\net472\
set sog_dir=E:\Steam\steamapps\common\SecretsOfGrindea\

Rem Copies everything
echo Starting copy...
xcopy %build_dir%\0Harmony.dll %sog_dir% /y /f
xcopy %build_dir%\GrindScript.dll %sog_dir% /y /f
xcopy %build_dir%\GrindScript.pdb %sog_dir% /y /f
xcopy %build_dir%\GrindScriptLauncher.exe %sog_dir% /y /f

Rem Copy the test mod
xcopy %build_dir%\TestMod.dll %sog_dir%\Mods\ /y /f /i
xcopy %build_dir%\ModGoodies.dll %sog_dir%\Mods\ /y /f /i

echo Finished copying