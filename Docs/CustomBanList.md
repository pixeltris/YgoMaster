## Custom ban list tutorial by [@RedRobin16](https://github.com/RedRobin16)

- Open `YgoMaster/Data/RegulationInfo.json`
- Search for `IDS_CARDMENU_REGULATION_NORMAL`, copy the value to the left of that
- Open `Regulation.json` and search for the value you took from `RegulationInfo.json`
- Scroll Down until you find `"a0":` and some numbers, look at the following key below
  - `a0` = forbidden
  - `a1` = limited
  - `a2` = semi-limited
  - `a3` = no longer on the list (you can have card IDs here or just delete any card IDs in this section)
- The numbers you will see under these sections in `Regulation.json` are card IDs [(use this spreadsheet to find card IDs by name)](https://docs.google.com/spreadsheets/d/1IXpwCaabi47Ly8dAf4aJtFdYwi29yGkCJ4bvMhnsvc8/edit#gid=1081892055)
- In `Regulation.json` paste the card ID number like the other numbers you see
  - You can do this for sections `a0`, `a1`, `a2`, or `a3`
- You can also delete cards from a ban list by deleting card IDs from the given section
- Save the file and reopen `YgoMaster.exe` / `YgoMasterClient.exe` to see the updated ban list

## Extra info

- The value beside `IDS_CARDMENU_REGULATION_NORMAL` in `RegulationInfo.json` will change overtime, so be sure to double check both `RegulationInfo.json` and `Regulation.json` for the correct value number (value number should be the same)

## Official Yu-Gi-Oh! ban list

https://www.yugioh-card.com/en/limited/