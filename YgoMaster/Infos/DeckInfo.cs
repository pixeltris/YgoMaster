using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace YgoMaster
{
    class DeckInfo
    {
        public int Id;
        public string Name;
        public int Status;
        public long TimeCreated;
        public long TimeEdited;
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

        public DeckInfo()
        {
            Accessory = new DeckAccessoryInfo();
            DisplayCards = new CardCollection();
            MainDeckCards = new CardCollection();
            ExtraDeckCards = new CardCollection();
            SideDeckCards = new CardCollection();
            TrayCards = new CardCollection();
        }

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
            string name = Name;
            char[] invalids = System.IO.Path.GetInvalidFileNameChars();
            String.Join("_", name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
            if (string.IsNullOrEmpty(name))
            {
                name = "Deck";
            }
            for (int i = 0; i < int.MaxValue; i++)
            {
                File = System.IO.Path.Combine(targetDir, name + (i == 0 ? "" : "-" + i) + ".json");
                if (!System.IO.File.Exists(File))
                {
                    break;
                }
            }
        }

        public void CopyFrom(DeckInfo other)
        {
            Id = other.Id;
            Name = other.Name;
            Status = other.Status;
            TimeCreated = other.TimeCreated;
            TimeEdited = other.TimeEdited;
            Accessory.CopyFrom(other.Accessory);
            DisplayCards.CopyFrom(other.DisplayCards);
            MainDeckCards.CopyFrom(other.MainDeckCards);
            ExtraDeckCards.CopyFrom(other.ExtraDeckCards);
            SideDeckCards.CopyFrom(other.SideDeckCards);
            TrayCards.CopyFrom(other.TrayCards);
        }

        public void FromDictionary(Dictionary<string, object> data, bool longKeys = false)
        {
            if (data == null)
            {
                return;
            }
            MainDeckCards.FromDictionary(GameServer.GetDictionary(data, longKeys ? "Main" : "m"), longKeys);
            ExtraDeckCards.FromDictionary(GameServer.GetDictionary(data, longKeys ? "Extra" : "e"), longKeys);
            SideDeckCards.FromDictionary(GameServer.GetDictionary(data, longKeys ? "Side" : "s"), longKeys);
            TrayCards.FromDictionary(GameServer.GetDictionary(data, longKeys ? "Tray" : "t"), longKeys);
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

        public void FromDictionary(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return;
            }
            Box = GameServer.GetValue<int>(dict, "box");
            Sleeve = GameServer.GetValue<int>(dict, "sleeve");
            Field = GameServer.GetValue<int>(dict, "field");
            FieldObj = GameServer.GetValue<int>(dict, "object");
            AvBase = GameServer.GetValue<int>(dict, "av_base");
        }

        public void Sanitize(Player player)
        {
            Box = Sanitize<ItemID.DECK_CASE>(player, Box);
            Sleeve = Sanitize<ItemID.PROTECTOR>(player, Sleeve);
            Field = Sanitize<ItemID.FIELD>(player, Field);
            FieldObj = Sanitize<ItemID.FIELD_OBJ>(player, FieldObj);
            AvBase = Sanitize<ItemID.AVATAR_HOME>(player, AvBase, true);
        }

        int Sanitize<T>(Player player, int value, bool allowZeroValue = false)
        {
            if (value == 0 && allowZeroValue)
            {
                return value;
            }
            if (value != 0 && player.Items.Contains(value))
            {
                return value;
            }
            foreach (int id in Enum.GetValues(typeof(T)))
            {
                if (player.Items.Contains(id))
                {
                    return id;
                }
            }
            return 0;
        }
    }
}
