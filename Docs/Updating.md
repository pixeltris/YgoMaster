## Card list / card craft list / ban list

- Log `User.home`.
- Take the contents of `Master.CardRare` and place it into `CardList.json`.
- Take the contents of `Master.CardCr` and place it into `CardCraftableList.json`.
- Take the contents of `Master.Regulation` and place it into `CardBanList.json`.

## YdkIds.txt

- In `ClientSettings.json` set `ShowConsole` to `true` and run the client.
- In the client console run `carddata` which should create `/Data/ClientDataDump/CardData/`, copy the `CardData` folder to `/Data/CardData/`.
- Run `--updateydk` to update `YdkIds.txt`

## Solo

- Log `Solo.info`, format the json and remove the user data `Solo.cleared` which should just leave `Master.solo`.
- Log starting the the loaner deck of deck duel. Select the `Duel.begin` packet and copy the json into a file called CHAPTER_ID.json (where CHAPTER_ID is the duel chapter id) and place it into the `SoloDuels` folder.

## Shop

- Log entering the shop from the home screen and copy the log into the `ShopDumps` folder.
- Run the `--mergeshops` command to merge the `ShopDumps` folder into `AllShopsMerged.json`. Copy out what you want. For some shops like 3001 you'll need to manually fix up the shop price (and maybe the card list as well?).

For any new packs with new pack images...

- In `ClientSettings.json` set `ShowConsole` to `true` and run the client.
- In the client console `packimages` which should create `dump-packimages.txt` in the root folder. Copy the contents into `Shop.json` `PackShopImages`.

For shop pack odds...

- Log the odds from clicking the button in-game and copy the data into `ShopPackOdds.json`.

## Structure decks

- Log `User.home` and take the `Master.Structure` json and place them into `StructureDecks.json`.
- Run `--extractstructure` to extract those structure decks into individual files which get placed into the `StructureDecks` folder.

## ItemID.cs

- In `ClientSettings.json` set `ShowConsole` to `true` and run the client.
- In the client console run `itemid` which should produce `dump-itemid.txt` in the root folder.
- Manually copy the contents into `ItemID.cs`, and remove any invalid entries (some make the client crash / are blank).