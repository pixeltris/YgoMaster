## Logging requirements

- Download Fiddler Classic https://www.telerik.com/download/fiddler (install to `C:/Program Files (x86)/Fiddler/` as the YgoMasterFiddler project file uses this path)
- Download and unzip the YgoMaster source code https://github.com/pixeltris/YgoMaster/archive/refs/heads/master.zip
- Run `YgoMaster-master/YgoMasterFiddler/Build.bat`
- Copy `YgoMaster-master/YgoMasterFiddler/bin/Debug/YgoMasterFiddler.dll` to `C:/Program Files (x86)/Fiddler/Inspectors/`
- Run Fiddler, select a game network request and click the two `YgoMaster` tabs (request / response)

Certain logs will be dumped as json into `%USERPROFILE%/Documents/Fiddler2/Captures/YgoMasterUpdate/` when the log is clicked and both `YgoMaster` tabs are selected.

*Obtain all logs from the Steam version of the game. Not YgoMaster.*

## Card list / card craft list / ban list

- Open the game using Steam (reopen it if it's already open)
- Enter the game and log `/ayk/api/System.info` / `/ayk/api/User.entry` / `/ayk/api/User.home`
- Copy the following files from `YgoMasterUpdate/` to `YgoMaster/Data/`
- `CardCraftableList.json`, `CardList.json`, `Regulation.json`, `RegulationIcon.json`, `RegulationInfo.json`, `StructureDecks.json`, `TitleLoop.json`

## Structure decks

- Run `YgoMaster.exe --extractstructure` to extract those structure decks from the previous step into individual files which get placed into the `StructureDecks` folder.
- Delete `YgoMaster/Data/StructureDecks.json`

## Solo

- Click `SOLO` to log `/ayk/api/Solo.info`
- Copy `YgoMasterUpdate/Solo.json` to `YgoMaster/Data/Solo.json`
- Log `/ayk/api/Duel.begin` for each duel (**loaner deck only**)
- Log `/ayk/api/Solo.detail` for each duel
- Copy all files in `YgoMasterUpdate/SoloDuels/` to `YgoMaster/Data/SoloDuels/`
- Copy all files in `YgoMasterUpdate/SoloNpcDeckIds/` to `YgoMaster/Data/SoloNpcDeckIds/`
- Run `YgoMaster.exe --merge-deckids` to update `SoloNpcDeckIds.json` based on the `SoloNpcDeckIds` files

*To log this data faster add `"AlwaysWin": true` to `ClientSettings.json` to win duels [on the live game](LiveMods.md) by surrendering.*

## Shop

For updating `Shop.json`...

- Create a new Steam account
- Complete the tutorial
- Go to the SOLO screen and enter `solo_clear` into the client console - this will take some time to complete and will complete all solo content
- Keep buying Master Pack bundles / packs until you have no gems
- Go to the deck editor and enter `dismantle_all_cards SuperRare UltraRare` into the client console - this will dismantle every SR/UR you own (you will need to use this command multiple times as it doesn't seem to dismantle all as it should)
- Go to the shop and enter `craft_secrets` into the client console - this will craft every missing secret pack
- In the shop enter `auto_free_pull` into the client console - this will open every free pull secret pack (you will need to re-enter the shop and do the command multiple times until no more packs open)
- Re-enter the shop to log `/ayk/api/Shop.get_list`
- Save the log into `YgoMaster/Data/ShopDumps/Shop.json`
- In game select `Master Pack` and click `Cards included in this pack`. Log `Gacha.get_card_list` to generate `Gacha-10000001.json` and copy it into ShopDumps
- In game select `Legacy Pack` and click `Cards included in this pack`. Log `Gacha.get_card_list` to generate `Gacha-10003001.json` and copy it into ShopDumps
- Run the `YgoMaster.exe --mergeshops` command to merge `YgoMaster/Data/ShopDumps/` into `YgoMaster/Data/AllShopsMerged.json`
- Copy everything from `YgoMaster/Data/AllShopsMerged.json` and paste it into `YgoMaster/Data/Shop.json` replacing anything that already exists (except from the top entries)

For any new packs with new pack images...

- In the client console run `packimages` which will create `dump-packimages.txt` in the game folder. Copy the contents into `Shop.json` under the json entry `PackShopImages`.

For shop pack odds...

- Log the odds from clicking the button in-game and copy the data into `ShopPackOdds.json`.

## Docs/AltCardsYdk.json

`Docs/AltCardsYdk.json` is used for mapping missing alt art card ids when generating `YdkIds.txt`

- Run `YgoMaster.exe --unknown-alt-cards` which will print a list of missing alt card ids (or nothing if there's nothing new)
- Navigate to ygoprodeck.com (or duelingnexus.com/wiki), search the card, copy the alt art image url, take the number in the url to get the ydk id
- Update `Docs/AltCardsYdk.json` manually based on the found card id / ydk id

NOTE: Card ids >= 30000 seem to be used for promotions / announcements and aren't real cards.

## YdkIds.txt

- In the client console run `carddata` which should create `YgoMaster/Data/ClientDataDump/Card/Data/{CLIENT_VERSION}/`, move and rename the `{CLIENT_VERSION}` folder to `YgoMaster/Data/CardData/`. You must do this while using the `English` language setting.
- Run `YgoMaster.exe --updateydk` to update `YdkIds.txt`.

## PvP card data

- To update PvP card data you must follow the above YdkIds.txt instructions. If you don't do this any newly added cards wont function correctly in PvP as the duel engine requires `YgoMaster/Data/CardData/` being up to date.

## ItemID.json

- In the client console run `itemid` which will create `ItemID.json` in the game folder.
- Copy it over to `YgoMaster/Data/` and remove any invalid entries (some make the client crash / are blank).

## Bgm.json

- For new duel fields you'll need to obtain the duel field and then log starting a PvP duel looking for `Duel.begin` and check the BGM entries then manually modify Bgm.json to set the correct BGM for the duel field

## Enabling the client console

- In `ClientSettings.json` set `ShowConsole` to `true` and run the YgoMasterClient.
- You will probably want to inject into the live version of the game while doing this. See [LiveMods.md](LiveMods.md). This is a requirement if you want to complete solo with the `AlwaysWin` setting.

## Client updates

Client updates typically occur every few months (see https://steamdb.info/depot/1449853/manifests/). Client changes often break YgoMaster and this requires coding knowledge to fix. The following can be used to get some insight about the changes to the client:

- In the client console run `updatediff` which will create `updatediff.cs` in the game folder.
- Run a diff against that file and `/Docs/updatediff.cs`. Update relevant code throughout YgoMaster based on the changes and then insert the new `updatediff.cs`.
- After client updates enable `ReflectionValidatorValidate` in `ClientSettings.json` and check the output. Then disable it and enable `ReflectionValidatorDump` and run again to update `ReflectionDump.json` (after fixing any broken code from the previous step).
- Update `MultiplayerPvpClientDoCommandUserOffset` / `MultiplayerPvpClientRunDialogUserOffset` in Settings.json to fix controlling the opponents hand in PvP (TODO: Provide a tool to do this)

## Unity Engine updates

The following needs to be updated in ClientSettings.json if using custom content (sound images). See `UnityPlayerPdb.cs`

- UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal
- UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound
- UnityPlayerRVA_AudioClip_CUSTOM_SetData
- UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create
- UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative