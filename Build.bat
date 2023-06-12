@echo off 

echo Compiling YgoMaster / YgoMasterClient (C#)
echo.

REM Compile YgoMaster / YgoMasterClient with the .NET Framework redistributable compiler (should be on most systems)
call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe YgoMaster.sln /p:Configuration=Debug /p:Platform=x64

echo.
echo Compiling YgoMasterLoader (C++)
echo.

REM Compile YgoMasterLoader using cl (requires Visual Studio with C++ compilers) (TODO: Improve this... maybe also check vswhere.exe)
set BATPATH=%ProgramW6432%\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat
if exist "%BATPATH%" ( call "%BATPATH%" amd64 ) else ^
if defined VS190COMNTOOLS ( call "%VS190COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS180COMNTOOLS ( call "%VS180COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS170COMNTOOLS ( call "%VS170COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS160COMNTOOLS ( call "%VS160COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS150COMNTOOLS ( call "%VS150COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS140COMNTOOLS ( call "%VS140COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS130COMNTOOLS ( call "%VS130COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS120COMNTOOLS ( call "%VS120COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS110COMNTOOLS ( call "%VS110COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
if defined VS100COMNTOOLS ( call "%VS100COMNTOOLS%\..\..\VC\vcvarsall.bat" amd64 ) else ^
goto cppCompilerNotFound

cd YgoMasterLoader
cl YgoMasterLoader.cpp /LD /DWITHDETOURS /Fe:../YgoMaster/YgoMasterLoader.dll
cd ../
goto done

:cppCompilerNotFound
echo [ERROR] Failed to compile YgoMasterLoader.cpp
echo.
echo Solutions:
echo 1) Ignore this error if you already have YgoMasterLoader.dll
echo 2) Download a release build from github
echo 3) Install Visual Studio with C++ compilers and re-run Build.bat
echo 4) Manually compile it by reading the top of YgoMasterLoader.cpp

:done
echo.
pause