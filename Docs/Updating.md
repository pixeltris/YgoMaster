## Enabling the client console

- In `ClientSettings.json` set `ShowConsole` to `true` and run the client.

## Card list / card craft list / ban list

- Log `User.home`.
- Take the contents of `Master.CardRare` and place it into `CardList.json`.
- Take the contents of `Master.CardCr` and place it into `CardCraftableList.json`.
- Take the contents of `Master.Regulation` and place it into `CardBanList.json`.

## Structure decks

- Log `User.home` and take the `Master.Structure` json and place them into `StructureDecks.json`.
- Run `--extractstructure` to extract those structure decks into individual files which get placed into the `StructureDecks` folder.

## YdkIds.txt

- In the client console run `carddata` which should create `/Data/ClientDataDump/Card/Data/{CLIENT_VERSION}/`, move and rename the `{CLIENT_VERSION}` folder to `/Data/CardData/`. You must do this while using the `English` language setting.
- Run `--updateydk` to update `YdkIds.txt`.

*To be run on client updates only.*

## Solo

- Log `Solo.info`, format the json and remove the user data `Solo.cleared` which should just leave `Master.Solo`.
- Log starting the the loaner deck of deck duel. Select the `Duel.begin` packet and copy the json into a file called CHAPTER_ID.json (where CHAPTER_ID is the duel chapter id) and place it into `/Data/SoloDuels/`.

## Shop

- Log entering the shop from the home screen and copy the log into `/Data/ShopDumps/`.
- Run the `--mergeshops` command to merge `/Data/ShopDumps/` into `/Data/AllShopsMerged.json`. Copy out what you want. For some shops like 3001 you'll need to manually fix up the shop price (and maybe the card list as well?).

For any new packs with new pack images...

- In the client console run `packimages` which will create `dump-packimages.txt` in the game folder. Copy the contents into `Shop.json` `PackShopImages`.

For shop pack odds...

- Log the odds from clicking the button in-game and copy the data into `ShopPackOdds.json`.

## ItemID.cs

- In the client console run `itemid` which will create `ItemID.json` in the game folder.
- Copy it over to `/Data/` and remove any invalid entries (some make the client crash / are blank).

## Misc

- In the client console run `updatediff` which will create `updatediff.cs` in the game folder.
- Run a diff against that file and `/Docs/updatediff.cs`. Update relevant code throughout YgoMaster based on the changes and then insert the new `updatediff.cs`.