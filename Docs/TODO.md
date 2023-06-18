A rough list of possible features / things to do. There aren't any plans to implement any of these.

- Add a json file with all of the referenced IL2CPP types / methods / fields etc. Use this to automatically validate everything for updates
- Auto updating of `DuelDllActiveUserDoCommandOffset` / `DuelDllActiveUserSetIndexOffset`
- Embed Ultralight https://ultralig.ht/ or https://sciter.com/ and create a custom shop UI and solo content (visual novel style) - https://github.com/Monogatari/Monogatari / https://github.com/avgjs/avg-core / https://github.com/Kirilllive/tuesday-js / https://github.com/lunafromthemoon/RenJS-V2
- Implement emoting like in the WC games (auto emotes based on card actions)
- Add a textbox to the duel UI to send custom messages without having to modify the emote file
- Customize the PvP duel starter UI to allow for things such as rush duels / speed duels / custom hand / LP /etc
- Modify DuelSettings.cs to work with a raw Dictionary<string, object>. Change all members to properties which access the dictionary. Fix rank/rate which is currently broken under PvP duels / replays and the rank/rate is displayed incorrectly
- "First to 2" / "First to X" duels with side decking in duel rooms. Prefix duel room names with "FT2: XXXXXXX". Modify table names with info about the state of the best of 3. In the duel prefix player names with the current win count e.g. "1/2: XXXXXX". Allow modifying decks but modify the deck editor to limit what can be moved into the deck (this might be hard to visually do / make clear what is available in the deck editor UI).
- For archival purposes... implement downloading assets, implement the initial tutorial / name / deck selection, implement gift box, implement buying ($0) gems, implement missions, implement a basic matching system / the regular DUEL UI