using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    class CraftInfo
    {
        public Dictionary<CardRarity, CraftPointRollover> Rollover { get; private set; }
        public Dictionary<CardRarity, Dictionary<CardStyleRarity, int>> CraftRates { get; private set; }
        public Dictionary<CardRarity, Dictionary<CardStyleRarity, double>> CraftStyleRarityRates { get; private set; }
        public Dictionary<CardRarity, Dictionary<CardStyleRarity, int>> DismantleRates { get; private set; }

        public CraftInfo()
        {
            Rollover = new Dictionary<CardRarity, CraftPointRollover>();
            CraftRates = new Dictionary<CardRarity, Dictionary<CardStyleRarity, int>>();
            CraftStyleRarityRates = new Dictionary<CardRarity, Dictionary<CardStyleRarity, double>>();
            DismantleRates = new Dictionary<CardRarity, Dictionary<CardStyleRarity, int>>();
        }

        int GetRate(Dictionary<CardRarity, Dictionary<CardStyleRarity, int>> rates, CardRarity rarity, CardStyleRarity styleRarity)
        {
            while (styleRarity > CardStyleRarity.None)
            {
                int rate;
                Dictionary<CardStyleRarity, int> styleRates;
                if (rates.TryGetValue(rarity, out styleRates) && styleRates.TryGetValue(styleRarity, out rate))
                {
                    return rate;
                }
                styleRarity--;
            }
            return 0;
        }

        public int GetCraftRequirement(CardRarity rarity, CardStyleRarity styleRarity)
        {
            return GetRate(CraftRates, rarity, styleRarity);
        }

        public int GetDismantleReward(CardRarity rarity, CardStyleRarity styleRarity)
        {
            return GetRate(DismantleRates, rarity, styleRarity);
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            Rollover.Clear();
            CraftRates.Clear();
            CraftStyleRarityRates.Clear();
            DismantleRates.Clear();
            if (data == null)
            {
                return;
            }
            Dictionary<string, object> craftData = null;
            Dictionary<string, object> craftStyleRarityData = null;
            Dictionary<string, object> dismantleData = null;
            if (data.ContainsKey("Craft"))
            {
                Dictionary<string, object> rolloverDatas = Utils.GetDictionary(data, "CraftPointRollover");
                if (rolloverDatas != null)
                {
                    foreach (KeyValuePair<string, object> rolloverEntry in rolloverDatas)
                    {
                        CardRarity rarity;
                        if (Enum.TryParse<CardRarity>(rolloverEntry.Key, out rarity) && rarity < CardRarity.UltraRare)
                        {
                            Dictionary<string, object> rolloverData = rolloverEntry.Value as Dictionary<string, object>;
                            if (rolloverData == null)
                            {
                                continue;
                            }
                            CraftPointRollover rollover = new CraftPointRollover();
                            rollover.At = Utils.GetValue<int>(rolloverData, "at");
                            rollover.Take = Utils.GetValue<int>(rolloverData, "take");
                            rollover.Give = Utils.GetValue<int>(rolloverData, "give");
                            Rollover[rarity] = rollover;
                        }
                    }
                }

                craftData = Utils.GetDictionary(data, "Craft");
                craftStyleRarityData = Utils.GetDictionary(data, "CraftStyleRarity");
                dismantleData = Utils.GetDictionary(data, "Dismantle");
            }
            else if (data.ContainsKey("generate_rate_list"))
            {
                craftData = Utils.GetDictionary(data, "generate_rate_list");
                dismantleData = Utils.GetDictionary(data, "exchange_rate_list");
            }
            if (craftData != null)
            {
                RatesFromDictionary(craftData, CraftRates);
            }
            if (craftStyleRarityData != null)
            {
                RatesFromDictionary(craftStyleRarityData, CraftStyleRarityRates);
            }
            if (dismantleData != null)
            {
                RatesFromDictionary(dismantleData, DismantleRates);
            }
        }

        void RatesFromDictionary<T>(Dictionary<string, object> data, Dictionary<CardRarity, Dictionary<CardStyleRarity, T>> rates)
        {
            foreach (KeyValuePair<string, object> rarityData in data)
            {
                CardRarity rarity;
                if (!Enum.TryParse<CardRarity>(rarityData.Key, out rarity))
                {
                    continue;
                }
                rates[rarity] = new Dictionary<CardStyleRarity, T>();
                Dictionary<string, object> rarityItems = rarityData.Value as Dictionary<string, object>;
                if (rarityItems != null)
                {
                    foreach (KeyValuePair<string, object> item in rarityItems)
                    {
                        CardStyleRarity styleRarity;
                        if (!Enum.TryParse<CardStyleRarity>(item.Key, out styleRarity))
                        {
                            continue;
                        }
                        rates[rarity][styleRarity] = (T)Convert.ChangeType(item.Value, typeof(T));
                    }
                }
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["generate_rate_list"] = RatesToDictionary(CraftRates);
            result["exchange_rate_list"] = RatesToDictionary(DismantleRates);
            return result;
        }

        Dictionary<string, object> RatesToDictionary(Dictionary<CardRarity, Dictionary<CardStyleRarity, int>> rates)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<CardRarity, Dictionary<CardStyleRarity, int>> rateInfo in rates)
            {
                Dictionary<string, object> rarityRates = new Dictionary<string, object>();
                foreach (KeyValuePair<CardStyleRarity, int> rarity in rateInfo.Value)
                {
                    rarityRates[((int)rarity.Key).ToString()] = rarity.Value;
                }
                result[((int)rateInfo.Key).ToString()] = rarityRates;
            }
            return result;
        }
    }

    class CraftPointRollover
    {
        public int At;
        public int Take;
        public int Give;
    }
}
