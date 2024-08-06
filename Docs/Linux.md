# YgoMaster for Linux

- install wine
- install .NET Framework under wine
- run `wine regedit` and do this to fix input on window focus change (create keys which don't exist) https://askubuntu.com/questions/299286/how-to-recover-focus-after-losing-it-while-using-wine/1202472#1202472
- run `wine YgoMasterClient.exe`

If you'd like to run without the wine logging you can use this command instead `WINEDEBUG=-all wine YgoMasterClient.exe`

## ClientSettings.json changes (required)

Currently you need edit `YgoMaster/Data/ClientData/ClientSettings.json`

Change `{ProxyPort}` to `{BasePort}` in both the `"ServerUrl"` and `"ServerPollUrl"`

NOTE: The client doesn't want to connect on anything other than localhost which is the reason why `ProxyPort` exists. As the proxy isn't being used in this case you cannot use any IP other than localhost for `BaseIP`. The reason why the above change is required under Linux is because the HTTP proxy wasn't implemented properly and the HTTP listener is ignoring requests due to the `Host` address of the HTTP request not matching the HTTP listener's host name.

NOTE: PvP currently doesn't work on Linux unless you run the `YgoMaster.exe` server on a Windows machine. If you do that then you will need to undo the changes to `"ServerUrl"` / `"ServerPollUrl"`.

TODO: Fix either the HTTP proxy or the HTTP Listener to allow the connection through.

## Bugs

- `ClientSettings.json` setting `ShowConsole` does not work