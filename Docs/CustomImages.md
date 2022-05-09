## Unity asset file paths

File paths in `LocalData` are determined crc32 of the file path which you can calculate online https://emn178.github.io/online-tools/crc32.html

The first two characters of the crc determines the folder name for the file.

A few examples:

- `Card/Images/Illust/tcg/10544` = `cd/cdcc181b`
- `Images/WallPaper/WallPaper0001/WallPaperThumb1130001` = `c8/c89a3c42`
- `Images/DeckCase/HighEnd_HD/DeckCase0001_L` = `73/732381bb`

The casing of the file path is important. For the most part you can use AssetStudio to find the file paths on disk.

If you want to create new files which don't already exist in the game you can use the crc method as described above to determine where the given file should be placed. There are also a few commands `locate` / `locateraw` / `crc` in YgoMasterClient which can be used to crc file paths (enable `ShowConsole` in `ClientSettings.json`). [Read the code for more info on what these do](https://github.com/pixeltris/YgoMaster/blob/83f53fe6cc3f38cbdedda1f88a49d6bd0bfda423/YgoMasterClient/Program.cs#L368-L420).

The following can be used to create / modify unity assets:

- https://github.com/nesrak1/UABEA
- https://github.com/Igor55x/UAAE

Check out this comprehensive asset modding guide https://www.nexusmods.com/yugiohmasterduel/articles/3

## ClientData

`/Build/Data/ClientData` can be used to override images. If you enable `AssetHelperDump` in `ClientSettings.json` it will dump all accessed images into `Build/Data/ClientDataDump` and if you copy any given file over to `Build/Data/ClientData` (using the same folder structure) then the game will load that instead. This allows you to add custom images by simply modifying pngs. Currently it this loads synchronously which can lag a little, and there are issues with the texture loader resulting in rapid increases in memory. So generally avoid using this method, or use it sparingly.