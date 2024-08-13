# BepInEx

BepInEx is a modding framework for Unity games and there are some good plugins like UnityExplorer. YgoMaster doesn't use BepInEx but there are cases when you may want to run them together.

.NET Framework / .NET seem to conflict with each other when running in the same process (at least in the case of Master Duel + BepInEx + YgoMaster).

To get around this you can use the mono runtime with YgoMaster.

## Instructions

- Grab the zip from [Linux.md](Linux.md)
- Extract / merge it into your YgoMaster folder
- Run `YgoMasterClient.exe` / `YgoMaster.exe` as you normally would

Your folder structure should look like this:

```md
YgoMaster
├── Data/
├── mono/
├── MonoRun.exe
├── YgoMasterClient.exe
├── YgoMaster.exe
└── YgoMasterLoader.dll
```

`mono` is the new folder you introduced. YgoMaster checks for this folder and runs with the mono files inside if it exists. You don't need to worry about `MonoRun.exe` as it's for Linux / running under Wine.

To stop using mono you can delete / rename the `mono` folder.