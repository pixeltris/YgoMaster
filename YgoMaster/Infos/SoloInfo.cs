// UNUSED
// - See Act_Solo.cs instead
// - Commenting out this code to avoid confusion (may be used at a later date)
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    class SoloInfo
    {
        public Dictionary<int, SoloGateInfo> Gates { get; private set; }
        public const int BaseChapterIdMultiplier = 10000;
        public const int BaseGateId = 1;

        public SoloInfo()
        {
            Gates = new Dictionary<int, SoloGateInfo>();
        }

        public void Clear()
        {
            Gates.Clear();
        }

        public SoloGateInfo GetGate(int gateId)
        {
            SoloGateInfo gate;
            Gates.TryGetValue(gateId, out gate);
            return gate;
        }

        public SoloGateInfo GetGateFromChapterId(int chapterId)
        {
            int gateId = chapterId / BaseChapterIdMultiplier;
            return GetGate(gateId);
        }

        public SoloGateInfo GetGate(SoloChapterInfo chapter)
        {
            return GetGateFromChapterId(chapter.Id);
        }

        public int GetNextGateId()
        {
            int nextGateId = Gates.Count + BaseGateId;
            while (Gates.ContainsKey(nextGateId))
            {
                nextGateId++;
            }
            return nextGateId;
        }

        public SoloChapterInfo GetChapter(int chapterId)
        {
            int gateId = chapterId / BaseChapterIdMultiplier;
            SoloGateInfo gate = GetGate(gateId);
            if (gate != null)
            {
                SoloChapterInfo chapter;
                if (gate.AllChapters.TryGetValue(chapterId, out chapter))
                {
                    return chapter;
                }
            }
            return null;
        }

        int GetNamedNumber(object value)
        {
            int number = 0;
            string stringNumber = Convert.ToString(value);
            if (!int.TryParse(stringNumber, out number))
            {
                for (int i = 0; i < stringNumber.Length; i++)
                {
                    if (char.IsDigit(stringNumber[i]))
                    {
                        int.TryParse(stringNumber.Substring(i), out number);
                        break;
                    }
                }
            }
            return number;
        }

        int GetNamedNumber(Dictionary<string, object> data, string key)
        {
            int number = 0;
            string stringNumber;
            if (GameServer.TryGetValue<string>(data, key, out stringNumber))
            {
                return GetNamedNumber(stringNumber);
            }
            else
            {
                number = GameServer.GetValue<int>(data, key);
            }
            return number;
        }

        int ParseItemId(object obj)
        {
            int itemId = 0;
            if (obj is string)
            {
                string itemStr = (string)obj;
                string[] splitted = itemStr.Split('.');
                switch (splitted[0].ToLowerInvariant())
                {
                    case "orbtype":
                        OrbType orbType;
                        if (splitted.Length > 1 && Enum.TryParse(splitted[1], out orbType))
                        {
                            switch (orbType)
                            {
                                case OrbType.Dark: itemId = (int)ItemID.CONSUME.ID0008; break;
                                case OrbType.Light: itemId = (int)ItemID.CONSUME.ID0009; break;
                                case OrbType.Earth: itemId = (int)ItemID.CONSUME.ID0010; break;
                                case OrbType.Water: itemId = (int)ItemID.CONSUME.ID0011; break;
                                case OrbType.Fire: itemId = (int)ItemID.CONSUME.ID0012; break;
                                case OrbType.Wind: itemId = (int)ItemID.CONSUME.ID0013; break;
                            }
                        }
                        break;
                    case "gem":
                    case "gems":
                        itemId = (int)ItemID.CONSUME.ID0001;
                        break;
                    default:
                        itemId = GetNamedNumber(obj);
                        break;
                }
            }
            else
            {
                itemId = GetNamedNumber(obj);
            }
            return itemId;
        }

        /// <summary>
        /// Detects the format used and calls the relevant FromDictionaryXXXX function
        /// </summary>
        public void FromDictionary(Dictionary<string, object> data)
        {
            Clear();
            if (data == null)
            {
                return;
            }
            if (GameServer.GetValue<List<object>>(data, "gates") != null)
            {
                FromDictionaryCustomized(data);
            }
            else
            {
                FromDictionaryClient(data);
            }
        }

        /// <summary>
        /// Dictionary format used when creating custom solo content
        /// </summary>
        public void FromDictionaryCustomized(Dictionary<string, object> data)
        {
            Clear();
            List<object> gatesObj = GameServer.GetValue<List<object>>(data, "gates");
            Dictionary<int, Dictionary<string, object>> allGateData = new Dictionary<int, Dictionary<string, object>>();
            int gateEntryNumber = 0;
            foreach (object gateObj in gatesObj)
            {
                gateEntryNumber++;
                Dictionary<string, object> gateData = gateObj as Dictionary<string, object>;
                if (gateData == null)
                {
                    continue;
                }
                int id = GetNamedNumber(gateData, "id");
                if (id <= 0)
                {
                    GameServer.LogWarning("Invalid gate id on entry number " + gateEntryNumber);
                    continue;
                }
                Gates[id] = new SoloGateInfo(id);
                allGateData[id] = gateData;
            }
            gateEntryNumber = 0;
            foreach (object gateObj in gatesObj)
            {
                gateEntryNumber++;
                Dictionary<string, object> gateData = gateObj as Dictionary<string, object>;
                if (gateData == null)
                {
                    continue;
                }
                int id = GetNamedNumber(gateData, "id");
                SoloGateInfo gate;
                if (!Gates.TryGetValue(id, out gate))
                {
                    continue;
                }
                gate.Title = GameServer.GetValue<string>(gateData, "title");
                gate.Description = GameServer.GetValue<string>(gateData, "description");
                gate.CardId = GameServer.GetValue<int>(gateData, "card");
                int viewGateId = GetNamedNumber(gateData, "viewReq");
                if (viewGateId > 0)
                {
                    SoloGateInfo viewGate;
                    if (Gates.TryGetValue(viewGateId, out viewGate))
                    {
                        gate.ViewGate = viewGate;
                    }
                    else
                    {
                        GameServer.LogWarning("Failed to get view gate id " + viewGateId + " for gate " + id);
                    }
                }
                int parentGateId = GetNamedNumber(gateData, "parent");
                if (parentGateId > 0)
                {
                    SoloGateInfo parentGate;
                    if (Gates.TryGetValue(parentGateId, out parentGate))
                    {
                        gate.ParentGate = parentGate;
                        parentGate.ChildGates.Add(gate);
                    }
                    else
                    {
                        GameServer.LogWarning("Failed to get parent gate id " + parentGateId + " for gate " + id);
                    }
                }
                gate.Priority = GameServer.GetValue<int>(gateData, "priority");

                // TODO: Make sure there aren't conflicting OR/AND entries
                Dictionary<string, ChapterUnlockType> unlockSpecifiers = new Dictionary<string, ChapterUnlockType>()
                {
                    { "unlockReq", ChapterUnlockType.CHAPTER_OR },
                    { "unlockReqOR", ChapterUnlockType.CHAPTER_OR },
                    { "unlockReqAND", ChapterUnlockType.CHAPTER_AND },
                };
                foreach (KeyValuePair<string, ChapterUnlockType> unlockSpecifier in unlockSpecifiers)
                {
                    int gateUnlockReqChId = GetNamedNumber(gateData, unlockSpecifier.Key);
                    if (gateUnlockReqChId > 0)
                    {
                        gate.UnlockRequirements.Add(new SoloUnlockEntry()
                        {
                            Type = unlockSpecifier.Value,
                            Key = gateUnlockReqChId
                        });
                    }
                }
                List<object> gateUnlockReqListOR = GameServer.GetValue<List<object>>(gateData, "unlockReqOR");
                if (gateUnlockReqListOR != null)
                {
                    foreach (object chapterIdObj in gateUnlockReqListOR)
                    {
                        int chapterId = GetNamedNumber(chapterIdObj);
                        if (chapterId > 0)
                        {
                            gate.UnlockRequirements.Add(new SoloUnlockEntry()
                            {
                                Type = ChapterUnlockType.CHAPTER_OR,
                                Key = chapterId
                            });
                        }
                    }
                }
                List<object> gateUnlockReqListAND = GameServer.GetValue<List<object>>(gateData, "unlockReqAND");
                if (gateUnlockReqListAND != null)
                {
                    foreach (object chapterIdObj in gateUnlockReqListAND)
                    {
                        int chapterId = GetNamedNumber(chapterIdObj);
                        if (chapterId > 0)
                        {
                            gate.UnlockRequirements.Add(new SoloUnlockEntry()
                            {
                                Type = ChapterUnlockType.CHAPTER_AND,
                                Key = chapterId
                            });
                        }
                    }
                }

                List<object> chaptersList = GameServer.GetValue<List<object>>(gateData, "chapters");
                if (chaptersList != null)
                {
                    Dictionary<int, int> parentChapterIds = new Dictionary<int, int>();
                    foreach (object chapterObj in chaptersList)
                    {
                        Dictionary<string, object> chapterData = chapterObj as Dictionary<string, object>;
                        if (chapterData == null)
                        {
                            continue;
                        }
                        int chapterId = GetNamedNumber(chapterData, "id");
                        if (chapterId <= 0)
                        {
                            object cid;
                            chapterData.TryGetValue("id", out cid);
                            GameServer.LogWarning("Failed to handle chapter id " + cid + " on gate " + gate.Id);
                        }
                        SoloChapterInfo chapter = new SoloChapterInfo(chapterId);
                        gate.AllChapters[chapter.Id] = chapter;
                        parentChapterIds[chapter.Id] = GetNamedNumber(chapterData, "parent");
                        chapter.Scenario = GameServer.GetValue<string>(chapterData, "scenario");
                        chapter.Description = GameServer.GetValue<string>(chapterData, "description");

                        for (int i = 0; i < 3; i++)
                        {
                            string kind = null;
                            switch (i)
                            {
                                case 0: kind = "unlockReq"; break;
                                case 1: kind = "reward"; break;
                                case 2: kind = "rewardMyDeck"; break;
                            }
                            object itemDataObj;
                            if (chapterData.TryGetValue(kind, out itemDataObj))
                            {
                                List<object> itemList = itemDataObj as List<object>;
                                if (itemList == null)
                                {
                                    continue;
                                }
                                foreach (object itemObj in itemList)
                                {
                                    Dictionary<string, object> itemData = itemObj as Dictionary<string, object>;
                                    if (itemData == null)
                                    {
                                        continue;
                                    }
                                    int itemCount = GameServer.GetValue<int>(itemData, "count");
                                    object itemIdObj;
                                    if (itemData.TryGetValue("item", out itemIdObj) && itemCount > 0)
                                    {
                                        int itemId = ParseItemId(itemIdObj);
                                        if (itemId > 0)
                                        {
                                            switch (i)
                                            {
                                                case 0:
                                                    chapter.UnlockRequirements.Add(new SoloUnlockEntry()
                                                    {
                                                        Type = ChapterUnlockType.ITEM,
                                                        Key = itemId,
                                                        Value = itemCount
                                                    });
                                                    break;
                                                case 1:
                                                case 2:
                                                    List<SoloRewardEntry> rewards = i == 1 ? chapter.Rewards : chapter.RewardsMyDeck;
                                                    rewards.Add(new SoloRewardEntry()
                                                    {
                                                        ItemId = itemId,
                                                        Count = itemCount
                                                    });
                                                    break;

                                            }
                                        }
                                        else
                                        {
                                            GameServer.LogWarning("Failed to process item id " + itemIdObj + " (" + kind + ") on chapter " + chapterId);
                                        }
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(chapter.Scenario) && chapter.UnlockRequirements.Count == 0)
                        {
                            chapter.IsDuel = !GameServer.GetValue<bool>(chapterData, "isReward");
                        }
                    }
                    foreach (SoloChapterInfo chapter in gate.AllChapters.Values)
                    {
                        int parentChapterId;
                        if (parentChapterIds.TryGetValue(chapter.Id, out parentChapterId) && parentChapterId > 0)
                        {
                            SoloChapterInfo parentChapter;
                            if (gate.AllChapters.TryGetValue(parentChapterId, out parentChapter))
                            {
                                chapter.Parent = parentChapter;
                                parentChapter.Children.Add(chapter);
                            }
                        }
                    }
                }
            }
            foreach (SoloGateInfo gate in Gates.Values)
            {
                int clearChapterId = GameServer.GetValue<int>(allGateData[gate.Id], "clear");
                if (clearChapterId > 0)
                {
                    SoloChapterInfo chapter = GetChapter(clearChapterId);
                    if (chapter != null)
                    {
                        gate.ClearChapter = chapter;
                    }
                    else
                    {
                        GameServer.LogWarning("Failed to look up clear chapter " + clearChapterId + " for gate " + gate.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Dictionary format used when creating custom solo content
        /// </summary>
        public Dictionary<string, object> ToDictionaryCustomized()
        {
            const string chapterPrefix = "C";
            const string gatePrefix = "G";
            List<object> gatesList = new List<object>();
            foreach (SoloGateInfo gate in Gates.Values)
            {
                Dictionary<string, object> gateData = new Dictionary<string, object>();
                gatesList.Add(gateData);
                gateData["id"] = gatePrefix + gate.Id;
                if (gate.Priority > 0)
                {
                    gateData["priority"] = gate.Priority;
                }
                if (!string.IsNullOrEmpty(gate.Title))
                {
                    gateData["title"] = gate.Title;
                }
                if (!string.IsNullOrEmpty(gate.Description))
                {
                    gateData["description"] = gate.Description;
                }
                if (gate.CardId > 0)
                {
                    gateData["card"] = gate.CardId;
                }
                if (gate.ClearChapter != null)
                {
                    gateData["clear"] = chapterPrefix + gate.ClearChapter.Id;
                }
                if (gate.ParentGate != null)
                {
                    gateData["parent"] = gatePrefix + gate.ParentGate.Id;
                }
                if (gate.ViewGate != null)
                {
                    gateData["viewReq"] = gatePrefix + gate.ViewGate.Id;
                }
                if (gate.UnlockRequirements.Count > 0)
                {
                    string unlockTypeStr = "unlockReqOR";
                    if (gate.UnlockRequirements[0].Type == ChapterUnlockType.CHAPTER_AND)
                    {
                        unlockTypeStr = "unlockReqAND";
                    }
                    gateData[unlockTypeStr] = gate.UnlockRequirements.Select(x => chapterPrefix + x.Key).ToList();
                }
                List<object> chaptersData = new List<object>();
                gateData["chapters"] = chaptersData;
                foreach (SoloChapterInfo chapter in gate.AllChapters.Values)
                {
                    Dictionary<string, object> chapterData = new Dictionary<string, object>();
                    chaptersData.Add(chapterData);
                    chapterData["id"] = chapterPrefix + chapter.Id;
                    if (!string.IsNullOrEmpty(chapter.Description))
                    {
                        chapterData["description"] = chapter.Description;
                    }
                    if (chapter.Parent != null)
                    {
                        chapterData["parent"] = chapterPrefix + chapter.Parent.Id;
                    }
                    if (!string.IsNullOrEmpty(chapter.Scenario))
                    {
                        chapterData["scenario"] = chapter.Scenario;
                    }
                    if (chapter.UnlockRequirements.Count > 0)
                    {
                        List<Dictionary<string, object>> unlockReq = new List<Dictionary<string, object>>();
                        chapterData["unlockReq"] = unlockReq;
                        foreach (SoloUnlockEntry unlock in chapter.UnlockRequirements)
                        {
                            // TODO: support generating string item id (e.g. OrbType) as it already supports parsing it
                            unlockReq.Add(new Dictionary<string,object>()
                            {
                                { "item", unlock.ItemId },
                                { "count", unlock.ItemCount }
                            });
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        List<SoloRewardEntry> collection = i == 0 ? chapter.Rewards : chapter.RewardsMyDeck;
                        if (collection.Count > 0)
                        {
                            List<Dictionary<string, object>> rewards = new List<Dictionary<string, object>>();
                            chapterData[i == 0 ? "reward" : "rewardMyDeck"] = rewards;
                            foreach (SoloRewardEntry reward in collection)
                            {
                                rewards.Add(new Dictionary<string, object>()
                                {
                                    { "item", reward.ItemId },
                                    { "count", reward.Count }
                                });
                            }
                        }
                    }
                }
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            result["gates"] = gatesList;
            return result;
        }

        /// <summary>
        /// Dictionary format used by the client
        /// </summary>
        public void FromDictionaryClient(Dictionary<string, object> data)
        {
            Clear();
            data = GameServer.GetResData(data);
            Dictionary<string, object> masterData;
            if (data != null && GameServer.TryGetValue(data, "Master", out masterData))
            {
                data = masterData;
            }
            Dictionary<string, object> soloData;
            if (data != null && GameServer.TryGetValue(data, "Solo", out soloData))
            {
                data = soloData;
            }
            if (data == null)
            {
                return;
            }
            Dictionary<int, Dictionary<string, object>> allGateData = GameServer.GetIntDictDict(data, "gate");
            Dictionary<int, Dictionary<string, object>> allChapterData = GameServer.GetIntDictDict(data, "chapter");
            Dictionary<int, Dictionary<string, object>> allUnlockData = GameServer.GetIntDictDict(data, "unlock");
            Dictionary<int, Dictionary<string, object>> allUnlockItemData = GameServer.GetIntDictDict(data, "unlock_item");
            Dictionary<int, Dictionary<string, object>> allRewardData = GameServer.GetIntDictDict(data, "reward");
            if (allGateData == null || allChapterData == null || allUnlockData == null || allUnlockItemData == null || allRewardData == null)
            {
                return;
            }
            Dictionary<int, int> parentChapterIds = new Dictionary<int, int>();
            Dictionary<int, List<SoloRewardEntry>> allRewards = new Dictionary<int, List<SoloRewardEntry>>();
            foreach (KeyValuePair<int, Dictionary<string, object>> rewardData in allRewardData)
            {
                List<SoloRewardEntry> entries = new List<SoloRewardEntry>();
                int rewardId = rewardData.Key;
                foreach (KeyValuePair<string, object> reward in rewardData.Value)
                {
                    ItemID.Category category;
                    if (!Enum.TryParse(reward.Key, out category))
                    {
                        continue;
                    }
                    Dictionary<string, object> rewardItems = reward.Value as Dictionary<string, object>;
                    foreach (KeyValuePair<string, object> item in rewardItems)
                    {
                        int itemId;
                        int count = (int)Convert.ChangeType(item.Value, typeof(int));
                        if (!int.TryParse(item.Key, out itemId))
                        {
                            continue;
                        }
                        entries.Add(new SoloRewardEntry(itemId, count));
                    }
                }
                allRewards[rewardData.Key] = entries;
            }
            foreach (KeyValuePair<int, Dictionary<string, object>> gateData in allGateData)
            {
                if (gateData.Key > 0)
                {
                    SoloGateInfo gate = new SoloGateInfo(gateData.Key);
                    Gates[gate.Id] = gate;
                    gate.Priority = GameServer.GetValue<int>(gateData.Value, "priority");
                    int unlockId = GameServer.GetValue<int>(gateData.Value, "unlock_id");
                    if (unlockId > 0)
                    {
                        Dictionary<string, object> unlockData;
                        if (allUnlockData.TryGetValue(unlockId, out unlockData))
                        {
                            if (unlockData.Count > 1)
                            {
                                GameServer.LogWarning("TODO: Support more than 1 unlock_id type for for gates");
                            }
                            foreach (KeyValuePair<string, object> unlockReq in unlockData)
                            {
                                ChapterUnlockType unlockType;
                                if (Enum.TryParse(unlockReq.Key, out unlockType))
                                {
                                    switch (unlockType)
                                    {
                                        case ChapterUnlockType.CHAPTER_AND:
                                        case ChapterUnlockType.CHAPTER_OR:
                                            List<object> items = unlockReq.Value as List<object>;
                                            if (items != null)
                                            {
                                                foreach (object item in items)
                                                {
                                                    int chapterId = (int)Convert.ChangeType(item, typeof(int));
                                                    gate.UnlockRequirements.Add(new SoloUnlockEntry()
                                                    {
                                                        Type = unlockType,
                                                        Key = chapterId
                                                    });
                                                }
                                            }
                                            break;
                                        default:
                                            GameServer.LogWarning("TODO: Handle " + unlockType + " unlocks for gates");
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, Dictionary<string, object>> gateChapterData in allChapterData)
            {
                int gateId = gateChapterData.Key;
                SoloGateInfo gate = GetGate(gateId);
                if (gate == null)
                {
                    GameServer.LogWarning("Failed to find gate " + gateId + " when getting chapters");
                    continue;
                }
                foreach (KeyValuePair<string, object> chapterDataKV in gateChapterData.Value)
                {
                    int chapterId;
                    Dictionary<string, object> chapterData = chapterDataKV.Value as Dictionary<string, object>;
                    if (!int.TryParse(chapterDataKV.Key, out chapterId) || chapterId == 0 || chapterData == null)
                    {
                        continue;
                    }
                    if (!gate.IsValidChapterId(chapterId))
                    {
                        GameServer.LogWarning(chapterId + " is an invalid id for gate " + gateId);
                        continue;
                    }
                    SoloChapterInfo chapter = new SoloChapterInfo(chapterId);
                    gate.AllChapters[chapter.Id] = chapter;
                    parentChapterIds[chapter.Id] = GameServer.GetValue<int>(chapterData, "parent_chapter");
                    chapter.Scenario = GameServer.GetValue<string>(chapterData, "begin_sn");
                    int unlockId = GameServer.GetValue<int>(chapterData, "unlock_id");
                    if (unlockId == 0)
                    {
                        chapter.IsDuel = string.IsNullOrEmpty(chapter.Scenario) && GameServer.GetValue<int>(chapterData, "npc_id") != 0;
                    }
                    else
                    {
                        Dictionary<string, object> unlockData;
                        if (allUnlockData.TryGetValue(unlockId, out unlockData))
                        {
                            foreach (KeyValuePair<string, object> unlockReq in unlockData)
                            {
                                ChapterUnlockType unlockType;
                                if (Enum.TryParse(unlockReq.Key, out unlockType))
                                {
                                    if (unlockType != ChapterUnlockType.ITEM)
                                    {
                                        GameServer.LogWarning("TODO: Handle " + unlockType + " unlocks for chapters");
                                        continue;
                                    }
                                    List<object> itemSetList = unlockReq.Value as List<object>;
                                    if (itemSetList == null)
                                    {
                                        continue;
                                    }
                                    foreach (object itemSet in itemSetList)
                                    {
                                        int itemSetId = (int)Convert.ChangeType(itemSet, typeof(int));
                                        Dictionary<string, object> itemsByCategory;
                                        if (allUnlockItemData.TryGetValue(itemSetId, out itemsByCategory))
                                        {
                                            foreach (KeyValuePair<string, object> itemCategory in itemsByCategory)
                                            {
                                                Dictionary<string, object> items = itemCategory.Value as Dictionary<string, object>;
                                                ItemID.Category category;
                                                if (!Enum.TryParse(itemCategory.Key, out category))
                                                {
                                                    continue;
                                                }
                                                foreach (KeyValuePair<string, object> item in items)
                                                {
                                                    int itemId;
                                                    int count = (int)Convert.ChangeType(item.Value, typeof(int));
                                                    if (!int.TryParse(item.Key, out itemId))
                                                    {
                                                        continue;
                                                    }
                                                    chapter.UnlockRequirements.Add(new SoloUnlockEntry(ChapterUnlockType.ITEM, itemId, count));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        int setId = GameServer.GetValue<int>(chapterData, i == 0 ? "set_id" : "mydeck_set_id");
                        if (setId != 0)
                        {
                            List<SoloRewardEntry> rewards;
                            if (allRewards.TryGetValue(setId, out rewards))
                            {
                                if (i == 0)
                                {
                                    chapter.Rewards.AddRange(rewards);
                                }
                                else
                                {
                                    chapter.RewardsMyDeck.AddRange(rewards);
                                }
                            }
                            else
                            {
                                GameServer.LogWarning("Failed to look up chapter " + chapter.Id + " set id " + setId);
                            }
                        }
                    }
                }
            }
            foreach (SoloGateInfo gate in Gates.Values)
            {
                int parentGateId;
                if (GameServer.TryGetValue(allGateData[gate.Id], "parent_gate", out parentGateId))
                {
                    SoloGateInfo parentGate;
                    if (Gates.TryGetValue(parentGateId, out parentGate))
                    {
                        gate.ParentGate = parentGate;
                        parentGate.ChildGates.Add(gate);
                    }
                }
                int viewGateId;
                if (GameServer.TryGetValue(allGateData[gate.Id], "view_gate", out viewGateId))
                {
                    SoloGateInfo viewGate;
                    if (Gates.TryGetValue(viewGateId, out viewGate))
                    {
                        gate.ViewGate = viewGate;
                    }
                }
                foreach (SoloChapterInfo chapter in gate.AllChapters.Values)
                {
                    int parentChapterId;
                    if (parentChapterIds.TryGetValue(chapter.Id, out parentChapterId) && parentChapterId > 0)
                    {
                        SoloChapterInfo parentChapter;
                        if (gate.AllChapters.TryGetValue(parentChapterId, out parentChapter))
                        {
                            chapter.Parent = parentChapter;
                            parentChapter.Children.Add(chapter);
                        }
                    }
                }
            }
            // Now all the chapters are added to the gates we can hook up the clear chapter of the gates
            foreach (SoloGateInfo gate in Gates.Values)
            {
                int clearChapterId = GameServer.GetValue<int>(allGateData[gate.Id], "clear_chapter");
                if (clearChapterId > 0)
                {
                    SoloChapterInfo chapter = GetChapter(clearChapterId);
                    if (chapter != null)
                    {
                        gate.ClearChapter = chapter;
                    }
                    else
                    {
                        GameServer.LogWarning("Failed to look up clear_chapter " + clearChapterId + " for gate " + gate.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Dictionary format used by the client
        /// </summary>
        public Dictionary<string, object> ToDictionaryClient()
        {
            // TODO
            throw new NotImplementedException();
        }
    }

    class SoloGateInfo
    {
        public int Id;
        public int Priority;
        public List<SoloUnlockEntry> UnlockRequirements { get; private set; }
        public List<SoloGateInfo> ChildGates { get; private set; }// <gateid, SoloGateInfo>
        public SoloGateInfo ParentGate;
        public SoloGateInfo ViewGate;
        public SoloChapterInfo ClearChapter;
        public Dictionary<int, SoloChapterInfo> AllChapters { get; private set; }// Does not include chapters in child gates

        // Params which require client modding
        public int CardId;
        public string Title;
        public string Description;

        public SoloGateInfo(int id)
        {
            Id = id;
            UnlockRequirements = new List<SoloUnlockEntry>();
            ChildGates = new List<SoloGateInfo>();
            AllChapters = new Dictionary<int, SoloChapterInfo>();
        }

        public SoloChapterInfo GetChapter(int chapterId)
        {
            SoloChapterInfo result;
            AllChapters.TryGetValue(chapterId, out result);
            return result;
        }

        public int GetNextChapterId()
        {
            int nextChapterId = (Id * SoloInfo.BaseChapterIdMultiplier) + AllChapters.Count + 1;
            while (AllChapters.ContainsKey(nextChapterId))
            {
                nextChapterId++;
            }
            return nextChapterId;
        }

        public bool IsValidChapterId(int chapterId)
        {
            int chapterIdStart = (Id * SoloInfo.BaseChapterIdMultiplier) + 1;
            int chapterIdEnd = ((Id + 1) * SoloInfo.BaseChapterIdMultiplier) - 1;
            return chapterId >= chapterIdStart && chapterId <= chapterIdEnd;
        }
    }

    struct SoloUnlockEntry
    {
        public ChapterUnlockType Type;
        public int Key;
        public int Value;

        public int ItemId
        {
            get { return Key; }
        }
        public int ItemCount
        {
            get { return Value; }
        }
        public int ChapterId
        {
            get { return Key; }
        }

        public SoloUnlockEntry(ChapterUnlockType type, int key, int value = 0)
        {
            Type = type;
            Key = key;
            Value = value;
        }
    }

    struct SoloRewardEntry
    {
        public int ItemId;
        public int Count;

        public ItemID.Category Category
        {
            get { return ItemID.GetCategoryFromID(ItemId); }
        }

        public SoloRewardEntry(int itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
    }

    class SoloChapterInfo
    {
        public int Id;
        public bool IsDuel;
        public string Scenario;
        public List<SoloRewardEntry> Rewards { get; private set; }
        public List<SoloRewardEntry> RewardsMyDeck { get; private set; }
        public List<SoloUnlockEntry> UnlockRequirements { get; private set; }
        public List<SoloChapterInfo> Children { get; private set; }
        public SoloChapterInfo Parent;

        // Params which require client modding
        public string Description;

        public SoloChapterType Type
        {
            get
            {
                if (!string.IsNullOrEmpty(Scenario))
                {
                    return SoloChapterType.Scenario;
                }
                if (UnlockRequirements.Count > 0)
                {
                    return SoloChapterType.Lock;
                }
                if (!IsDuel)
                {
                    return SoloChapterType.Reward;
                }
                if (RewardsMyDeck.Count > 0)
                {
                    return SoloChapterType.Duel;
                }
                return SoloChapterType.PracticeDuel;
            }
        }

        public SoloChapterInfo(int id)
        {
            Id = id;
            Rewards = new List<SoloRewardEntry>();
            RewardsMyDeck = new List<SoloRewardEntry>();
            UnlockRequirements = new List<SoloUnlockEntry>();
            Children = new List<SoloChapterInfo>();
        }
    }

    /// <summary>
    /// Basically the same as YgomGame.Solo.SoloModeUtil.DialogType
    /// </summary>
    enum SoloChapterType
    {
        None,
        Duel,
        PracticeDuel,
        Scenario,
        Lock,
        Reward
    }
}
*/