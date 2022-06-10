using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    class DuelRewardInfos
    {
        /// <summary>
        /// A multiplier to apply if the chapter status changed from this duel
        /// </summary>
        public double ChapterStatusChangedMultiplier;
        /// <summary>
        /// Don't give the rewards if the chapter status changed from this duel
        /// </summary>
        public bool ChapterStatusChangedNoRewards;
        /// <summary>
        /// Only give the rewards if the chapter status changed from this duel
        /// </summary>
        public bool ChapterStatusChangedOnly;
        public List<DuelRewardInfo> Win { get; private set; }
        public List<DuelRewardInfo> Lose { get; private set; }

        public DuelRewardInfos()
        {
            Win = new List<DuelRewardInfo>();
            Lose = new List<DuelRewardInfo>();
        }

        public int GetAmount(Random rand, DuelRewardInfo reward, bool chapterStatusChanged)
        {
            int amount = reward.MinValue == reward.MaxValue ?
                reward.MinValue : rand.Next(reward.MinValue, reward.MaxValue + 1);
            if (chapterStatusChanged && ChapterStatusChangedMultiplier > 0)
            {
                amount = (int)(amount * ChapterStatusChangedMultiplier);
            }
            return amount;
        }

        public void FromDictionary(Dictionary<string, object> data)
        {
            if (data == null)
            {
                return;
            }
            Win.Clear();
            Lose.Clear();
            ChapterStatusChangedMultiplier = Utils.GetValue<double>(data, "ChapterStatusChangedMultiplier");
            ChapterStatusChangedNoRewards = Utils.GetValue<bool>(data, "ChapterStatusChangedNoRewards");
            ChapterStatusChangedOnly = Utils.GetValue<bool>(data, "ChapterStatusChangedOnly");
            List<object> winData = Utils.GetValue(data, "win", default(List<object>));
            if (winData != null)
            {
                ParseRewards(Win, winData);
            }
            List<object> loseData = Utils.GetValue(data, "lose", default(List<object>));
            if (loseData != null)
            {
                ParseRewards(Lose, loseData);
            }
        }

        void ParseRewards(List<DuelRewardInfo> rewards, List<object> objList)
        {
            foreach (object obj in objList)
            {
                Dictionary<string, object> data = obj as Dictionary<string, object>;
                if (data != null)
                {
                    DuelCustomRewardType type = Utils.GetValue<DuelCustomRewardType>(data, "type");
                    if (type == DuelCustomRewardType.None)
                    {
                        return;
                    }
                    DuelRewardInfo reward = new DuelRewardInfo();
                    reward.Type = type;
                    reward.Rate = Utils.GetValue<double>(data, "rate");
                    reward.Rare = Utils.GetValue<bool>(data, "rare");
                    switch (type)
                    {
                        case DuelCustomRewardType.Gem:
                            {
                                int value;
                                if (Utils.TryGetValue(data, "value", out value))
                                {
                                    reward.MinValue = value;
                                    reward.MaxValue = value;
                                }
                                else
                                {
                                    int min, max;
                                    if (Utils.TryGetValue(data, "min", out min) &&
                                        Utils.TryGetValue(data, "max", out max))
                                    {
                                        if (min > max)
                                        {
                                            int temp = min;
                                            min = max;
                                            max = temp;
                                        }
                                        reward.MinValue = min;
                                        reward.MaxValue = max;
                                    }
                                }
                                if (reward.MinValue == 0 && reward.MaxValue == 0)
                                {
                                    continue;
                                }
                                if (reward.Rate == 0)
                                {
                                    continue;
                                }
                            }
                            break;
                        case DuelCustomRewardType.Item:
                            {
                                reward.Rate = Utils.GetValue<double>(data, "rate");
                                if (reward.Rate == 0)
                                {
                                    continue;
                                }
                                List<object> idsObjList = Utils.GetValue(data, "ids", default(List<object>));
                                if (idsObjList != null && idsObjList.Count > 0)
                                {
                                    reward.Ids = new List<int>();
                                    foreach (object idObj in idsObjList)
                                    {
                                        reward.Ids.Add((int)Convert.ChangeType(idObj, typeof(int)));
                                    }
                                }
                            }
                            break;
                        case DuelCustomRewardType.Card:
                            {
                                reward.CardRate = new List<double>();
                                reward.CardRare = new List<bool>();
                                reward.Ids = new List<int>();
                                reward.CardNoDismantle = Utils.GetValue<bool>(data, "cardNoDismantle");
                                reward.CardOwnedLimit = Utils.GetValue<int>(data, "cardOwnedLimit");
                                List<object> cardRateObjList = Utils.GetValue(data, "cardRate", default(List<object>));
                                if (cardRateObjList != null)
                                {
                                    foreach (object rateObj in cardRateObjList)
                                    {
                                        reward.CardRate.Add((int)Convert.ChangeType(rateObj, typeof(int)));
                                    }
                                }
                                List<object> cardRareObjList = Utils.GetValue(data, "cardRare", default(List<object>));
                                if (cardRareObjList != null)
                                {
                                    foreach (object rareObj in cardRareObjList)
                                    {
                                        reward.CardRare.Add((bool)Convert.ChangeType(rareObj, typeof(bool)));
                                    }
                                }
                                List<object> idsObjList = Utils.GetValue(data, "ids", default(List<object>));
                                if (idsObjList != null)
                                {
                                    foreach (object idObj in idsObjList)
                                    {
                                        reward.Ids.Add((int)Convert.ChangeType(idObj, typeof(int)));
                                    }
                                }
                                if (reward.Ids.Count > 0 && reward.Rate == 0)
                                {
                                    continue;
                                }
                                if (reward.Ids.Count == 0 && reward.CardRate.Count == 0)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                    rewards.Add(reward);
                }
            }
        }
    }

    /// <summary>
    /// For giving custom rewards from duels
    /// </summary>
    class DuelRewardInfo
    {
        public DuelCustomRewardType Type;
        public int MinValue;
        public int MaxValue;
        /// <summary>
        /// The percent chance for obtaining this reward
        /// </summary>
        public double Rate;
        /// <summary>
        /// Show gold box instead of blue box
        /// </summary>
        public bool Rare;
        /// <summary>
        /// The percent chance for each card type of rarity (normal=0, rare=1, superRare=2, ultraRare=3)
        /// </summary>
        public List<double> CardRate;
        /// <summary>
        /// Show gold box instead of blue box (specified per card rate)
        /// </summary>
        public List<bool> CardRare;
        /// <summary>
        /// Disable dismantle on this card
        /// </summary>
        public bool CardNoDismantle;
        /// <summary>
        /// A limit on the number of times a given card can be obtained from before excluding it
        /// </summary>
        public int CardOwnedLimit;
        /// <summary>
        /// A specific set of card ids to pull from (ignores CardRate/CardRare in this case, uses Rate/Rare instead)
        /// Or specific item ids (in the case of Item)
        /// </summary>
        public List<int> Ids;
    }

    enum DuelCustomRewardType
    {
        None,
        /// <summary>
        /// A number of gems (specific value or random range)
        /// </summary>
        Gem,
        /// <summary>
        /// A random unowned item
        /// </summary>
        Item,
        /// <summary>
        /// A random card using the currently unlocked shop packs as the card pool (unless otherwise specified)
        /// </summary>
        Card
    }
}
