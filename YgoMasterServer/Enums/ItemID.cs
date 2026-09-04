using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    /// <summary>
    /// YgomGame.TextIDs.IDS_ITEM
    /// </summary>
    static class ItemID
    {
        public static Dictionary<Category, int[]> Values = new Dictionary<Category, int[]>();

        public static void Load(string dataDir)
        {
            Dictionary<Category, List<int>> values = new Dictionary<Category, List<int>>();
            string file = Path.Combine(dataDir, "ItemID.json");
            if (File.Exists(file))
            {
                Dictionary<string, object> categories = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                if (categories != null)
                {
                    foreach (KeyValuePair<string, object> categoryData in categories)
                    {
                        List<int> ids = Utils.GetIntList(categories, categoryData.Key);
                        Category category;
                        if (ids != null && Enum.TryParse<Category>(categoryData.Key, out category))
                        {
                            values[category] = ids;
                        }
                    }
                }
            }
            Values.Clear();
            foreach (KeyValuePair<Category, List<int>> category in values)
            {
                Values[category.Key] = category.Value.ToArray();
            }
            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                if (!Values.ContainsKey(category))
                {
                    Values[category] = new int[0];
                }
            }
            Values[Category.NONE] = new int[] { 0 };
        }

        public static int GetDefaultId(Category category)
        {
            if (Values[category].Length == 0)
            {
                return 0;
            }
            return Values[category][0];
        }

        public static List<int> GetDuelFieldParts(int itemId)
        {
            List<int> result = new List<int>();
            if (ItemID.GetCategoryFromID(itemId) == Category.FIELD)
            {
                Dictionary<Category, int> categories = new Dictionary<Category, int>()
                {
                    { Category.FIELD_OBJ, 1100000 },
                    { Category.AVATAR_HOME, 1110000 },
                };
                foreach (KeyValuePair<Category, int> category in categories)
                {
                    foreach (int value in Values[category.Key])
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
                return GetDefaultId(Category.FIELD_OBJ);
            }
            return (fieldId - 1090000) + 1100000;
        }

        public static int GetFieldAvatarBaseFromField(int fieldId)
        {
            if (fieldId <= 0)
            {
                return GetDefaultId(Category.AVATAR_HOME);
            }
            return (fieldId - 1090000) + 1110000;
        }

        public static int GetRandomId(Random rand, Category category)
        {
            int[] items;
            if (Values.TryGetValue(category, out items) && items.Length > 0)
            {
                return items[rand.Next(items.Length)];
            }
            return 0;
        }

        /// <summary>
        /// Categories by item id block (itemId / 10000) for ids of 1000000 onwards
        /// </summary>
        static Dictionary<int, Category> categoriesByIdBlock = new Dictionary<int, Category>()
        {
            { 100, Category.AVATAR },
            { 101, Category.ICON },
            { 102, Category.PROFILE_TAG },
            { 103, Category.ICON_FRAME },
            { 107, Category.PROTECTOR },
            { 108, Category.DECK_CASE },
            { 109, Category.FIELD },
            { 110, Category.FIELD_OBJ },
            { 111, Category.AVATAR_HOME },
            { 112, Category.STRUCTURE },
            { 113, Category.WALLPAPER },
            { 114, Category.PACK_TICKET },
            { 115, Category.DECK_LIMIT },
            { 116, Category.REPLAY_LIMIT },
            { 117, Category.CARD_FILE },
            { 118, Category.COIN },
            { 119, Category.BOOKMARK_LIMIT },
        };

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
            if (itemId < 300000)
            {
                // 3000-99999 (+100000 per CardStyleRarity - see RemapCardId)
                return Category.CARD;
            }
            Category category;
            if (categoriesByIdBlock.TryGetValue(itemId / 10000, out category))
            {
                return category;
            }
            return Category.NONE;
        }

        /// <summary>
        /// YgomGame.Utility.ItemUtil.Category
        /// </summary>
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
            PACK_TICKET,
            DECK_LIMIT,
            REPLAY_LIMIT,
            CARD_FILE,
            COIN,
            BOOKMARK_LIMIT,
        }

        public enum Value
        {
            None = 0,
            Gem = 1,
            GemAlt = 2,
            CpN = 3,
            CpR = 4,
            CpSR = 5,
            CpUR = 6,
            OrbDark = 8,
            OrbLight = 9,
            OrbEarth = 10,
            OrbWater = 11,
            OrbFire = 12,
            OrbWind = 13,

            DefaultIcon = 1010001,//Duelist
            DefaultIconFrame = 1030001,//ICON_FRAME01
            DefaultDeckCase = 1080001,//Duelist Card Case Red
            DefaultProtector = 1070001,//Yu-Gi-Oh! Trading Card Game
            DefaultField = 1090001,//Forest
            DefaultFieldObj = 1100001,//Forest
            StartingStructureDeck = 1120001,//Starting Deck
            DefaultWallpaper = 1130001,//Blue-Eyes Alternative White Dragon
            DefaultCoin = 1180001,//Blue-Eyes Alternative White Dragon Coin
        }
    }
}
