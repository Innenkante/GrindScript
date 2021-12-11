Rem Copy the required files

Rem Set your paths here
set build_dir=C:\Users\Mark\source\repos\GrindScript\build\Debug\net472\
set sog_dir=E:\Steam\steamapps\common\SecretsOfGrindea\

echo Starting copy...
xcopy %build_dir%\0Harmony.dll %sog_dir% /y /f
xcopy %build_dir%\GrindScript.dll %sog_dir% /y /f
xcopy %build_dir%\GrindScript.pdb %sog_dir% /y /f
xcopy %build_dir%\GrindScriptLauncher.exe %sog_dir% /y /f

Rem Copy the test mod
xcopy %build_dir%\TestMod.dll %sog_dir%\Mods\ /y /f /i
xcopy %build_dir%\ModGoodies.dll %sog_dir%\Mods\ /y /f /i

echo Finished copying