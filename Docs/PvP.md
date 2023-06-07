## PvP

PvP is a work in progress. It lets you use duel rooms under a low latency environment (LAN). The friends list also works

## Setting it up on a single PC

- Open `Data/Setting.json` and set `MultiplayerEnabled` to `true`
- Open `Data/ClientData/ClientSettings.json` and set `MultiplayerToken` to some random text value
- Copy / paste the entire `YgoMaster` folder so that it creates `YgoMaster - Copy`
- Inside that `YgoMaster - Copy` folder edit `Data/ClientData/ClientSettings.json` and set `MultiplayerToken` to a different random text value to the previous folder
- Run `YgoMaster.exe` in the `YgoMaster` folder
- Run `YgoMasterClient.exe` in both folders

## Setting it up on LAN/WAN

- Modify `BaseIP` in both `Data/Setting.json` and `Data/ClientData/ClientSettings.json` to point to the IP of the machine which runs `YgoMaster.exe`
- On WAN you will need to use `*` for `BaseIP` and `0.0.0.0` for `SessionServerIP` in `Settings.json`. In `ClientSettings.json` you will want to set `BaseIP` to the WAN IP
- Play around with `MultiplayerNoDelay` (`Setting.json` / `ClientSettings.json`) to see which works best for you (it disables nagle's algorithm)

## Starting duels

- Click `DUEL` in the home menu
- Click `Duel Room (PvP)` and create a duel room as you would in the normal game

## Bugs

- For some people clicking `OnClickRoomMatchMenuItem` param isn't working which results in any menu item going to create a duel room