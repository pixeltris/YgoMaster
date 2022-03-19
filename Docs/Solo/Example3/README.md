Refer to [SoloFileFormat.md](https://github.com/pixeltris/YgoMaster/blob/master/Docs/SoloFileFormat.md) for more info on the json.

In this example there is 1 gate (id 67). It does not exist in the normal game. There are two duels which were also created manually (though their contents are the same as each other).

- `Solo.json` needs to be copied to `/Build/Data/Solo.json`
- `ClientData` needs to be merged with `/Build/Data/ClientData/`
- `SoloDuels` needs to be merged with `/Build/Data/SoloDuels/`

### Gate 67

```
[ ]-[🏁]
```

### ClientData/SoloGateCards.txt

These are the card images of the gates. In this case `67,4027,1.03,0`

- `67` is the gate id
- `4027` is the card id
- `1.03` is the Y offset to move the card image up / down
- `0` is the X offset to move the card image left / right

### ClientData/SoloGateBackgrounds/

These are the background images of the gates which is shown when you enter the gate.

### ClientData/IDS/IDS_SOLO.txt

This contains the text of the gates and the chapters (the names / descriptions)