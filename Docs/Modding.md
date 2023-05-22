# Modding

There isn't any documentation for this at the moment. But you can learn by example by looking through https://github.com/pixeltris/YgoMaster/issues/1

Without any specialized tools it's possible to:

- Change card images.
- Add some missing cards (they need to already be in the data files).
- Add custom packs (images, text).
- Add custom structure decks (contents, text).
- Add custom solo content (gates, chapters, text, images) (but no scenario modifications).
- Add linear progression of pack openings (i.e. progression series).

There are currently issues with the custom content loader. It eats ram with larger images and slows down the game while loading (no async loading).

An alternative to this is manually creating unity files and placing them in the right location. Setting `ShowConsole` / `AssetHelperLog` to true in `ClientSettings.json` can be used to find files, and the command `crc` to locate where a file should be placed on disk from it's input path. `AssetHelperDump` will also dump image files into `/Data/ClientDataDump/` as they are loaded.