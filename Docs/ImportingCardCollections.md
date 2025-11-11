## How to import cards into Player.json

`YgoMaster.exe --import_card_collection [args] [path]`

## args

- _**`--set`**_ Sets the card count to the given value rather than adding to the existing card count. e.g. if you have already have 6x of a card and you `--set --count 3` you'll end up with 3x of that card instead of 9x
- _**`--set-clear`**_ When using `--set` this will also clear all existing entries for the given card id before doing the set (all rarities are set to 0 before setting the card count)
- _**`--unique`**_ Every card id is to only be counted once. e.g. `--unique --count 5` will add 5 of a given card id regardless of how many times the card id appears in the import data, without the `--unique` it'll add 5x every time it sees the given card id in the import data
- _**`--count [number]`**_ The number of cards to add for each card id. e.g. `--count 5`
- _**`--rarity [string]`**_ Forces the given rarity for all cards added. Values: `Normal`, `Shine`, `Royal`
- _**`--default-rarity [string]`**_ The rarity to use if there's no rarity defined in the imported data. Values: `Normal`, `Shine`, `Royal`
- _**`--player [number]`**_ Targets a specific player ID to update. e.g. `--player 209541230` (the player needs to exist under /Data/Players/)
- _**`--all-network-players`**_ Applies the import to all networked players (all players in the "Players" folder other than "Local")
- _**`--clear`**_ Completely clears the existing players card collection before doing any importing. You can use this to clear your card collection without importing anything by not specifying any import file
- _**`--recursive`**_ When the import path is a folder this will recursively look inside the target folder

## path

Either a full file path (json, ydk, lflist.conf) or a full folder path which contains many json/ydk/lflist.conf files. The path must be put after all args.

## Note

You need to close YgoMasterClient / YgoMaster and then do the command and then reopen YgoMasterClient / YgoMaster. You need to use a shell to run this command (e.g. cmd or PowerShell).

## Example 1

This will clear player 775503945's existing card collection and set 3x shine (glossy) for all cards in GOAT format. You can get the GOAT format file from https://raw.githubusercontent.com/ProjectIgnis/LFLists/refs/heads/master/GOAT.lflist.conf

`YgoMaster.exe --import_card_collection --player 775503945 --clear --set --count 3 --rarity Shine "C:\Users\PC\Desktop\GOAT.lflist.conf"`

## Example 2

This will add 3x of every card in GOAT format to the `Local` player's existing card collection

`YgoMaster.exe --import_card_collection --count 3 "C:\Users\PC\Desktop\GOAT.lflist.conf"`

## Example 3

This will add all cards in all files which are in the folder "Desktop" and it'll search recursively. It'll add each card 1x however many times they appear in the files that are found and it'll use the rarties from the files.

`YgoMaster.exe --import_card_collection --recursive "C:\Users\PC\Desktop"`