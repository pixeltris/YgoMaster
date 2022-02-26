using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace YgoMaster
{
    // NOTE: The indexed dictionary is exclusively for the displayed cards on a deck so it has a hard coded limit of 3
    [DebuggerDisplay("Count = {Count}")]
    class CardCollection
    {
        List<KeyValuePair<int, CardStyleRarity>> collection;
        const int displayedCardsCount = 3;

        public int Count
        {
            get { return collection.Count; }
        }

        public CardCollection()
        {
            collection = new List<KeyValuePair<int, CardStyleRarity>>();
        }

        public void Clear()
        {
            collection.Clear();
        }

        public void Add(int cardId, CardStyleRarity styleRarity = CardStyleRarity.Normal)
        {
            collection.Add(new KeyValuePair<int, CardStyleRarity>(cardId, styleRarity));
        }

        public void RemoveAll(int cardId)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i].Key == cardId)
                {
                    collection.RemoveAt(i);
                }
            }
        }

        public List<KeyValuePair<int, CardStyleRarity>> GetCollection()
        {
            return collection;
        }

        public IEnumerable<int> GetIds()
        {
            return collection.Select(x => x.Key);
        }

        public void CopyFrom(CardCollection other)
        {
            Clear();
            foreach (KeyValuePair<int, CardStyleRarity> item in other.collection)
            {
                collection.Add(item);
            }
        }

        public Dictionary<string, object> ToIndexDictionary(bool longKeys = false)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Dictionary<string, object> ids = new Dictionary<string, object>();
            Dictionary<string, object> r = new Dictionary<string, object>();
            for (int i = 0; i < displayedCardsCount; i++)
            {
                ids[(i + 1).ToString()] = i < collection.Count ? collection[i].Key : 0;
                r[(i + 1).ToString()] =  i < collection.Count ? (int)collection[i].Value : 1;
            }
            result[longKeys ? "CardIds" : "ids"] = ids;
            result[longKeys ? "Rare" : "r"] = r;
            return result;
        }

        public void FromIndexedDictionary(Dictionary<string, object> dict, bool longKeys = false)
        {
            Clear();
            if (dict == null)
            {
                return;
            }
            Dictionary<string, object> ids = Utils.GetDictionary(dict, longKeys ? "CardIds" : "ids");
            Dictionary<string, object> r = Utils.GetDictionary(dict, longKeys ? "Rare" : "r");
            if (ids != null)
            {
                if (r != null && ids.Count != r.Count)
                {
                    Utils.LogWarning("Card style rarity length missmatch " + ids.Count + " - " + r.Count);
                }
                else
                {
                    for (int i = 0; i < displayedCardsCount; i++)
                    {
                        int cardId = Utils.GetValue<int>(ids, (i + 1).ToString());
                        CardStyleRarity styleRarity = (r == null ? CardStyleRarity.Normal : (CardStyleRarity)Utils.GetValue<int>(r, (i + 1).ToString()));
                        collection.Add(new KeyValuePair<int, CardStyleRarity>(cardId, styleRarity));
                    }
                }
            }
        }

        public Dictionary<string, object> ToDictionary(bool longKeys = false)
        {
            List<int> ids = new List<int>();
            List<int> r = new List<int>();
            foreach (KeyValuePair<int, CardStyleRarity> item in collection)
            {
                ids.Add(item.Key);
                r.Add((int)item.Value);
            }
            return new Dictionary<string, object>()
            {
                { longKeys ? "CardIds" : "ids", ids },
                { longKeys ? "Rare" : "r", r }
            };
        }

        public void FromDictionary(Dictionary<string, object> dict, bool longKeys = false)
        {
            Clear();
            if (dict == null)
            {
                return;
            }
            List<object> ids;
            List<object> r;
            if (Utils.TryGetValue(dict, longKeys ? "CardIds" : "ids", out ids))
            {
                Utils.TryGetValue(dict, longKeys ? "Rare" : "r", out r);
                if (r != null && ids.Count != r.Count)
                {
                    Utils.LogWarning("Card style rarity length missmatch " + ids.Count + " - " + r.Count);
                }
                else
                {
                    for (int i = 0; i < ids.Count; i++)
                    {
                        int cardId = (int)(long)ids[i];
                        CardStyleRarity styleRarity = (r == null ? CardStyleRarity.Normal : (CardStyleRarity)(long)r[i]);
                        collection.Add(new KeyValuePair<int, CardStyleRarity>(cardId, styleRarity));
                    }
                }
            }
        }
    }
}
