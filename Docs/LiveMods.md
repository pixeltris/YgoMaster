# Live mods

A select number of mods from YgoMasterClient can be applied to the live game to improve the experience of the game.

*These **do not give you a competitive advantage** in PvP / solo duels and do not let you obtain any additional cards, gems, items, or increase your exp, duelist level, rank, etc.*

***Modding the live game risks getting your account banned.***

## Mods

The following mods can be applied to the live game:

- Custom PvE duels (no rewards).
- Card collection stats.
- Load / save decks (YDK / JSON).
- Copy / paste decks from the clipboard (YDKe / YDK / JSON / TXT).
- Break deck editor limits (add banned cards / add more than 3 of any card).

## Usage

If `YgoMaster.exe` is not found then `YgoMasterClient.exe` will attempt to inject into master duel. Therefore you can simply:

- Rename or delete `YgoMaster.exe`
- Run `YgoMasterClient.exe`

An alternative to the above is running `YgoMasterClient.exe live` using your favorite shell / command prompt.

### Additional info

- When successfully injected `(modded)` is appended to the window title of the game.
- Data files are required so do not delete `/Data/`.
- The mentioned features operate in the same way as the offline version (unless otherwise specified).

## Breaking deck editor limits

When breaking deck editor limits the server will not save your deck. You must therefore use the save / load / clipboard functionalities provided in the deck editor sub menu. Starting a duel with more than 75 cards (60 main + 15 extra) seems to break the game.

To enable this set `DeckEditorDisableLimits` to `true` in `/Data/ClientData/ClientSettings.json`.

## Custom PvE duels

Starting the second tutorial duel will open the custom PvE duel starter. The custom duel starter will only open if the tutorial is already complete. There are no rewards for completing the custom duel.

## Disabling "(modded)" on the window title

Set `ChangeWindowTitleOnLiveMod` to `false` in `/Data/ClientData/ClientSettings.json`.