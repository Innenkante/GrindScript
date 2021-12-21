Rem Copy the required files

Rem Set your paths here
Rem The first path should point to the build directory (don't modifiy it unless you modify all .csproj too)
Rem The second path points to your game directory
set solution_dir=%1
set build_dir=%solution_dir%build\x86\Debug\net472\
set sog_dir="C:\Program Files (x86)\Steam\steamapps\common\SecretsOfGrindea"

echo Build directory is %build_dir%

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