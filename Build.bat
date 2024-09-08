@echo off 

set VSWHERE_PATH=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
for /f "delims=" %%i in ('call "%VSWHERE_PATH%" -latest -property installationPath') do set VS_LATEST=%%i
REM VS_LATEST = C:\Program Files\Microsoft Visual Studio\2022\Professional
set VS_MSBUILD=%VS_LATEST%\MSBuild\Current\Bin\MSBuild.exe
set VS_VCVARSALL=%VS_LATEST%\VC\Auxiliary\Build\vcvarsall.bat
if not exist "%VS_MSBUILD%" (
    goto vsNotFound
)

REM Compile YgoMaster / YgoMasterClient with the .NET Framework redistributable compiler (should be on most systems)
echo.
echo Compiling YgoMaster / YgoMasterClient (C#)
echo.
call %VS_MSBUILD% YgoMaster.sln /p:Configuration=Debug /p:Platform=x64

REM Compile YgoMasterLoader using cl (requires Visual Studio with C++ compilers)
echo.
echo Compiling YgoMasterLoader (C++)
echo.
call "%VS_VCVARSALL%" amd64
cd YgoMasterLoader
cl YgoMasterLoader.cpp /LD /DWITHDETOURS /Fe:../YgoMaster/YgoMasterLoader.dll
cl MonoRun.cpp /Fe:../YgoMaster/MonoRun.exe
cd ../
goto done

REM Error handling

:vsNotFound
echo [ERROR] Visual Studio not found
echo.
echo Solutions:
echo 1) Install Visual Studio with C++ and C# workload
goto done

REM Done

:done
echo.
pause
