using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    /// <summary>
    /// YgomGame.TextIDs.IDS_ITEM
    /// </summary>
    static class ItemID
    {
        public static List<int> GetDuelFieldParts(int itemId)
        {
            List<int> result = new List<int>();
            if (ItemID.GetCategoryFromID(itemId) == Category.FIELD)
            {
                Dictionary<Type, int> categories = new Dictionary<Type, int>()
                {
                    { typeof(ItemID.FIELD_OBJ), 1100000 },
                    { typeof(ItemID.AVATAR_HOME), 1110000 },
                };
                foreach (KeyValuePair<Type, int> category in categories)
                {
                    foreach (int value in Enum.GetValues(category.Key))
                    {
                        if (value == (itemId - 1090000) + category.Value)
                        {
                            result.Add(value);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static int GetFieldObjFromField(int fieldId)
        {
            if (fieldId <= 0)
            {
                return (int)FIELD_OBJ.ID1100001;
            }
            return (fieldId - 1090000) + 1100000;
        }

        public static int GetFieldAvatarBaseFromField(int fieldId)
        {
            if (fieldId <= 0)
            {
                return (int)AVATAR_HOME.ID1110001;
            }
            return (fieldId - 1090000) + 1110000;
        }

        public static int GetRandomId(Random rand, Category category)
        {
            Type targetEnum = null;
            switch (category)
            {
                case Category.AVATAR: targetEnum = typeof(AVATAR); break;
                case Category.ICON: targetEnum = typeof(ICON); break;
                case Category.ICON_FRAME: targetEnum = typeof(ICON_FRAME); break;
                case Category.PROTECTOR: targetEnum = typeof(PROTECTOR); break;
                case Category.DECK_CASE: targetEnum = typeof(DECK_CASE); break;
                case Category.FIELD: targetEnum = typeof(FIELD); break;
                case Category.FIELD_OBJ: targetEnum = typeof(FIELD_OBJ); break;
                case Category.AVATAR_HOME: targetEnum = typeof(AVATAR_HOME); break;
                case Category.STRUCTURE: targetEnum = typeof(STRUCTURE); break;
                case Category.WALLPAPER: targetEnum = typeof(WALLPAPER); break;
            }
            if (targetEnum != null)
            {
                Array items = Enum.GetValues(targetEnum);
                if (items.Length > 0)
                {
                    return (int)items.GetValue(rand.Next(items.Length));
                }
            }
            return 0;
        }

        /// <summary>
        /// YgomGame.Utility.ItemUtil.GetCategoryFromID
        /// </summary>
        public static Category GetCategoryFromID(int itemId)
        {
            if (itemId == 0)
            {
                return Category.NONE;
            }
            if (itemId < 3000)
            {
                return Category.CONSUME;
            }
            if (itemId - 3000 < 97000 ||
                itemId - 103000 < 97000 ||
                itemId - 203000 < 97000)
            {
                return Category.CARD;
            }
            if (itemId - 1000000 < 10000)
            {
                return Category.AVATAR;
            }
            if (itemId - 1010000 < 10000)
            {
                return Category.ICON;
            }
            if (itemId - 1020000 < 10000)
            {
                return Category.PROFILE_TAG;
            }
            if (itemId - 1030000 < 10000)
            {
                return Category.ICON_FRAME;
            }
            if (itemId - 1070000 < 10000)
            {
                return Category.PROTECTOR;
            }
            if (itemId - 1080000 < 10000)
            {
                return Category.DECK_CASE;
            }
            if (itemId - 1090000 < 10000)
            {
                return Category.FIELD;
            }
            if (itemId - 1100000 < 10000)
            {
                return Category.FIELD_OBJ;
            }
            if (itemId - 1110000 < 10000)
            {
                return Category.AVATAR_HOME;
            }
            if (itemId - 1120000 < 10000)
            {
                return Category.STRUCTURE;
            }
            if (itemId - 1130000 < 10000)
            {
                return Category.WALLPAPER;
            }
            if (itemId - 1140000 < 10000)
            {
                return Category.PACK_TICKET;
            }
            return 0;
        }

        public enum Category
        {
            NONE,
            CONSUME,
            CARD,
            AVATAR,
            ICON,
            PROFILE_TAG,
            ICON_FRAME,
            PROTECTOR,
            DECK_CASE,
            FIELD,
            FIELD_OBJ,
            AVATAR_HOME,
            STRUCTURE,
            WALLPAPER,
            PACK_TICKET
        }

        public enum NONE
        {
            ID0000 = 0,//Not Set
        }
        public enum CONSUME
        {
            ID0001 = 1,//GEM
            ID0002 = 2,//GEM
            ID0003 = 3,//CP-N
            ID0004 = 4,//CP-R
            ID0005 = 5,//CP-SR
            ID0006 = 6,//CP-UR
            ID0007 = 7,//ORB
            ID0008 = 8,//ORB-DARK
            ID0009 = 9,//ORB-LIGHT
            ID0010 = 10,//ORB-EARTH
            ID0011 = 11,//ORB-WATER
            ID0012 = 12,//ORB-FIRE
            ID0013 = 13,//ORB-WIND
        }
        public enum AVATAR
        {
            ID1000001 = 1000001,//Rescue Rabbit
            ID1000002 = 1000002,//Ritual Raven
            ID1000003 = 1000003,//Dark Magician
            ID1000004 = 1000004,//Dark Magician (Mini)
            ID1000005 = 1000005,//Borreload Dragon (Mini)
            ID1000006 = 1000006,//Cyber Dragon Infinity (Mini)
            ID1000007 = 1000007,//coming soon
            ID1000008 = 1000008,//Ancient Gear Wyvern
            ID1000009 = 1000009,//Sangan
            ID1000010 = 1000010,//World Chalice Guardragon
            ID1000011 = 1000011,//Salamangreat Gazelle
            ID1000012 = 1000012,//Pot of Greed
            ID1000013 = 1000013,//Ash Blossom & Joyous Spring
            ID1000014 = 1000014,//Geargiano
            ID1000015 = 1000015,//Karakuri Barrel mdl 96 "Shinkuro"
            ID1000016 = 1000016,//Wightbaking
            ID1000017 = 1000017,//Ninja Grandmaster Hanzo
            ID1000018 = 1000018,//Ghostrick Lantern
            ID1000019 = 1000019,//coming soon
            ID1000020 = 1000020,//coming soon
            ID1000021 = 1000021,//coming soon
            ID1001001 = 1001001,//Instant Fusion
            ID1001002 = 1001002,//Noble Arms - Caliburn
            ID1001003 = 1001003,//Book of Moon
            ID1001004 = 1001004,//Qliphort Monolith
            ID1001005 = 1001005,//Scapegoat
            ID1001006 = 1001006,//Bitron
            ID1001007 = 1001007,//Moai Interceptor Cannons
            ID1001008 = 1001008,//Mokey Mokey
            ID1001009 = 1001009,//Shard of Greed
            ID1001010 = 1001010,//D/D Ghost
            ID1001011 = 1001011,//Marshmallon
            ID1001012 = 1001012,//Kuriboh
            ID1001013 = 1001013,//Danger!? Jackalope?
            ID1002001 = 1002001,//Soccer Ball
            ID1002002 = 1002002,//Basketball
            ID1002003 = 1002003,//Record Player
            ID1002004 = 1002004,//American Football
            ID1002005 = 1002005,//Rugby
            ID1002006 = 1002006,//Car
            ID1002007 = 1002007,//Treasure Box
            ID1002008 = 1002008,//Boxing
            ID1002009 = 1002009,//Crown
            ID1002010 = 1002010,//Inline Skates
            ID1002011 = 1002011,//Darts
        }
        public enum ICON
        {
            ID1010001 = 1010001,//Duelist
            ID1010002 = 1010002,//SPYRAL Quik-Fix
            ID1010003 = 1010003,//Dante, Pilgrim of the Burning Abyss
            ID1010004 = 1010004,//Condemned Darklord
            ID1010005 = 1010005,//Berserker of the Tenyi
            ID1010006 = 1010006,//Warrior Dai Grepher
            ID1010007 = 1010007,//Danger! Bigfoot!
            ID1010008 = 1010008,//Yuki-Onna, the Absolute Zero Mayakashi
            ID1010009 = 1010009,//Revendread Slayer
            ID1010010 = 1010010,//Kuriboh
            ID1010011 = 1010011,//Black Luster Soldier - Super Soldier
            ID1010012 = 1010012,//Time Thief Redoer
            ID1010013 = 1010013,//Dogmatika Ecclesia, the Virtuous
            ID1010014 = 1010014,//Hela, Generaider Boss of Doom
            ID1010015 = 1010015,//Upstart Goblin
            ID1010016 = 1010016,//Thunder Dragon Colossus
            ID1010017 = 1010017,//True King of All Calamities
            ID1010018 = 1010018,//House Dragonmaid
            ID1010019 = 1010019,//Fallen of Albaz
            ID1011001 = 1011001,//Soccer Ball
            ID1011002 = 1011002,//Trophy
            ID1011003 = 1011003,//Diamond
            ID1012001 = 1012001,//X-Krawler Synaphysis
            ID1012002 = 1012002,//Mekk-Knight Spectrum Supreme
            ID1012003 = 1012003,//Knightmare Gryphon
            ID1012004 = 1012004,//Digital Bug Rhinosebus
            ID1012005 = 1012005,//Erebus the Underworld Monarch
            ID1012006 = 1012006,//Elementsaber Lapauila Mana
            ID1012007 = 1012007,//Demise, Supreme King of Armageddon
            ID1012008 = 1012008,//Megalith Aratron
            ID1012009 = 1012009,//Gem-Knight Master Diamond
            ID1012010 = 1012010,//Gladiator Beast Domitianus
            ID1012011 = 1012011,//Karakuri Super Shogun mdl 00N "Bureibu"
            ID1012012 = 1012012,//Shiranui Sunsaga
            ID1012013 = 1012013,//Geargiagear Gigant XG
            ID1012014 = 1012014,//Dinomist Rex
            ID1012015 = 1012015,//Apoqliphort Towers
            ID1012016 = 1012016,//The Weather Painter Rainbow
            ID1012017 = 1012017,//Judgment Dragon
            ID1012018 = 1012018,//Punishment Dragon
            ID1012019 = 1012019,//Mecha Phantom Beast Dracossack
            ID1012020 = 1012020,//Herald of Ultimateness
            ID1012021 = 1012021,//Auram the World Chalice Blademaster
            ID1012022 = 1012022,//Oneiros, the Dream Mirror Erlking
            ID1012023 = 1012023,//Oneiros, the Dream Mirror Tormentor
            ID1012024 = 1012024,//Crusadia Equimax
            ID1012025 = 1012025,//Longirsu, the Orcust Orchestrator
            ID1012026 = 1012026,//Orcustrion
            ID1012027 = 1012027,//U.A. Playmaker
            ID1012028 = 1012028,//Great Shogun Shien
            ID1012029 = 1012029,//Danger! Nessie!
            ID1012030 = 1012030,//Nephthys, the Sacred Flame
        }
        public enum ICON_FRAME
        {
            ID1030001 = 1030001,//Beginner's Frame
            ID1030002 = 1030002,//SILVER Frame
            ID1030003 = 1030003,//BRONZE Frame
            ID1030004 = 1030004,//GOLD Frame
            ID1030005 = 1030005,//Green Frame
            ID1030006 = 1030006,//Purple Frame
            ID1030007 = 1030007,//PLATINUM Frame
            ID1030008 = 1030008,//DIAMOND Frame
            ID1030009 = 1030009,//Blue Frame
            ID1030010 = 1030010,//Bronze Award Frame
            ID1030011 = 1030011,//Silver Award Frame
            ID1030012 = 1030012,//Gold Award Frame
            //ID1030013 = 1030013,//ICON_FRAME13
            //ID1030014 = 1030014,//ICON_FRAME14
            //ID1030015 = 1030015,//ICON_FRAME15
            //ID1030016 = 1030016,//ICON_FRAME16
            //ID1030017 = 1030017,//ICON_FRAME17
            //ID1030018 = 1030018,//ICON_FRAME18
            //ID1030019 = 1030019,//ICON_FRAME19
            //ID1030020 = 1030020,//ICON_FRAME20
            //ID1030021 = 1030021,//ICON_FRAME21
        }
        public enum PROFILE_TAG
        {
            ID1020001 = 1020001,//Beginner Duelist
            ID1020002 = 1020002,//Fan of the Anime
            ID1020003 = 1020003,//Returnee
            ID1020004 = 1020004,//TCG Player
            ID1020005 = 1020005,//Links Player
            ID1020006 = 1020006,//Quick Play
            ID1020007 = 1020007,//Relaxed Play
            ID1020008 = 1020008,//Casual
            ID1020009 = 1020009,//Stoic
            ID1020010 = 1020010,//Follow Me
            ID1020011 = 1020011,//True Duelist
            ID1020012 = 1020012,//Strong Arm
            ID1020013 = 1020013,//Showoff
            ID1020014 = 1020014,//Tactician
            ID1020015 = 1020015,//Demon
            ID1020016 = 1020016,//Sealed One
            ID1020017 = 1020017,//Destroyer
            ID1020018 = 1020018,//Strategy Expert
            ID1020019 = 1020019,//Unfathomable
            ID1020020 = 1020020,//Synchro Festival 2022
            //ID1020021 = 1020021,//
            ID1020022 = 1020022,//<color=#9933CC>Fusion Festival 2022</color>
            //ID1020023 = 1020023,//
            //ID1020024 = 1020024,//
            //ID1020025 = 1020025,//
            //ID1020026 = 1020026,//
            //ID1020027 = 1020027,//
            //ID1020028 = 1020028,//
            //ID1020029 = 1020029,//
            //ID1020030 = 1020030,//
        }
        public enum PROTECTOR
        {
            ID1070001 = 1070001,//Yu-Gi-Oh! Trading Card Game
            ID1070002 = 1070002,//Auram the World Chalice Blademaster
            ID1070003 = 1070003,//Link Black
            ID1070004 = 1070004,//Fusion Purple
            ID1070005 = 1070005,//Synchro Silver
            ID1070006 = 1070006,//Xyz Black
            ID1070007 = 1070007,//Geartown
            ID1070008 = 1070008,//Ash Blossom & Joyous Spring
            ID1070009 = 1070009,//Blue-Eyes Chaos MAX Dragon
            ID1070010 = 1070010,//Trishula, Dragon of the Ice Barrier
            ID1070011 = 1070011,//Protector Silver
            ID1070012 = 1070012,//Dragonmaid Send-Off
            ID1070013 = 1070013,//Protector - FIRE
            ID1070014 = 1070014,//Protector - WIND
            ID1071001 = 1071001,//Crusadia Maximus
            ID1071002 = 1071002,//Longirsu, the Orcust Orchestrator
            ID1071003 = 1071003,//Dream Mirror Phantasms
            ID1071004 = 1071004,//Six Samurai United
            ID1071005 = 1071005,//Danger! Bigfoot!
            ID1071006 = 1071006,//Nephthys, the Sacred Flame
            ID1071007 = 1071007,//U.A. Playmaker
        }
        public enum DECK_CASE
        {
            ID1080001 = 1080001,//Duelist Card Case Red
            ID1080002 = 1080002,//Fusion Purple
            ID1080003 = 1080003,//Synchro Silver
            ID1080004 = 1080004,//Link Blue
            ID1080005 = 1080005,//Re-Contract Universe
            ID1080006 = 1080006,//Magician of Pendulum
            ID1080007 = 1080007,//Rage of Cipher
            ID1080008 = 1080008,//Cybernetic Successor
            ID1080009 = 1080009,//Heir to the Shiranui-Style
            ID1080010 = 1080010,//Growing Digital Bug
            ID1080011 = 1080011,//Roar of the Gladiator Beasts
            ID1080012 = 1080012,//Emergence of the Monarchs
            ID1080013 = 1080013,//Being Who Sees the End of the World
            ID1080014 = 1080014,//Gem-Knights' Resolution
            //ID1080015 = 1080015,//coming soon
            //ID1080016 = 1080016,//coming soon
            //ID1080017 = 1080017,//coming soon
            ID1082001 = 1082001,//Xyz Black
        }
        public enum FIELD
        {
            ID1090001 = 1090001,//Forest
            ID1090002 = 1090002,//Spellbook Star Hall
            ID1090003 = 1090003,//Ritual Cage
            ID1090004 = 1090004,//Geartown
            ID1090005 = 1090005,//Volcano
            ID1090006 = 1090006,//World Legacy Ruins
            ID1090007 = 1090007,//Foreign Capital
            ID1090008 = 1090008,//Realm of Danger!
            ID1090009 = 1090009,//Night Skyscrapers
            ID1090010 = 1090010,//Glacial World
            ID1090011 = 1090011,//The Desolate Temple
            ID1092001 = 1092001,//Colosseum
        }
        public enum FIELD_OBJ
        {
            ID1100001 = 1100001,//Forest
            ID1100002 = 1100002,//Spellbook Star Hall
            ID1100003 = 1100003,//Ritual Cage
            ID1100004 = 1100004,//Geartown
            ID1100005 = 1100005,//Volcano
            ID1100006 = 1100006,//World Legacy Ruins
            ID1100007 = 1100007,//Foreign Capital
            ID1100008 = 1100008,//Realm of Danger!
            ID1100009 = 1100009,//Night Skyscrapers
            ID1100010 = 1100010,//Glacial World
            ID1100011 = 1100011,//The Desolate Temple
            ID1101001 = 1101001,//Trap Hole
            ID1101002 = 1101002,//Magical Hats
            ID1101003 = 1101003,//Summon Limit
            ID1101004 = 1101004,//Giant Trap Hole
            ID1101005 = 1101005,//TG1-EM1
            ID1101006 = 1101006,//Predaplant Chimerafflesia
            ID1101007 = 1101007,//Contract with the Abyss
            ID1102001 = 1102001,//Colosseum
        }
        public enum AVATAR_HOME
        {
            ID1110001 = 1110001,//Forest
            ID1110002 = 1110002,//Spellbook Star Hall
            ID1110003 = 1110003,//Ritual Cage
            ID1110004 = 1110004,//Geartown
            ID1110005 = 1110005,//Volcano
            ID1110006 = 1110006,//World Legacy Ruins
            ID1110007 = 1110007,//Foreign Capital
            ID1110008 = 1110008,//Realm of Danger!
            ID1110009 = 1110009,//Night Skyscrapers
            ID1110010 = 1110010,//Glacial World
            ID1110011 = 1110011,//The Desolate Temple
            ID1111001 = 1111001,//Destiny Board
            ID1111002 = 1111002,//Bug Matrix
            ID1111003 = 1111003,//Shien's Dojo
            ID1111004 = 1111004,//Flower Gathering
            ID1111005 = 1111005,//Dragonic Tactics
            ID1111006 = 1111006,//Catapult Turtle
            ID1111007 = 1111007,//World Legacy - "World Key"
            ID1112001 = 1112001,//Colosseum
        }
        public enum STRUCTURE
        {
            ID1120001 = 1120001,//Starting Deck
            ID1120002 = 1120002,//Power of the Dragon
            ID1120003 = 1120003,//Synchro of Unity
            ID1120004 = 1120004,//Link Generation
            ID1120005 = 1120005,//Re-Contract Universe
            ID1120006 = 1120006,//Magician of Pendulum
            ID1120007 = 1120007,//Rage of Cipher
            ID1120008 = 1120008,//Cybernetic Successor
            ID1120009 = 1120009,//Heir to the Shiranui-Style
            ID1120010 = 1120010,//Growing Digital Bug
            ID1120011 = 1120011,//Roar of the Gladiator Beasts
            ID1120012 = 1120012,//Emergence of the Monarchs
            ID1120013 = 1120013,//Being Who Sees the End of the World
            ID1120014 = 1120014,//Gem-Knights' Resolution
            ID1120015 = 1120015,//Dragonmaid-To-Order
            //ID1120016 = 1120016,//coming soon
            //ID1120017 = 1120017,//coming soon
        }
        public enum WALLPAPER
        {
            ID1130001 = 1130001,//Blue-Eyes Alternative White Dragon
            ID1130002 = 1130002,//El Shaddoll Construct
            ID1130003 = 1130003,//Tri-Brigade Shuraig the Ominous Omen
            ID1130004 = 1130004,//Dingirsu, the Orcust of the Evening Star
            ID1130005 = 1130005,//Sky Striker Ace - Kagari
            ID1130006 = 1130006,//Eldlich the Golden Lord
            ID1130007 = 1130007,//Mekk-Knight Crusadia Avramax
            ID1130008 = 1130008,//Meteonis Drytron
            ID1130009 = 1130009,//Palladium Oracle Mahad
            ID1130010 = 1130010,//Elemental HERO Honest Neos
            ID1130011 = 1130011,//Shooting Quasar Dragon
            ID1130012 = 1130012,//Number F0: Utopic Draco Future
            ID1130013 = 1130013,//Odd-Eyes Arc Pendulum Dragon
            ID1130014 = 1130014,//Accesscode Talker
        }
        public enum PACK_TICKET
        {
            ID1140001 = 1140001,//Legacy Pack ticket
            ID1140002 = 1140002,//Tournament Pack ticket 1
            ID1141001 = 1141001,//Xyz Reward Ticket
            ID1141002 = 1141002,//Fusion Reward Ticket
            ID1141003 = 1141003,//Synchro Reward Ticket
            ID1141004 = 1141004,//
            ID1141005 = 1141005,//
            ID1141006 = 1141006,//
            ID1141007 = 1141007,//
        }
    }
}
