Offline Yu-Gi-Oh Master Duel

WIP:
- Add solo content
- Implement "how to obtain"
- Add custom duel start UI
- Add ability to create custom packs (custom images, etc)

Updating:
- Run the `itemid` command and copy the data from `dump-itemid.txt` into `ItemID.cs` (make sure to clean it up, some entries are invalid)
- Run the `packimages` command and copy the data from `dump-packimages.txt` into `Shop.json`
- Use YgoMaster.GameServer.MergeShopDumps() to merge logs of new shop packs

TODO:
- Look at implementation of OpenReportDialog - it has a multi select dialog
- Look into supporting custom structure decks (custom name, desc, images)
- Find out how to give an error response to an expired shop pack (should be able to get it from a secret as it only lasts 24 hours and you can easily buy an expired one)
- Add more options to ShopPackOddsVisuals.json
- Add support for "searchCategory" in "Shop.get_list"
- Add default prices setting to Shop.json and add support for buying >1 / <10 packs for packs with a buy limit
- Implement how to obtain card packet
- Fix "live" injector (crashes the client 80% of the time)
- Find how to get generic method via IL2CPP reflection to correctly handle TextData hook
- Patch the GetSteamID xrefs and set to existing folder (maybe hook GetSteamID / crc func and add special check in crc func for returned string)
- Fix which player starts first on duels in /SoloDuel/ (they are recorded so have a specific starting duelist - remove where not required)

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