It's possible to add cards to game before they are officially released as packs.

- Run `ygomaster --missing-cards`. This will create `missing-cards.txt` which contains a list of card ids
- Add the desired card ids to `/Data/CardList.json` and `CardCraftableList.json`
- For each card id add an image to `Data/ClientData/Card/Images/Illust/tcg/XXXXX.png` where XXXXX is the given card id