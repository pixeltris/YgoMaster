# YgoMaster for Linux

- Install wine
- Install .NET Framework under wine
- Run `wine regedit` and do this to fix input on window focus change (create keys which don't exist) https://askubuntu.com/questions/299286/how-to-recover-focus-after-losing-it-while-using-wine/1202472#1202472
- In `Data/ClientData/ClientSettings.json` change `{ProxyPort}` to `{BasePort}` in both `ServerUrl` and `ServerPollUrl`
- In `Data/Settings.json` change `BindIP` to `http://+:{BasePort}/`
- Run `wine YgoMasterClient.exe`

## PvP (server)

- In `Data/Settings.json` set `MultiplayerEnabled` to `true`
- In `Data/Settings.json` set `BaseIP` to `0.0.0.0`
- Install mono
- Run `mono YgoMaster.exe`

## PvP (clients)

- In `Data/ClientData/ClientSettings.json` set `MultiplayerToken` to some random text value
- In `Data/ClientData/ClientSettings.json` change `{BasePort}` back to `{ProxyPort}` in both `ServerUrl` and `ServerPollUrl`
- In `Data/ClientData/ClientSettings.json` set `BaseIP` to point to the IP of the server (the LAN IP of the PC which runs the server)
- Run `wine YgoMasterClient.exe`

## Bugs

- `ClientSettings.json` setting `ShowConsole` does not work

## Note to self

- The HTTP server under wine seems to check if the hostname / port matches the server's in the HTTP headers and if it doesn't it ignores the request. This is why `{ProxyPort}` doesn't work when the target server is running under wine
- The HTTP server under mono seems to work fine so you can use `{ProxyPort}` if running `mono YgoMaster.exe`. This also works on non-multiplayer but if multiplayer isn't enabled it'll keep trying to open another instance of `YgoMaster.exe` under wine
- The reason for the `{ProxyPort}` in the first place is because the client rejects attempts to connect to anything outside of localhost (if not using HTTPS) so a proxy is required. TODO: Possibly look where they make HTTP requests and change it to be a custom HTTP sender without the localhost restrictions, or fix up the HTTP headers in the proxy so the wine HTTP server doesn't ignore the requests which should avoid needing to make any json tweaks other than for PvP