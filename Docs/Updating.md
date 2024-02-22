## Logging requirements

- Download Fiddler Classic https://www.telerik.com/download/fiddler (install to `C:/Program Files (x86)/Fiddler/` as the YgoMasterFiddler project file uses this path)
- Run `YgoMaster/YgoMasterFiddler/Build.bat`
- Copy `YgoMaster/YgoMasterFiddler/bin/Debug/YgoMasterFiddler.dll` to `C:/Program Files (x86)/Fiddler/Inspectors/`
- Run Fiddler, select a game network request and click the two `YgoMaster` tabs (request / response)

Certain logs will be dumped as json into `%USERPROFILE%/Documents/Fiddler2/Captures/YgoMasterUpdate/` when the log is clicked and both `YgoMaster` tabs are selected.

*Obtain all logs from the Steam version of the game. Not YgoMaster.*

## Card list / card craft list / ban list

- Go back to the title screen
- Enter the game and log `/ayk/api/User.home`
- Copy the following files from `YgoMasterUpdate/` to `YgoMaster/Data/`
- `CardCraftableList.json`, `CardList.json`, `Regulation.json`, `RegulationIcon.json`, `RegulationInfo.json`, `StructureDecks.json`

## Structure decks

- Run `YgoMaster.exe --extractstructure` to extract those structure decks from the previous step into individual files which get placed into the `StructureDecks` folder.
- Delete `YgoMaster/Data/StructureDecks.json`

## Solo

- Click `SOLO` to log `/ayk/api/Solo.info`
- Copy `YgoMasterUpdate/Solo.json` to `YgoMaster/Data/Solo.json`
- Log `/ayk/api/Duel.begin` for each duel (**loaner deck only**)
- Copy all files in `YgoMasterUpdate/SoloDuels/` to `YgoMaster/Data/SoloDuels/`

*Enable the hidden client setting `AlwaysWin` to win duels on the live game by surrendering to log this data faster.*

## Shop

- Download the latest shops list from [here](https://github.com/pixeltris/YgoMaster/issues/129) and place them `YgoMaster/Data/ShopDumps/`
- Click `SHOP` to log `/ayk/api/Shop.get_list`
- Rename the `YgoMasterUpdate/Shop.json` log to the update date and copy it into `YgoMaster/Data/ShopDumps/`
- In game select `Master Pack` and click `Cards included in this pack`. Manually copy the response of `/ayk/api/Gacha.get_card_list` into `Data/ShopDumps/Gacha-10000001.json`
- Run the `YgoMaster.exe --mergeshops` command to merge `/Data/ShopDumps/` into `/Data/AllShopsMerged.json`. Copy out what is needed. For some shops like 3001 you'll need to manually fix up the shop price (and maybe the card list as well?).
- Zip up your `ShopDumps` folder and update the link at the top of this section. Then delete your `ShopDumps` folder.

For any new packs with new pack images...

- In the client console run `packimages` which will create `dump-packimages.txt` in the game folder. Copy the contents into `Shop.json` `PackShopImages`.

For shop pack odds...

- Log the odds from clicking the button in-game and copy the data into `ShopPackOdds.json`.

For creating `YgoMaster/Data/ShopDumps/` from scratch...

- Create a new Steam account
- Complete the tutorial
- Go to the SOLO screen and enter `solo_clear` into the client console - this will take some time to complete and will complete all solo content
- Keep buying Master Pack bundles / packs until you have no gems
- Go to the deck editor and enter `dismantle_all_cards SuperRare UltraRare` into the client console - this will dismantle every SR/UR you own
- Go to the shop and enter `craft_secrets` into the client console - this will craft every missing secret pack
- In the shop enter `auto_free_pull` into the client console - this will open every free pull secret pack
- Re-enter the shop to log `/ayk/api/Shop.get_list`
- Save the log into `YgoMaster/Data/ShopDumps/Shop-2024-02-22.json` (or whatever the current date is)
- Get the card list for the Master Pack and save the `/ayk/api/Gacha.get_card_list` log into `YgoMaster/Data/ShopDumps/Gacha-10000001.json`
- Get the card list for Legacy Pack and save the `/ayk/api/Gacha.get_card_list` log into `YgoMaster/Data/ShopDumps/Gacha-10003001.json`
- Follow the above `YgoMaster.exe --mergeshops` instructions

## YdkIds.txt

- In the client console run `carddata` which should create `/Data/ClientDataDump/Card/Data/{CLIENT_VERSION}/`, move and rename the `{CLIENT_VERSION}` folder to `/Data/CardData/`. You must do this while using the `English` language setting.
- Run `YgoMaster.exe --updateydk` to update `YdkIds.txt`.

## ItemID.json

- In the client console run `itemid` which will create `ItemID.json` in the game folder.
- Copy it over to `/Data/` and remove any invalid entries (some make the client crash / are blank).

## Misc

- In the client console run `updatediff` which will create `updatediff.cs` in the game folder.
- Run a diff against that file and `/Docs/updatediff.cs`. Update relevant code throughout YgoMaster based on the changes and then insert the new `updatediff.cs`.
- After client updates uncomment `ReflectionValidator.ValidateDump()` in `Program.cs` and check the output. Then re-comment it, uncomment `ReflectionValidator.IsDumping = true;` and run again to update `ReflectionDump.json` (after fixing any broken code from the previous step).

## Enabling the client console

- In `ClientSettings.json` set `ShowConsole` to `true` and run the YgoMasterClient.