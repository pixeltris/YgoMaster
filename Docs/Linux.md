# YgoMaster for Linux

- Install Steam
- Install Master Duel using Steam
- Complete the tutorial in Master Duel to download all content
- Download the latest YgoMaster release from https://github.com/pixeltris/YgoMaster/releases
- Copy the `YgoMaster` folder (the folder, not the contents of the folder) into the game folder (right click the game in Steam -> Manage -> Browse local files)
- [Download this zip](https://github.com/pixeltris/YgoMaster/releases/download/v1.50/YgoMaster-Linux-Data-v1.zip) and extract / merge it with the `YgoMaster` folder from the previous step
- Go to your Steam library and at the bottom left click "Add a Game" and then click "Add a Non-Steam Game..."
- A popup should appear, at the bottom left click "Browse..." and locate the YgoMaster folder and choose `MonoRun.exe` and then click "Add Selected Programs"
- Right click "MonoRun.exe" in your Steam library and then click "Properties"
- In the properties window rename "MonoRun.exe" in the first text box to "YgoMaster"
- In the "LAUNCH OPTIONS" text box put `YgoMasterClient.exe`
- Click on the "Compatibility" tab on the left side of the properties window
- Check "Force the use of a specific Steam Play compatibility tool"
- In the dropdown select the newest version of Proton (e.g. Proton 9.0-4)
- Close the properties window
- Find YgoMaster in your Steam library and then click PLAY

You can get Steam artwork from https://www.steamgriddb.com/game/5316261

## Folder structure

Your folder structure should look like this:

```md
Master Duel
├── YgoMaster
│   ├── Data/
│   ├── mono/
│   ├── MonoRun.exe
│   ├── YgoMasterClient.exe
│   ├── YgoMaster.exe
│   ├── YgoMasterLoader.dll
├── ...
```

Make sure you copied `mono` / `MonoRun.exe` from the zip into the correct place as noted into the above folder structure.

## Where the `mono` files come from

The files inside `mono` were copied from the latest mono stable release https://www.mono-project.com/download/stable/#download-win

- `%ProgramFiles%/mono/lib/mono/4.5/` folder was stripped down and copied to the output `mono/lib`. Additionally `mscorlib.dll` was copied from the `4.5` folder into `mono/lib/mono/4.5/mscorlib.dll` as that is where mono wants it
- `%ProgramFiles%/mono/etc/` was copied as-is to the output `mono/etc`
- `%ProgramFiles%/mono/bin/*.dll` was copied as-is to the output `mono/bin`

## Diagnosing issues with mono

Create `mono_log.txt` in the Master Duel folder and inside the `YgoMaster` folder. This will log all mono messages in these files which you can look through to diagnose problems with mono.
