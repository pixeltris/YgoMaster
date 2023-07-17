# YgoMaster for Linux

- install wine
- install .NET Framework under wine
- run `wine regedit` and do this to fix input on window focus change (create keys which don't exist) https://askubuntu.com/questions/299286/how-to-recover-focus-after-losing-it-while-using-wine/1202472#1202472
- run `wine YgoMasterClient.exe`

If you'd like to run without the wine logging you can use this command instead `WINEDEBUG=-all wine YgoMasterClient.exe`

## Bugs

- `ClientSettings.json` setting `ShowConsole` does not work