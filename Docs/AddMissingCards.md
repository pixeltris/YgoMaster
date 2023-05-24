It's possible to add cards to game before they are officially released as packs.

- Edit `Data/ClientData/ClientSettings.json` and change `ShowConsole` from `false` to `true`.
- Open `YgoMasterClient.exe`. You should also see a console window which belongs to the game window. In that console type `carddata`. This should say `done`.
- Navigate to `Data/ClientDataDump/Card/Data/XXXXX` which should have been created by the above step. Copy the `XXXXX` folder to Data/ and rename it to `CardData`. So it should be `Data/CardData/`.
- Open `cmd` and use `cd` to change the path to the YgoMaster folder. Then type `ygomaster.exe --missing-cards`.

You should now see `missing-cards.txt` in your folder. You can then use these card ids and just add them to the end of `CardList.json` and `CardCraftableList.json`. You will also need images for these cards if they aren't already in the game. To do this create an image in the following location where `XXXXX` is the card id:

`Data/ClientData/Card/Images/Illust/tcg/XXXXX.png`

You will likely add monsters which are flagged with animations but don't yet have animation files. This will freeze the game when you summon them. To disable all summon animations open `Data/ClientData/ClientSettings.json` and change `DuelClientDisableCutinAnimations` from `false` to `true`.