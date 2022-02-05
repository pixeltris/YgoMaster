Offline Yu-Gi-Oh Master Duel

WIP:
[x] Implement CustomDuel.json
[x] Fix LocalData folder path lookup (steam id)
[x] Hook resource functions to replace images
[x] hook GetText to replace text (either hooked up to json of specific txt files)
[ ] Manually create a custom shop from the old boosters (with images)
[ ] Generate deck json files from LE data. Also generate Solo.json from LE data.
[ ] Create option to limit cards by release year
[ ] Move solo content into /Solo/Solo.json and /Solo/Decks/

[ ] Support varying card rarities based on the pack (i.e. boosters throughout the years). Need to do a request.Remove("Gacha.cardList") in Act_GachaGetCardList and don't send the gacha lists in Act_ShopGetList

Updating:
- Run the `itemid` command and copy the data from `dump-itemid.txt` into `ItemID.cs` (make sure to clean it up, some entries are invalid)
- Run the `packimages` command and copy the data from `dump-packimages.txt` into `Shop.json`
- Use YgoMaster.GameServer.MergeShopDumps() to merge logs of new shop packs

TODO:
- Look into supporting custom structure decks (custom name, desc, images)
- Add more options to ShopPackOddsVisuals.json
- Add default prices setting to Shop.json and add support for buying >1 / <10 packs for packs with a buy limit
- Find how to get generic method via IL2CPP reflection to correctly handle TextData hook
- Fix which player starts first on duels in /SoloDuel/ (they are recorded so have a specific starting duelist - remove where not required)
- Display the correct tutorial gate in "How to Obtain" for cards obtained in solo
- Add support for loading ydk decks on custom duels
- Look into changing solo card info - Definition/Solo/SoloCardThumbSettings, Prefabs/VC/Solo/SoloMode, Definition/Solo/SoloDefine, Definition/Solo/SoloFlyingCardSettings

Implemented:
- Editing player profile
- Deck editing / creation
- Crafting / dismantling
- Shop (excluding duel pass and bundle deals)
- Solo mode

Not implemented:
- Handling more than 1 player
- Exp / levels
- Missions
- Gift box
- PvP
- Replays
- Profile data (player stats)
- Friends list
- Downloads (i.e. patching via a custom patch server)
- Record of gems / items obtained

There aren't any plans to implement any of the above.

NOTES:
- Card pack thumbnails (that appear in the shop and as "iconData" in Shop.json) are looked up via "Images/Shop/HighlightThumbs/XXXXXX" (for "iconType":2 only)
- Card pack images are looked up via "Images/CardPack/<_RESOURCE_TYPE_>/<_CARD_ILLUST_>/XXXXXX"
- SoloCardThumbSettings / SoloResourceBinder are used to manage the card id / texture for the gate

Asset editing (shop packs):
- Images/Shop/HighlightThumbs/
- Images/CardPack/HighEnd_HD/tcg/
- Images/CardPack/SD/tcg/

Asset editing (card sleeve / deck case):
- Images/Protector/
- Images/DeckCase/HighEnd_HD/
- Images/DeckCase/SD/
- Images/DeckCase/

Asset editing (profile icon / frame):
- Images/ProfileFrame/HighEnd_HD/
- Images/ProfileFrame/SD/
- Images/ProfileFrame/
- Images/ProfileIcon/HighEnd_HD/
- Images/ProfileIcon/SD/
- Images/ProfileIcon/