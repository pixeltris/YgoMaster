## PvP

PvP lets you use duel rooms and the friends list. You can also trade cards.

## Setting it up on a single PC

- In `Data/Settings.json` set `MultiplayerEnabled` to `true`
- In `Data/ClientData/ClientSettings.json` set `MultiplayerToken` to some random text value
- Copy / paste the entire `YgoMaster` folder so that it creates `YgoMaster - Copy`
- Inside the `YgoMaster - Copy` folder edit `Data/ClientData/ClientSettings.json` and set `MultiplayerToken` to a different random text value to the previous folder
- Run `YgoMaster.exe` in the `YgoMaster` folder
- Run `YgoMasterClient.exe` in both folders

## Setting it up on LAN

- In `Data/Settings.json` set `MultiplayerEnabled` to `true`
- In `Data/ClientData/ClientSettings.json` set `MultiplayerToken` to some random text value
- Modify `BaseIP` in both `Data/Settings.json` and `Data/ClientData/ClientSettings.json` to point to the IP of the machine which runs `YgoMaster.exe`

*If this doesn't work then use the WAN instructions but use your LAN IP where it says "WAN IP".*

## Setting it up on WAN

- In `Data/Settings.json` set `MultiplayerEnabled` to `true`
- In `Settings.json` set `SessionServerIP` to `0.0.0.0`
- In `Settings.json` set `MultiplayerPvpClientConnectIP` to `localhost`
- In `Settings.json` set `BindIP` to `http://*:{BasePort}/`
- In `Settings.json` and `ClientSettings.json` set `BaseIP` to the WAN IP
- In `Data/ClientData/ClientSettings.json` set `MultiplayerToken` to some random text value

## Notes

- Every PC (and every seperate YgoMaster folder) must have a different `MultiplayerToken` as otherwise they will share the same session which will break things
- Do not modify `YgoMaster/Data/Players/` or any sub folders while `YgoMaster.exe` is running
- Clicking mates / duel field borders are synced with the other player. Spectators also see it but their clicks don't sync
- Sometimes clients bug out if you restart YgoMaster while in a duel (when you next enter a duel one client will get stuck before starting the duel). Restart all clients when reopening YgoMaster
- Play around with `MultiplayerNoDelay` (`Settings.json` / `ClientSettings.json`) to see which works best for you (it disables nagle's algorithm)
- Releases include a folder called `YgoMaster/Data/CardData/`. To Generate that folder yourself read [Updating.md](Updating.md)
- Client updates can break PvP. If you'd like to keep using PvP with YgoMaster you should wait until a new release before letting Steam update the client

## Starting duels

- Click `DUEL` in the home menu
- Click `Duel Room (PvP)` and create a duel room as you would in the normal game

## Trading cards

You can trade cards with other players by going to their profile and clicking "Trade"

- Both players need to click "Trade" to enter the trade
- Your cards go to the "main deck" and their cards go to the "extra deck"
- Use the button on the top right where "SAVE" normally is to complete your trade. There's a cooldown of a few seconds on the button when moving cards to avoid accidental trading. You will need to press the trade button again if either player modifies the cards. If the button says "Trade!!!" it means the other player has pressed the trade button
- Use the sub menu of the trade menu to view their cards
- You can add their cards / remove their cards from the trade
- You cannot craft / dismantle while trading

## Duel emotes

You can send a pre defined message to the other player during the duel by clicking your player icon in the bottom left of the screen then clicking an entry.

The raw text is sent over to the player allowing you to enter custom text. The text can be found in `YgoMaster/Data/ClientData/Text/Emotes.json`. Every time you click on your player icon the file is reloaded.

You can play sounds such as `This will make a sound plsd:SE_BUFF_CHANGE`