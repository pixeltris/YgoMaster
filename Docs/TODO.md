A rough list of possible features / things to do. There aren't any plans to implement any of these.

- Auto updating of `DuelDllActiveUserDoCommandOffset` / `DuelDllActiveUserSetIndexOffset` *NOTE: This is done but it's not currently open source*
- Embed Ultralight https://ultralig.ht/ or https://sciter.com/ and create a custom shop UI and solo content (visual novel style) - https://github.com/Monogatari/Monogatari / https://github.com/avgjs/avg-core / https://github.com/Kirilllive/tuesday-js / https://github.com/lunafromthemoon/RenJS-V2
- Customize the PvP duel starter UI to allow for things such as rush duels / speed duels / custom hand / LP /etc
- Modify DuelSettings.cs to work with a raw Dictionary<string, object>. Change all members to properties which access the dictionary. Fix rank/rate which is currently broken under PvP duels / replays and the rank/rate is displayed incorrectly

There are issues with using LoadImmediate. If a Load is called while we are doing a LoadImmediate call there's a race condition and one can fail. Steps to reproduce:
- Unlock all items
- Add new deluxe protectors
- Set your protector as the first protector in the list ("Yu-Gi-Oh Trading Card Game")
- Reopen the game
- Scroll down to the bottom the protectors list triggering a Load on deluxe protector "Sky Striker Conversion" and a LoadImmediate for the custom protectors
- One will often fail to load (either our LoadImmediate or the Load) with the error "0b\0b2c8765 can't be loaded because another AssetBundle with the same files is already loaded."