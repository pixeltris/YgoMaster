# YgoMaster

Offline Yu-Gi-Oh! Master Duel (PC)

*Online (PvP) functionality is not implemented. Progress is not shared with the live game.*

## Features

- Create decks
- Open packs
- Solo content
- Custom duels
- YDK / YDKe support
- Card collection stats / deck editor sub menu improvements

[Some features can be modded into the live game (at your own risk).](Docs/LiveMods.md)

## Requirements

- .NET Framework 4.0 (or above).
- The game fully downloaded via Steam (~5GB).

*You must complete the tutorial on Steam to fully download the game.*

*YgoMaster is portable. It can be used on any machine without Steam installed (after being fully downloaded).*

## Usage

- Download the latest release from https://github.com/pixeltris/YgoMaster/releases
- Copy the `Build` folder (the folder, not the contents of the folder) into the game folder.
- Run `YgoMasterClient.exe` (this should also auto run `YgoMaster.exe`, if it doesn't manually run it).
- In the game settings change the language to match the language set via Steam.

Additionally...

- [It's recommended that you tailor the server settings to your preferences.](Docs/Settings.md)
- The custom duel starter UI can be accessed via the DUEL button on the home screen.
- If you see `FILE LOAD ERROR` (or other popups) [follow these instructions](Docs/FileLoadError.md).

## Compiling from source

- Install Visual Studio with C++ compilers.
- Run `Build.bat`.
- Copy the `Build` folder into the game folder as mentioned above.

Running `Build.bat` is the equivilant of:

- Compiling `YgoMaster.sln` with Visual Studio.
- Compiling `YgoMasterLoader.cpp` with `cl`.

## Related

- https://github.com/SethPDA/MasterDuel-Modding/wiki
- https://www.nexusmods.com/yugiohmasterduel/mods

## Screenshots

![Alt text](Docs/Pics/ss1.png)
![Alt text](Docs/Pics/ss2.png)
![Alt text](Docs/Pics/ss3.png)
![Alt text](Docs/Pics/ss4.png)
![Alt text](Docs/Pics/ss5.png)
![Alt text](Docs/Pics/ss6.png)
![Alt text](Docs/Pics/ss7.png)