# BepInEx

BepInEx is a modding framework for Unity games and there are some good plugins like UnityExplorer. YgoMaster doesn't use BepInEx but there are cases when you may want to run them together.

.NET Framework / .NET seem to conflict with each other when running in the same process (at least in the case of Master Duel + BepInEx + YgoMaster). YgoMaster uses .NET Framework 4.0 as it's pre-installed on all Windows 7+ machines. BepInEx uses .NET which is a different runtime.

To get around this there is code in this repo to make YgoMaster a plugin to BepInEx.

## Running YgoMasterClient + BepInEx

- Copy the following dlls from your BepInEx install to the current directory
  - `BepInEx/core/BepInEx.Core.dll`
  - `BepInEx/core/BepInEx.Unity.IL2CPP.dll`
  - `BepInEx/core/Il2CppInterop.Runtime.dll`
- Run `YgoMasterBepInEx.bat`
- Copy `YgoMasterBepInEx.dll` to your BepInEx plugins folder `BepInEx/plugins/YgoMasterBepInEx/YgoMasterBepInEx.dll`
- In `YgoMaster/Data/ClientData/ClientSettings.json` change `LaunchMode` from `Detours` to `BepInEx`
- Run `YgoMaster/YgoMasterClient.exe` from your Master Duel folder

## Limitations

Some things haven't been supported in BepInEx yet. Mostly it's things that use WinForms including:

- Load deck from file in duel starter UI
- Random deck from folder in duel starter UI
- Most of the custom menu items in the deck editor (load deck from file, copy to clipboard, etc)

Interacting with any of the above will crash the game.

The "live" mode of YgoMaster also currently doesn't work under BepInEx (i.e. you can only start YgoMasterClient.exe directly, if you run the game using Steam it wont load YgoMasterClient)