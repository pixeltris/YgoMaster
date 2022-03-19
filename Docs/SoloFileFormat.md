## Solo.json file format

Related enums:

- [Categories (numbered 0-14)](https://github.com/pixeltris/YgoMaster/blob/d1582097f30fc73e9827276010c9ecbcbf26d339/YgoMaster/Enums/ItemID.cs#L153-L170)
- [Item ids](https://github.com/pixeltris/YgoMaster/blob/d1582097f30fc73e9827276010c9ecbcbf26d339/YgoMaster/Enums/ItemID.cs#L172-L430)

Related code:

- [Act_Solo.cs](https://github.com/pixeltris/YgoMaster/blob/05ee8db136b168ab7473b9d92c2714b57a09c708/YgoMaster/Acts/Act_Solo.cs#L125-L649)
- [SoloInfo.cs](https://github.com/pixeltris/YgoMaster/blob/05ee8db136b168ab7473b9d92c2714b57a09c708/YgoMaster/Infos/SoloInfo.cs#L523-L786)

Chapter icon:

- begin_sn(!0) - scenario
- unlock_id(!0) - lock
- npc_id(0) - reward
- npc_id(!0) + mydeck_set_id(!0) - duel
- npc_id(!0) + mydeck_set_id(0) + set_id(!0) - practice

```
{
  "Master": {
    "Solo": {
      "gate": {// list of all gates
        "1": {// gate id (determines the name "IDS_SOLO.GATE001" which is "Tutorial" and description "IDS_SOLO.GATE001_EXPLANATION" which is "Let's learn the basics of Yu-Gi-Oh! MASTER DUEL.")
          "priority": 1,// sort order (gates visually appear in the list based on their priority)
          "parent_gate": 0,// parent gate id - used for gates which are children of other gates
          "unlock_id": 0,// unlock id which needs to be completed to unlock the current gate
          "view_gate": 0,// gate id which needs to be complete to show the current gate
          "clear_chapter": 10003// chapter id which will be flagged as the goal for this gate
        }
      },
      "chapter": {// list of all chapters which belong to the gates listed above
        "1": {// gate id
          "10001": {// chapter id
            "parent_chapter": 0,// parent chapter id (visually link the chapters together)
            "mydeck_set_id": 0,// the "reward" id for the user deck (0 means the user deck is not available) (reward ids can be shared)
            "set_id": 110,// the "reward" id for the loaner deck (0 means the loaner deck is not available) (reward ids can be shared)
            "unlock_id": 0,// states the unlock requirement id (points into "unlock", for "ITEM" entries follow the id into "unlock_item", else (CHAPTER_OR/CHAPTER_AND) the ids are chapters) (unlock ids can be shared)
            "begin_sn": "",// the scenario script (story)
            "npc_id": 1000// not sure of the importance of this, but required for duels. any value above 0 should be fine (doesn't need to be unique)
          },
          "10002": {
            "parent_chapter": 10001,
            "mydeck_set_id": 0,
            "set_id": 1120001,
            "unlock_id": 0,
            "begin_sn": "",
            "npc_id": 1001
          },
          "10003": {
            "parent_chapter": 10002,
            "mydeck_set_id": 0,
            "set_id": 110,
            "unlock_id": 0,
            "begin_sn": "",
            "npc_id": 1074
          }
        }
      },
      "unlock": {
        "1": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            10003// chapter id
          ]
        },
        "2": {
          "4": [// ChapterUnlockType.CHAPTER_AND
            50021,
            60025
          ]
        },
        "3": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            70037
          ]
        },
        "4": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            90049
          ]
        },
        "5": {
          "4": [// ChapterUnlockType.CHAPTER_AND
            70037,
            90049
          ]
        },
        "6": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            110061
          ]
        },
        "7": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            130073
          ]
        },
        "8": {
          "4": [// ChapterUnlockType.CHAPTER_AND
            110061,
            130073
          ]
        },
        "9": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            140079
          ]
        },
        "10": {
          "2": [// ChapterUnlockType.CHAPTER_OR
            20120
          ]
        },
        "11": {
          "4": [// ChapterUnlockType.CHAPTER_AND
            140079,
            20120
          ]
        },
        "12": {
          "4": [// ChapterUnlockType.CHAPTER_AND
            160091,
            170099,
            180103,
            190109
          ]
        },
        "10101": {
          "3": [// ChapterUnlockType.ITEM
            101// id which points into the "unlock_item" list
          ]
        },
        "10201": {
          "3": [// ChapterUnlockType.ITEM
            201
          ]
        },
        "10301": {
          "3": [// ChapterUnlockType.ITEM
            301
          ]
        },
        "10401": {
          "3": [// ChapterUnlockType.ITEM
            401
          ]
        },
        "10501": {
          "3": [// ChapterUnlockType.ITEM
            501
          ]
        },
        "10601": {
          "3": [// ChapterUnlockType.ITEM
            601
          ]
        },
        "10701": {
          "3": [// ChapterUnlockType.ITEM
            701
          ]
        },
        "10801": {
          "3": [// ChapterUnlockType.ITEM
            801
          ]
        },
        "10802": {
          "3": [// ChapterUnlockType.ITEM
            802
          ]
        },
        "10803": {
          "3": [// ChapterUnlockType.ITEM
            803
          ]
        },
        "10804": {
          "3": [// ChapterUnlockType.ITEM
            804
          ]
        },
        "10805": {
          "3": [// ChapterUnlockType.ITEM
            805
          ]
        },
        "10806": {
          "3": [// ChapterUnlockType.ITEM
            806
          ]
        },
        "10901": {
          "3": [// ChapterUnlockType.ITEM
            901
          ]
        },
        "10902": {
          "3": [// ChapterUnlockType.ITEM
            902
          ]
        },
        "10903": {
          "3": [// ChapterUnlockType.ITEM
            903
          ]
        },
        "10904": {
          "3": [// ChapterUnlockType.ITEM
            904
          ]
        },
        "11001": {
          "3": [// ChapterUnlockType.ITEM
            1001
          ]
        },
        "11002": {
          "3": [// ChapterUnlockType.ITEM
            1002
          ]
        },
        "11003": {
          "3": [// ChapterUnlockType.ITEM
            1003
          ]
        },
        "11004": {
          "3": [// ChapterUnlockType.ITEM
            1004
          ]
        },
        "11101": {
          "3": [// ChapterUnlockType.ITEM
            1101
          ]
        },
        "11102": {
          "3": [// ChapterUnlockType.ITEM
            1102
          ]
        },
        "11103": {
          "3": [// ChapterUnlockType.ITEM
            1103
          ]
        },
        "11104": {
          "3": [// ChapterUnlockType.ITEM
            1104
          ]
        },
        "11201": {
          "3": [// ChapterUnlockType.ITEM
            1201
          ]
        },
        "11202": {
          "3": [// ChapterUnlockType.ITEM
            1202
          ]
        },
        "11203": {
          "3": [// ChapterUnlockType.ITEM
            1203
          ]
        },
        "11204": {
          "3": [// ChapterUnlockType.ITEM
            1204
          ]
        },
        "11301": {
          "3": [// ChapterUnlockType.ITEM
            1301
          ]
        },
        "11302": {
          "3": [// ChapterUnlockType.ITEM
            1302
          ]
        },
        "11303": {
          "3": [// ChapterUnlockType.ITEM
            1303
          ]
        },
        "11304": {
          "3": [// ChapterUnlockType.ITEM
            1304
          ]
        },
        "12001": {
          "3": [// ChapterUnlockType.ITEM
            2001
          ]
        },
        "12002": {
          "3": [// ChapterUnlockType.ITEM
            2002
          ]
        },
        "12003": {
          "3": [// ChapterUnlockType.ITEM
            2003
          ]
        },
        "12004": {
          "3": [// ChapterUnlockType.ITEM
            2004
          ]
        },
        "12005": {
          "3": [// ChapterUnlockType.ITEM
            2005
          ]
        },
        "12006": {
          "3": [// ChapterUnlockType.ITEM
            2006
          ]
        },
        "12007": {
          "3": [// ChapterUnlockType.ITEM
            2007
          ]
        },
        "12008": {
          "3": [// ChapterUnlockType.ITEM
            2008
          ]
        }
      },
      "unlock_item": {
        "101": {
          "1": {// Category.CONSUME
            "1": 100
          }
        },
        "201": {
          "1": {// Category.CONSUME
            "2": 100
          }
        },
        "301": {
          "1": {// Category.CONSUME
            "3": 1
          }
        },
        "401": {
          "1": {// Category.CONSUME
            "4": 1
          }
        },
        "501": {
          "1": {// Category.CONSUME
            "5": 1
          }
        },
        "601": {
          "1": {// Category.CONSUME
            "6": 1
          }
        },
        "701": {
          "1": {// Category.CONSUME
            "7": 100
          }
        },
        "801": {
          "1": {// Category.CONSUME
            "8": 50// ORB-DARK
          }
        },
        "802": {
          "1": {// Category.CONSUME
            "8": 100// ORB-DARK
          }
        },
        "803": {
          "1": {// Category.CONSUME
            "8": 150// ORB-DARK
          }
        },
        "804": {
          "1": {// Category.CONSUME
            "8": 200// ORB-DARK
          }
        },
        "805": {
          "1": {// Category.CONSUME
            "8": 250// ORB-DARK
          }
        },
        "806": {
          "1": {// Category.CONSUME
            "8": 300// ORB-DARK
          }
        },
        "901": {
          "1": {// Category.CONSUME
            "9": 50// ORB-LIGHT
          }
        },
        "902": {
          "1": {// Category.CONSUME
            "9": 100// ORB-LIGHT
          }
        },
        "903": {
          "1": {// Category.CONSUME
            "9": 150// ORB-LIGHT
          }
        },
        "904": {
          "1": {// Category.CONSUME
            "9": 200// ORB-LIGHT
          }
        },
        "1001": {
          "1": {// Category.CONSUME
            "10": 50// ORB-EARTH
          }
        },
        "1002": {
          "1": {// Category.CONSUME
            "10": 100// ORB-EARTH
          }
        },
        "1003": {
          "1": {// Category.CONSUME
            "10": 150// ORB-EARTH
          }
        },
        "1004": {
          "1": {// Category.CONSUME
            "10": 200// ORB-EARTH
          }
        },
        "1101": {
          "1": {// Category.CONSUME
            "11": 50// ORB-WATER
          }
        },
        "1102": {
          "1": {// Category.CONSUME
            "11": 100// ORB-WATER
          }
        },
        "1103": {
          "1": {// Category.CONSUME
            "11": 150// ORB-WATER
          }
        },
        "1104": {
          "1": {// Category.CONSUME
            "11": 200// ORB-WATER
          }
        },
        "1201": {
          "1": {// Category.CONSUME
            "12": 50// ORB-FIRE
          }
        },
        "1202": {
          "1": {// Category.CONSUME
            "12": 100// ORB-FIRE
          }
        },
        "1203": {
          "1": {// Category.CONSUME
            "12": 150// ORB-FIRE
          }
        },
        "1204": {
          "1": {// Category.CONSUME
            "12": 200// ORB-FIRE
          }
        },
        "1301": {
          "1": {// Category.CONSUME
            "13": 50// ORB-WIND
          }
        },
        "1302": {
          "1": {// Category.CONSUME
            "13": 100// ORB-WIND
          }
        },
        "1303": {
          "1": {// Category.CONSUME
            "13": 150// ORB-WIND
          }
        },
        "1304": {
          "1": {// Category.CONSUME
            "13": 200// ORB-WIND
          }
        },
        "2001": {
          "1": {// Category.CONSUME
            "9": 100,// ORB-LIGHT
            "13": 100// ORB-WIND
          }
        },
        "2002": {
          "1": {// Category.CONSUME
            "8": 100,// ORB-DARK
            "10": 100// ORB-EARTH
          }
        },
        "2003": {
          "1": {// Category.CONSUME
            "10": 100,// ORB-EARTH
            "12": 100// ORB-FIRE
          }
        },
        "2004": {
          "1": {// Category.CONSUME
            "8": 100,// ORB-DARK
            "12": 100// ORB-FIRE
          }
        },
        "2005": {
          "1": {// Category.CONSUME
            "9": 200,// ORB-LIGHT
            "11": 200,// ORB-WATER
            "13": 200// ORB-WIND
          }
        },
        "2006": {
          "1": {// Category.CONSUME
            "8": 100,// ORB-DARK
            "9": 100// ORB-LIGHT
          }
        },
        "2007": {
          "1": {// Category.CONSUME
            "12": 200,// ORB-FIRE
            "13": 200// ORB-WIND
          }
        },
        "2008": {
          "1": {// Category.CONSUME
            "8": 200,// ORB-DARK
            "9": 200,// ORB-LIGHT
            "10": 150,// ORB-EARTH
            "11": 150,// ORB-WATER
            "12": 150,// ORB-FIRE
            "13": 150// ORB-WIND
          }
        }
      },
      "reward": {
        "101": {// reward id (this can be any unique number. the reward id can be used by multiple chapters)
          "1": {// Category.CONSUME
            "1": 20// GEM * 20
          }
        },
        "102": {
          "1": {// Category.CONSUME
            "1": 40// GEM * 40
          }
        },
        "103": {
          "1": {// Category.CONSUME
            "1": 100// GEM * 100
          }
        },
        "104": {
          "1": {// Category.CONSUME
            "1": 200// GEM * 200
          }
        },
        "105": {
          "1": {// Category.CONSUME
            "1": 250// GEM * 250
          }
        },
        "106": {
          "1": {// Category.CONSUME
            "1": 300// GEM * 300
          }
        },
        "107": {
          "1": {// Category.CONSUME
            "1": 350// GEM * 350
          }
        },
        "108": {
          "1": {// Category.CONSUME
            "1": 400// GEM * 400
          }
        },
        "109": {
          "1": {// Category.CONSUME
            "1": 450// GEM * 450
          }
        },
        "110": {
          "1": {// Category.CONSUME
            "1": 500// GEM * 500
          }
        },
        "111": {
          "1": {// Category.CONSUME
            "1": 550// GEM * 550
          }
        },
        "112": {
          "1": {// Category.CONSUME
            "1": 600// GEM * 600
          }
        },
        "113": {
          "1": {// Category.CONSUME
            "1": 650// GEM * 650
          }
        },
        "114": {
          "1": {// Category.CONSUME
            "1": 700// GEM * 700
          }
        },
        "115": {
          "1": {// Category.CONSUME
            "1": 750// GEM * 750
          }
        },
        "116": {
          "1": {// Category.CONSUME
            "1": 800// GEM * 800
          }
        },
        "117": {
          "1": {// Category.CONSUME
            "1": 850// GEM * 850
          }
        },
        "118": {
          "1": {// Category.CONSUME
            "1": 900// GEM * 900
          }
        },
        "119": {
          "1": {// Category.CONSUME
            "1": 950// GEM * 950
          }
        },
        "120": {
          "1": {// Category.CONSUME
            "1": 1000// GEM * 1000
          }
        },
        "301": {
          "1": {// Category.CONSUME
            "3": 1// CP-N * 1
          }
        },
        "401": {
          "1": {// Category.CONSUME
            "4": 1// CP-R * 1
          }
        },
        "501": {
          "1": {// Category.CONSUME
            "5": 1// CP-SR * 1
          }
        },
        "601": {
          "1": {// Category.CONSUME
            "6": 1// CP-UR * 1
          }
        },
        "801": {
          "1": {// Category.CONSUME
            "8": 50// ORB-DARK * 50
          }
        },
        "802": {
          "1": {// Category.CONSUME
            "8": 100// ORB-DARK * 100
          }
        },
        "803": {
          "1": {// Category.CONSUME
            "8": 150// ORB-DARK * 150
          }
        },
        "804": {
          "1": {// Category.CONSUME
            "8": 200// ORB-DARK * 200
          }
        },
        "805": {
          "1": {// Category.CONSUME
            "8": 250// ORB-DARK * 250
          }
        },
        "901": {
          "1": {// Category.CONSUME
            "9": 50// ORB-LIGHT * 50
          }
        },
        "902": {
          "1": {// Category.CONSUME
            "9": 100// ORB-LIGHT * 100
          }
        },
        "903": {
          "1": {// Category.CONSUME
            "9": 150// ORB-LIGHT * 150
          }
        },
        "904": {
          "1": {// Category.CONSUME
            "9": 200// ORB-LIGHT * 200
          }
        },
        "905": {
          "1": {// Category.CONSUME
            "9": 250// ORB-LIGHT * 250
          }
        },
        "1001": {
          "1": {// Category.CONSUME
            "10": 50// ORB-EARTH * 50
          }
        },
        "1002": {
          "1": {// Category.CONSUME
            "10": 100// ORB-EARTH * 100
          }
        },
        "1003": {
          "1": {// Category.CONSUME
            "10": 150// ORB-EARTH * 150
          }
        },
        "1004": {
          "1": {// Category.CONSUME
            "10": 200// ORB-EARTH * 200
          }
        },
        "1005": {
          "1": {// Category.CONSUME
            "10": 250// ORB-EARTH * 250
          }
        },
        "1101": {
          "1": {// Category.CONSUME
            "11": 50// ORB-WATER * 50
          }
        },
        "1102": {
          "1": {// Category.CONSUME
            "11": 100// ORB-WATER * 100
          }
        },
        "1103": {
          "1": {// Category.CONSUME
            "11": 150// ORB-WATER * 150
          }
        },
        "1104": {
          "1": {// Category.CONSUME
            "11": 200// ORB-WATER * 200
          }
        },
        "1105": {
          "1": {// Category.CONSUME
            "11": 250// ORB-WATER * 250
          }
        },
        "1201": {
          "1": {// Category.CONSUME
            "12": 50// ORB-FIRE * 50
          }
        },
        "1202": {
          "1": {// Category.CONSUME
            "12": 100// ORB-FIRE * 100
          }
        },
        "1203": {
          "1": {// Category.CONSUME
            "12": 150// ORB-FIRE * 150
          }
        },
        "1204": {
          "1": {// Category.CONSUME
            "12": 200// ORB-FIRE * 200
          }
        },
        "1205": {
          "1": {// Category.CONSUME
            "12": 250// ORB-FIRE * 250
          }
        },
        "1301": {
          "1": {// Category.CONSUME
            "13": 50// ORB-WIND * 50
          }
        },
        "1302": {
          "1": {// Category.CONSUME
            "13": 100// ORB-WIND * 100
          }
        },
        "1303": {
          "1": {// Category.CONSUME
            "13": 150// ORB-WIND * 150
          }
        },
        "1304": {
          "1": {// Category.CONSUME
            "13": 200// ORB-WIND * 200
          }
        },
        "1305": {
          "1": {// Category.CONSUME
            "13": 250// ORB-WIND * 250
          }
        },
        "4343": {
          "2": {// Category.CARD
            "4343": 1// "Raigeki" * 1
          }
        },
        "4416": {
          "2": {// Category.CARD
            "4416": 3// "Karakuri Spider" * 3
          }
        },
        "4842": {
          "2": {// Category.CARD
            "4842": 1// "Monster Reborn" * 1
          }
        },
        "5328": {
          "2": {// Category.CARD
            "5328": 1// "Reinforcement of the Army" * 1
          }
        },
        "5910": {
          "2": {// Category.CARD
            "5910": 3// "Earth Chant" * 3
          }
        },
        "6117": {
          "2": {// Category.CARD
            "6117": 3// "Howling Insect" * 3
          }
        },
        "6692": {
          "2": {// Category.CARD
            "6692": 3// "Herald of Purple Light" * 3
          }
        },
        "6693": {
          "2": {// Category.CARD
            "6693": 3// "Herald of Green Light" * 3
          }
        },
        "6705": {
          "2": {// Category.CARD
            "6705": 3// "Celestial Transformation" * 3
          }
        },
        "6900": {
          "2": {// Category.CARD
            "6900": 3// "Ritual Foregone" * 3
          }
        },
        "7008": {
          "2": {// Category.CARD
            "7008": 3// "Eliminating the League" * 3
          }
        },
        "7591": {
          "2": {// Category.CARD
            "7591": 3// "Jain, Lightsworn Paladin" * 3
          }
        },
        "7593": {
          "2": {// Category.CARD
            "7593": 3// "Garoth, Lightsworn Warrior" * 3
          }
        },
        "7643": {
          "2": {// Category.CARD
            "7643": 3// "Light Spiral" * 3
          }
        },
        "7645": {
          "2": {// Category.CARD
            "7645": 3// "Destruction Jammer" * 3
          }
        },
        "7723": {
          "2": {// Category.CARD
            "7723": 3// "Jenis, Lightsworn Mender" * 3
          }
        },
        "8898": {
          "2": {// Category.CARD
            "8898": 3// "Gem-Merchant" * 3
          }
        },
        "9114": {
          "2": {// Category.CARD
            "9114": 3// "Delg the Dark Monarch" * 3
          }
        },
        "9285": {
          "2": {// Category.CARD
            "9285": 3// "Karakuri Barrel mdl 96 "Shinkuro"" * 3
          }
        },
        "9647": {
          "2": {// Category.CARD
            "9647": 3// "Geargiano" * 3
          }
        },
        "9769": {
          "2": {// Category.CARD
            "9769": 3// "Photon Lead" * 3
          }
        },
        "9793": {
          "2": {// Category.CARD
            "9793": 3// "Darklight" * 3
          }
        },
        "10157": {
          "2": {// Category.CARD
            "10157": 3// "Geargiano Mk-II" * 3
          }
        },
        "10505": {
          "2": {// Category.CARD
            "10505": 3// "Mecha Phantom Beast Stealthray" * 3
          }
        },
        "10628": {
          "2": {// Category.CARD
            "10628": 3// "Mecha Phantom Beast Harrliard" * 3
          }
        },
        "10661": {
          "2": {// Category.CARD
            "10661": 3// "Herald of Pure Light" * 3
          }
        },
        "10758": {
          "2": {// Category.CARD
            "10758": 3// "Mecha Phantom Beast Sabre Hawk" * 3
          }
        },
        "10883": {
          "2": {// Category.CARD
            "10883": 3// "Geargiattacker" * 3
          }
        },
        "10954": {
          "2": {// Category.CARD
            "10954": 3// "Xyz Override" * 3
          }
        },
        "11204": {
          "2": {// Category.CARD
            "11204": 3// "Escalation of the Monarchs" * 3
          }
        },
        "11357": {
          "2": {// Category.CARD
            "11357": 3// "Qliphort Shell" * 3
          }
        },
        "11532": {
          "2": {// Category.CARD
            "11532": 3// "Gem-Knight Lapis" * 3
          }
        },
        "11541": {
          "2": {// Category.CARD
            "11541": 3// "Qliphort Cephalopod" * 3
          }
        },
        "11559": {
          "2": {// Category.CARD
            "11559": 3// "Frontline Observer" * 3
          }
        },
        "11563": {
          "2": {// Category.CARD
            "11563": 3// "Marmiting Captain" * 3
          }
        },
        "11728": {
          "2": {// Category.CARD
            "11728": 3// "Fusion Conscription" * 3
          }
        },
        "12021": {
          "2": {// Category.CARD
            "12021": 3// "Urgent Ritual Art" * 3
          }
        },
        "12132": {
          "2": {// Category.CARD
            "12132": 3// "Dinomist Stegosaur" * 3
          }
        },
        "12133": {
          "2": {// Category.CARD
            "12133": 3// "Dinomist Plesios" * 3
          }
        },
        "12135": {
          "2": {// Category.CARD
            "12135": 3// "Dinomist Brachion" * 3
          }
        },
        "12336": {
          "2": {// Category.CARD
            "12336": 3// "Bug Matrix" * 3
          }
        },
        "12345": {
          "2": {// Category.CARD
            "12345": 3// "Dinomist Eruption" * 3
          }
        },
        "13060": {
          "2": {// Category.CARD
            "13060": 3// "World Chalice Guardragon" * 3
          }
        },
        "13061": {
          "2": {// Category.CARD
            "13061": 3// "Lee the World Chalice Fairy" * 3
          }
        },
        "13062": {
          "2": {// Category.CARD
            "13062": 3// "World Legacy - "World Chalice"" * 3
          }
        },
        "13063": {
          "2": {// Category.CARD
            "13063": 3// "Jain, Twilightsworn General" * 3
          }
        },
        "13086": {
          "2": {// Category.CARD
            "13086": 3// "Imduk the World Chalice Dragon" * 3
          }
        },
        "13087": {
          "2": {// Category.CARD
            "13087": 3// "Ib the World Chalice Priestess" * 3
          }
        },
        "13088": {
          "2": {// Category.CARD
            "13088": 3// "Auram the World Chalice Blademaster" * 3
          }
        },
        "13089": {
          "2": {// Category.CARD
            "13089": 3// "Ningirsu the World Chalice Warrior" * 3
          }
        },
        "13096": {
          "2": {// Category.CARD
            "13096": 3// "World Legacy Discovery" * 3
          }
        },
        "13097": {
          "2": {// Category.CARD
            "13097": 3// "World Legacy's Heart" * 3
          }
        },
        "13110": {
          "2": {// Category.CARD
            "13110": 3// "World Legacy Landmark" * 3
          }
        },
        "13238": {
          "2": {// Category.CARD
            "13238": 3// "World Legacy - "World Armor"" * 3
          }
        },
        "13251": {
          "2": {// Category.CARD
            "13251": 3// "Self-Destruct Ant" * 3
          }
        },
        "13274": {
          "2": {// Category.CARD
            "13274": 3// "World Legacy Clash" * 3
          }
        },
        "13290": {
          "2": {// Category.CARD
            "13290": 3// "World Legacy Trap Globe" * 3
          }
        },
        "13391": {
          "2": {// Category.CARD
            "13391": 3// "World Legacy - "World Shield"" * 3
          }
        },
        "13570": {
          "2": {// Category.CARD
            "13570": 3// "Mekk-Knight Avram" * 3
          }
        },
        "13577": {
          "2": {// Category.CARD
            "13577": 3// "Elementsaber Lapauila" * 3
          }
        },
        "13578": {
          "2": {// Category.CARD
            "13578": 3// "Elementsaber Molehu" * 3
          }
        },
        "13580": {
          "2": {// Category.CARD
            "13580": 3// "Phosphorage the Elemental Lord" * 3
          }
        },
        "13611": {
          "2": {// Category.CARD
            "13611": 3// "World Legacy's Corruption" * 3
          }
        },
        "13612": {
          "2": {// Category.CARD
            "13612": 3// "World Legacy Succession" * 3
          }
        },
        "13625": {
          "2": {// Category.CARD
            "13625": 3// "World Legacy Awakens" * 3
          }
        },
        "13735": {
          "2": {// Category.CARD
            "13735": 3// "Umbramirage the Elemental Lord" * 3
          }
        },
        "13963": {
          "2": {// Category.CARD
            "13963": 3// "Invincibility Barrier" * 3
          }
        },
        "14845": {
          "2": {// Category.CARD
            "14845": 3// "Megalith Hagith" * 3
          }
        },
        "14846": {
          "2": {// Category.CARD
            "14846": 3// "Megalith Och" * 3
          }
        },
        "14847": {
          "2": {// Category.CARD
            "14847": 3// "Megalith Phaleg" * 3
          }
        },
        "15556": {
          "2": {// Category.CARD
            "15556": 3// "Free-Range Monsters" * 3
          }
        },
        "205537": {
          "2": {
            "205537": 1
          }
        },
        "206161": {
          "2": {
            "206161": 1
          }
        },
        "1000002": {
          "3": {// Category.AVATAR
            "1000002": 1// "Ritual Raven" * 1
          }
        },
        "1000004": {
          "3": {// Category.AVATAR
            "1000004": 1// "deleted" * 1
          }
        },
        "1000007": {
          "3": {// Category.AVATAR
            "1000007": 1// "deleted" * 1
          }
        },
        "1000009": {
          "3": {// Category.AVATAR
            "1000009": 1// "Sangan" * 1
          }
        },
        "1000010": {
          "3": {// Category.AVATAR
            "1000010": 1// "World Chalice Guardragon" * 1
          }
        },
        "1000014": {
          "3": {// Category.AVATAR
            "1000014": 1// "Geargiano" * 1
          }
        },
        "1000015": {
          "3": {// Category.AVATAR
            "1000015": 1// "Karakuri Barrel mdl 96 "Shinkuro"" * 1
          }
        },
        "1001004": {
          "3": {// Category.AVATAR
            "1001004": 1// "Qliphort Monolith" * 1
          }
        },
        "1012001": {
          "4": {// Category.ICON
            "1012001": 1// "X-Krawler Synaphysis" * 1
          }
        },
        "1012002": {
          "4": {// Category.ICON
            "1012002": 1// "Mekk-Knight Spectrum Supreme" * 1
          }
        },
        "1012003": {
          "4": {// Category.ICON
            "1012003": 1// "Knightmare Gryphon" * 1
          }
        },
        "1012004": {
          "4": {// Category.ICON
            "1012004": 1// "Digital Bug Rhinosebus" * 1
          }
        },
        "1012005": {
          "4": {// Category.ICON
            "1012005": 1// "Erebus the Underworld Monarch" * 1
          }
        },
        "1012006": {
          "4": {// Category.ICON
            "1012006": 1// "Elementsaber Lapauila Mana" * 1
          }
        },
        "1012007": {
          "4": {// Category.ICON
            "1012007": 1// "Demise, Supreme King of Armageddon" * 1
          }
        },
        "1012008": {
          "4": {// Category.ICON
            "1012008": 1// "Megalith Aratron" * 1
          }
        },
        "1012009": {
          "4": {// Category.ICON
            "1012009": 1// "Gem-Knight Master Diamond" * 1
          }
        },
        "1012010": {
          "4": {// Category.ICON
            "1012010": 1// "Gladiator Beast Domitianus" * 1
          }
        },
        "1012011": {
          "4": {// Category.ICON
            "1012011": 1// "Karakuri Super Shogun mdl 00N "Bureibu"" * 1
          }
        },
        "1012012": {
          "4": {// Category.ICON
            "1012012": 1// "Shiranui Sunsaga" * 1
          }
        },
        "1012013": {
          "4": {// Category.ICON
            "1012013": 1// "Geargiagear Gigant XG" * 1
          }
        },
        "1012014": {
          "4": {// Category.ICON
            "1012014": 1// "Dinomist Rex" * 1
          }
        },
        "1012015": {
          "4": {// Category.ICON
            "1012015": 1// "Apoqliphort Towers" * 1
          }
        },
        "1012016": {
          "4": {// Category.ICON
            "1012016": 1// "The Weather Painter Rainbow" * 1
          }
        },
        "1012017": {
          "4": {// Category.ICON
            "1012017": 1// "Judgment Dragon" * 1
          }
        },
        "1012018": {
          "4": {// Category.ICON
            "1012018": 1// "Punishment Dragon" * 1
          }
        },
        "1012019": {
          "4": {// Category.ICON
            "1012019": 1// "Mecha Phantom Beast Dracossack" * 1
          }
        },
        "1012020": {
          "4": {// Category.ICON
            "1012020": 1// "Herald of Ultimateness" * 1
          }
        },
        "1012021": {
          "4": {// Category.ICON
            "1012021": 1// "Auram the World Chalice Blademaster" * 1
          }
        },
        "1020012": {
          "5": {// Category.PROFILE_TAG
            "1020012": 1// "Strong Arm" * 1
          }
        },
        "1070002": {
          "7": {// Category.PROTECTOR
            "1070002": 1// "Auram the World Chalice Blademaster" * 1
          }
        },
        "1090006": {
          "9": {// Category.FIELD
            "1090006": 1// "World Legacy Ruins" * 1
          }
        },
        "1090007": {
          "9": {// Category.FIELD
            "1090007": 1// "Foreign Capital" * 1
          }
        },
        "1101001": {
          "10": {// Category.FIELD_OBJ
            "1101001": 1// "Trap Hole" * 1
          }
        },
        "1101002": {
          "10": {// Category.FIELD_OBJ
            "1101002": 1// "Magical Hats" * 1
          }
        },
        "1111001": {
          "11": {// Category.AVATAR_HOME
            "1111001": 1// "Destiny Board" * 1
          }
        },
        "1111002": {
          "11": {// Category.AVATAR_HOME
            "1111002": 1// "Bug Matrix" * 1
          }
        },
        "1120001": {
          "12": {// Category.STRUCTURE
            "1120001": 1// "Starting Deck" * 1
          }
        },
        "1120008": {
          "12": {// Category.STRUCTURE
            "1120008": 1// "Cybernetic Successor" * 1
          }
        },
        "1120009": {
          "12": {// Category.STRUCTURE
            "1120009": 1// "Heir to the Shiranui-Style" * 1
          }
        },
        "1120010": {
          "12": {// Category.STRUCTURE
            "1120010": 1// "Growing Digital Bug" * 1
          }
        },
        "1120011": {
          "12": {// Category.STRUCTURE
            "1120011": 1// "Roar of the Gladiator Beasts" * 1
          }
        },
        "1120012": {
          "12": {// Category.STRUCTURE
            "1120012": 1// "Emergence of the Monarchs" * 1
          }
        },
        "1120013": {
          "12": {// Category.STRUCTURE
            "1120013": 1// "Being Who Sees the End of the World" * 1
          }
        },
        "1120014": {
          "12": {// Category.STRUCTURE
            "1120014": 1// "Gem-Knights' Resolution" * 1
          }
        },
        "1140101": {
          "14": {// Category.PACK_TICKET
            "1140001": 1// "Legacy Pack ticket" * 1
          }
        },
        "1140102": {
          "14": {// Category.PACK_TICKET
            "1140001": 2// "Legacy Pack ticket" * 2
          }
        },
        "1140103": {
          "14": {// Category.PACK_TICKET
            "1140001": 3// "Legacy Pack ticket" * 3
          }
        },
        "1140104": {
          "14": {// Category.PACK_TICKET
            "1140001": 4// "Legacy Pack ticket" * 4
          }
        }
      }
    }
  }
}
```