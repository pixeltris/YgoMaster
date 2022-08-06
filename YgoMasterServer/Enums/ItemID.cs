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
                return (int)items.GetValue(rand.Next(items.Length));
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
            DECK_LIMIT
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

            DefaultDeckCase = 1080001,//Duelist Card Case Red
            DefaultProtector = 1070001,//Yu-Gi-Oh! Trading Card Game
            DefaultField = 1090001,//Forest
            DefaultFieldObj = 1100001,//Forest
            StartingStructureDeck = 1120001,//Starting Deck
        }
    }
}
