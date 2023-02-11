using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace YgoMaster
{
    class DeckInfo
    {
#pragma warning disable CS0649
        public static int DefaultRegulationId;
#pragma warning restore CS0649
        public const string DefaultRegulationName = "IDS_CARDMENU_REGULATION_NORMAL";

        public int Id;
        public string Name;
        public long TimeCreated;
        public long TimeEdited;
        public int RegulationId;
        public DeckAccessoryInfo Accessory;
        public CardCollection DisplayCards { get; private set; }
        public CardCollection MainDeckCards { get; private set; }
        public CardCollection ExtraDeckCards { get; private set; }
        public CardCollection SideDeckCards { get; private set; }
        public CardCollection TrayCards { get; private set; }//KEY_DECKLIST_TRAY

        /// <summary>
        /// Used internally to keep track of a deck and save it
        /// </summary>
        public string File;

        public bool IsYdkDeck
        {
            get { return !string.IsNullOrEmpty(File) && File.ToLowerInvariant().EndsWith(".ydk"); }
        }

        /// <summary>
        /// Maximum deck name length as defined by the client
        /// </summary>
        public const int NameMaxLength = 70;

        public DeckInfo()
        {
            Accessory = new DeckAccessoryInfo();
            DisplayCards = new CardCollection();
            MainDeckCards = new CardCollection();
            ExtraDeckCards = new CardCollection();
            SideDeckCards = new CardCollection();
            TrayCards = new CardCollection();
        }

#if !YGO_MASTER_CLIENT
        public bool IsValid(Player player)
        {
            if (MainDeckCards.Count < 40 || MainDeckCards.Count > 60 || ExtraDeckCards.Count > 15 || SideDeckCards.Count > 15)
            {
                return false;
            }
            // TODO: Validate the StyleRarity of the cards
            Dictionary<int, int> count = new Dictionary<int, int>();
            IEnumerable<int>[] collections = { MainDeckCards.GetIds(), ExtraDeckCards.GetIds(), SideDeckCards.GetIds() };
            foreach (IEnumerable<int> collection in collections)
            {
                foreach (int id in collection)
                {
                    if (!count.ContainsKey(id))
                    {
                        count[id] = player.Cards.GetCount(id);
                    }
                    if (--count[id] < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
#endif

        public List<int> GetAllCards(bool main = true, bool extra = true, bool side = false, bool tray = false)
        {
            List<int> result = new List<int>();
            if (main) result.AddRange(MainDeckCards.GetIds());
            if (extra) result.AddRange(ExtraDeckCards.GetIds());
            if (side) result.AddRange(SideDeckCards.GetIds());
            if (tray) result.AddRange(TrayCards.GetIds());
            return result;
        }

        public void GetNewFilePath(string targetDir)
        {
            Utils.TryCreateDirectory(targetDir);
            string name = Name;
            char[] invalids = System.IO.Path.GetInvalidFileNameChars();
            name = String.Join("_", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            if (string.IsNullOrEmpty(name))
            {
                name = "Deck";
            }
            bool asYdk = false;
            if (name.EndsWith(".ydk", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(0, name.Length - 4);
                asYdk = true;
            }
            for (int i = 0; i < int.MaxValue; i++)
            {
                File = System.IO.Path.Combine(targetDir, name + (i == 0 ? "" : "-" + i) + (asYdk ? ".ydk" : ".json"));
                if (!System.IO.File.Exists(File))
                {
                    break;
                }
            }
        }

        public void Load()
        {
            if (IsYdkDeck)
            {
                YdkHelper.LoadDeck(this);
            }
            else
            {
                FromDictionaryEx(MiniJSON.Json.DeserializeStripped(System.IO.File.ReadAllText(File)) as Dictionary<string, object>);
            }
            if (RegulationId == 0)
            {
                RegulationId = DefaultRegulationId;
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(File))
            {
                return;
            }
            Utils.TryCreateDirectory(System.IO.Path.GetDirectoryName(File));
            if (IsYdkDeck)
            {
                YdkHelper.SaveDeck(this);
            }
            else
            {
                System.IO.File.WriteAllText(File, MiniJSON.Json.Serialize(ToDictionaryEx()));
            }
        }

        public void CopyFrom(DeckInfo other)
        {
            Id = other.Id;
            Name = other.Name;
            TimeCreated = other.TimeCreated;
            TimeEdited = other.TimeEdited;
            Accessory.CopyFrom(other.Accessory);
            DisplayCards.CopyFrom(other.DisplayCards);
            MainDeckCards.CopyFrom(other.MainDeckCards);
            ExtraDeckCards.CopyFrom(other.ExtraDeckCards);
            SideDeckCards.CopyFrom(other.SideDeckCards);
            TrayCards.CopyFrom(other.TrayCards);
        }

        public void Clear()
        {
            Id = 0;
            Name = null;
            File = null;
            TimeCreated = 0;
            TimeEdited = 0;
            Accessory.Clear();
            DisplayCards.Clear();
            MainDeckCards.Clear();
            ExtraDeckCards.Clear();
            SideDeckCards.Clear();
            TrayCards.Clear();
        }

        public void FromDictionary(Dictionary<string, object> data, bool longKeys = false)
        {
            if (data == null)
            {
                return;
            }
            MainDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Main" : "m"), longKeys);
            ExtraDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Extra" : "e"), longKeys);
            SideDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Side" : "s"), longKeys);
            TrayCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Tray" : "t"), longKeys);
        }

        public Dictionary<string, object> ToDictionary(bool longKeys = false)
        {
            return new Dictionary<string, object>()
            {
                // There is also "t" which is a "tray" (KEY_DECKLIST_TRAY)
                { longKeys ? "Main" : "m", MainDeckCards.ToDictionary(longKeys) },
                { longKeys ? "Extra" : "e", ExtraDeckCards.ToDictionary(longKeys) },
                { longKeys ? "Side" : "s", SideDeckCards.ToDictionary(longKeys) }
            };
        }

        public void FromDictionaryEx(Dictionary<string, object> data, bool longKeys = false)
        {
            if (data == null)
            {
                return;
            }
            Name = Utils.GetValue<string>(data, "name");
            TimeCreated = Utils.GetValue<uint>(data, "ct");
            TimeEdited = Utils.GetValue<uint>(data, "et");
            RegulationId = Utils.GetValue<int>(data, "regulation_id");
            Accessory.FromDictionary(Utils.GetDictionary(data, "accessory"));
            DisplayCards.FromIndexedDictionary(Utils.GetDictionary(data, "pick_cards"));
            if (data.ContainsKey("focus"))
            {
                DisplayCards.FromIndexedDictionary(Utils.GetDictionary(data, "focus"));
            }
            MainDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Main" : "m"), longKeys);
            ExtraDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Extra" : "e"), longKeys);
            SideDeckCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Side" : "s"), longKeys);
            TrayCards.FromDictionary(Utils.GetDictionary(data, longKeys ? "Tray" : "t"), longKeys);
        }

        public Dictionary<string, object> ToDictionaryEx(bool longKeys = false)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["name"] = Name;
            data["ct"] = TimeCreated;
            data["et"] = TimeEdited;
            data["regulation_id"] = RegulationId;
            data["accessory"] = Accessory.ToDictionary();
            data["pick_cards"] = DisplayCards.ToIndexDictionary();
            data[longKeys ? "Main" : "m"] = MainDeckCards.ToDictionary(longKeys);
            data[longKeys ? "Extra" : "e"] = ExtraDeckCards.ToDictionary(longKeys);
            data[longKeys ? "Side" : "s"] = SideDeckCards.ToDictionary(longKeys);
            return data;
        }

        public Dictionary<string, object> ToDictionaryStructureDeck()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["structure_id"] = Id;
            data["accessory"] = Accessory.ToDictionary();
            data["focus"] = DisplayCards.ToDictionary();
            data["contents"] = new Dictionary<string, object>()
            {
                { "m", MainDeckCards.ToDictionary() },
                { "e", ExtraDeckCards.ToDictionary() },
                { "s", SideDeckCards.ToDictionary() },
            };
            return data;
        }
    }

    class DeckAccessoryInfo
    {
        public int Box;
        public int Sleeve;
        public int Field;
        public int FieldObj;
        public int AvBase;// Can be 0 (the others should be assigned to some value)

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["box"] = Box;
            result["sleeve"] = Sleeve;
            result["field"] = Field;
            result["object"] = FieldObj;
            result["av_base"] = AvBase;
            return result;
        }

        public void CopyFrom(DeckAccessoryInfo other)
        {
            Box = other.Box;
            Sleeve = other.Sleeve;
            Field = other.Field;
            FieldObj = other.FieldObj;
            AvBase = other.AvBase;
        }

        public void Clear()
        {
            Box = 0;
            Sleeve = 0;
            Field = 0;
            FieldObj = 0;
            AvBase = 0;
        }

        public void FromDictionary(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return;
            }
            Box = Utils.GetValue<int>(dict, "box");
            Sleeve = Utils.GetValue<int>(dict, "sleeve");
            Field = Utils.GetValue<int>(dict, "field");
            FieldObj = Utils.GetValue<int>(dict, "object");
            AvBase = Utils.GetValue<int>(dict, "av_base");
        }

#if !YGO_MASTER_CLIENT
        public void Sanitize(Player player)
        {
            Box = Sanitize(player, ItemID.Category.DECK_CASE, Box);
            Sleeve = Sanitize(player, ItemID.Category.PROTECTOR, Sleeve);
            Field = Sanitize(player, ItemID.Category.FIELD, Field);
            FieldObj = Sanitize(player, ItemID.Category.FIELD_OBJ, FieldObj);
            AvBase = Sanitize(player, ItemID.Category.AVATAR_HOME, AvBase, true);
        }

        int Sanitize(Player player, ItemID.Category category, int value, bool allowZeroValue = false)
        {
            if (value == 0 && allowZeroValue)
            {
                return value;
            }
            if (value != 0 && player.Items.Contains(value))
            {
                return value;
            }
            foreach (int id in ItemID.Values[category])
            {
                if (player.Items.Contains(id))
                {
                    return id;
                }
            }
            return 0;
        }
#endif

        /// <summary>
        /// Sets values based on a fresh master duel account without any modifications (red deck case, grassy duel mat, standard sleeve)
        /// </summary>
        public void SetDefault()
        {
            Box = (int)ItemID.Value.DefaultDeckCase;
            Sleeve = (int)ItemID.Value.DefaultProtector;
            Field = (int)ItemID.Value.DefaultField;
            FieldObj = (int)ItemID.Value.DefaultFieldObj;
            AvBase = 0;
        }
    }
}
