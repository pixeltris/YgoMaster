@echo off 

echo Compiling YgoMasterFiddler
echo.

call %WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe YgoMasterFiddler.csproj /p:Configuration=Debug

:done
echo.
pause