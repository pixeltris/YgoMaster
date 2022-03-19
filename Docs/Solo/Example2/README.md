Refer to [SoloFileFormat.md](https://github.com/pixeltris/YgoMaster/blob/master/Docs/SoloFileFormat.md) for more info on the json.

In this example there are 3 gates. Completing gate 10 unlocks gates 11 / 12. The goal of gate 12 has a "mate base" as the reward for the loaner deck.

The gates / chapters / duels already exist in the data files so just replace `/Build/Data/Solo.json` with the `Solo.json` in this folder.

### Gate 10

```
    [ ]
   /
[ ]-[🏁]
```

### Gate 11

```
[ ]-[ ]-[🏁]
```

### Gate 12

```
    [🔒]-[ ]
   /
[ ]-[🏁]
```