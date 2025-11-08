# YgoMaster for Linux

- Install wine
- Do the following to fix input on window focus change https://askubuntu.com/questions/299286/how-to-recover-focus-after-losing-it-while-using-wine/1202472#1202472
- Download the latest YgoMaster release from https://github.com/pixeltris/YgoMaster/releases
- Copy the `YgoMaster` folder (the folder, not the contents of the folder) into the game folder
- [Download this zip](https://github.com/pixeltris/YgoMaster/releases/download/v1.50/YgoMaster-Linux-Data-v1.zip) and extract / merge it with the YgoMaster folder from the previous step
- Run `wine MonoRun.exe YgoMasterClient.exe`

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

## Missing fonts

You may have missing fonts under wine. If the game crashes when prompted to load files / folders in the custom sub menus (i.e. loading decks from files) try the following:

- Try adding the `Tahoma` font using `winetricks` (this worked for me)
  - Run `winetricks`
  - Select the default wineprefix
  - Install a font
  - Check `tahoma` and press OK
- Try installing `sudo apt-get install ttf-mscorefonts-installer`
- Diagnose the font issue yourself
  - Run `WINEDEBUG=+font wine MonoRun.exe YgoMasterClient.exe`
  - Go to the deck editor, open the sub menu in the top right and click to load a deck from a file
  - The game will crash
  - Check the console output for the last font that tried to load
  - Install the mentioned font with winetricks

## Diagnosing issues

Create `mono_log.txt` in the Master Duel folder and inside the `YgoMaster` folder. This will log all mono messages in these files which you can look through to diagnose problems with mono.

## Where the `mono` files come from

The files inside `mono` were copied from the latest mono stable release https://www.mono-project.com/download/stable/#download-win

- `%ProgramFiles%/mono/lib/mono/4.5/` folder was stripped down and copied to the output `mono/lib`. Additionally `mscorlib.dll` was copied from the `4.5` folder into `mono/lib/mono/4.5/mscorlib.dll` as that is where mono wants it
- `%ProgramFiles%/mono/etc/` was copied as-is to the output `mono/etc`
- `%ProgramFiles%/mono/bin/*.dll` was copied as-is to the output `mono/bin`

## Bugs

- `ClientSettings.json` setting `ShowConsole` does not work

# Alternative guide - Running YgoMaster through Proton as a Non-Steam game

Guide by [@Joomsy](https://github.com/Joomsy) (https://github.com/pixeltris/YgoMaster/issues/583)

I'm not a big fan of using WINE for games since it's not really tailored toward them, and this is a much simpler way to go about this than using a launcher like Lutris. For example, this makes it considerably easier to use your existing Master Duel Proton prefix, rather than manually configuring a launcher to look for it.

- [Go through the usual steps to install YgoMaster on Linux.](https://github.com/pixeltris/YgoMaster/blob/master/Docs/Linux.md)
- Open up Steam, and add MonoRun.exe as a game.
- Once added, right-click its entry in your library list, and open its properties.
- In the Launch Options field, enter `%command% YgoMasterClient.exe` (you might also want to give it a proper name while here).
- Once finished there, swap over to the compatibility preferences, and force the use of a specific Proton version (I use GE 10-7, but any version you typically use to play MD should work).
- Optionally, things like gamemode and gamescope will also work with this. For example, I use the ScopeBuddy wrapper for gamescope, as well as gamemode, and my launch options look like this:
```
gamemoderun scb -- %command% YgoMasterClient.exe
```

As an added bonus, you can get Steam artwork for it from here.
https://www.steamgriddb.com/game/5316261
