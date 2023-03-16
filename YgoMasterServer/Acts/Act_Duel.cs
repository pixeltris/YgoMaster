using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YgoMaster
{
    partial class GameServer
    {
        DuelSettings GetSoloDuelSettings(Player player, int chapterId)
        {
            DuelSettings duelSettings;
            ChapterStatus chapterStatus;
            if (SoloDuels.TryGetValue(chapterId, out duelSettings) &&
                player.SoloChapters.TryGetValue(chapterId, out chapterStatus))
            {
                if (!IsPracticeDuel(chapterId))
                {
                    // NOTE: "Solo.detail" is only requested once. We might need to tell the client of updates on duel completion?
                    if (chapterStatus == ChapterStatus.COMPLETE ||
                        (player.Duel.IsMyDeck && chapterStatus == ChapterStatus.MYDECK_CLEAR) ||
                        (!player.Duel.IsMyDeck && chapterStatus == ChapterStatus.RENTAL_CLEAR))
                    {
                        duelSettings.FirstPlayer = -1;
                    }
                    else if (player.Duel.IsMyDeck)
                    {
                        // As GetSoloDuelSettings is called over multiple functions this might show a different value in different places?
                        // i.e. "XXX is going first" then a different player starts first in-game
                        duelSettings.FirstPlayer = rand.Next(2);
                    }
                }

                FileInfo customDuelFile = new FileInfo(Path.Combine(dataDirectory, "CustomDuel.json"));
                if (chapterStatus == ChapterStatus.COMPLETE && customDuelFile.Exists)
                {
                    if (CustomDuelSettings == null || customDuelFile.LastWriteTime != CustomDuelLastModified)
                    {
                        CustomDuelLastModified = customDuelFile.LastWriteTime;
                        try
                        {
                            Dictionary<string, object> duelData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(customDuelFile.FullName)) as Dictionary<string, object>;
                            object deckListObj;
                            int targetChapterId;
                            if (duelData.TryGetValue("Deck", out deckListObj) &&
                                Utils.TryGetValue(duelData, "targetChapterId", out targetChapterId) &&
                                targetChapterId == chapterId)
                            {
                                List<object> deckList = deckListObj as List<object>;
                                if (deckList.Count >= 2)
                                {
                                    List<DeckInfo> decks = new List<DeckInfo>();
                                    for (int i = 0; i < deckList.Count; i++)
                                    {
                                        string deckFile = deckList[i] as string;
                                        if (deckFile != null)
                                        {
                                            string possibleFile = Path.Combine(dataDirectory, deckFile);
                                            if (!File.Exists(possibleFile))
                                            {
                                                possibleFile = Path.Combine(dataDirectory, deckFile + ".json");
                                            }
                                            if (!File.Exists(possibleFile))
                                            {
                                                possibleFile = Path.Combine(dataDirectory, deckFile + ".ydk");
                                            }
                                            if (!File.Exists(possibleFile))
                                            {
                                                foreach (DeckInfo playerDeck in player.Decks.Values)
                                                {
                                                    if (Path.GetFileNameWithoutExtension(playerDeck.File) == deckFile)
                                                    {
                                                        possibleFile = playerDeck.File;
                                                        break;
                                                    }
                                                    if (playerDeck.Name == deckFile)
                                                    {
                                                        possibleFile = playerDeck.Name;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (File.Exists(possibleFile))
                                            {
                                                DeckInfo deckInfo = new DeckInfo();
                                                deckInfo.File = possibleFile;
                                                deckInfo.Load();
                                                decks.Add(deckInfo);
                                            }
                                        }
                                    }
                                    duelData.Remove("Deck");

                                    if (decks.Count == 2)
                                    {
                                        DuelSettings customDuel = new DuelSettings();
                                        for (int i = 0; i < decks.Count && i < DuelSettings.MaxPlayers; i++)
                                        {
                                            customDuel.Deck[i] = decks[i];
                                        }
                                        customDuel.IsCustomDuel = true;
                                        customDuel.FromDictionary(duelData);
                                        customDuel.SetRequiredDefaults();
                                        customDuel.chapter = 0;
                                        CustomDuelSettings = customDuel;
                                        return customDuel;
                                    }
                                }
                                Utils.LogWarning("Failed to load custom duel");
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (CustomDuelSettings != null)
                    {
                        return CustomDuelSettings;
                    }
                }
            }
            return duelSettings;
        }

        DuelSettings CreateSoloDuelSettingsInstance(Player player, int chapterId)
        {
            DuelSettings duelSettings = null;
            PlayerDuelState duel = player.Duel;
            DuelSettings ds = GetSoloDuelSettings(player, chapterId);
            if (ds != null)
            {
                duelSettings = new DuelSettings();
                duelSettings.CopyFrom(ds);
                if (SoloRemoveDuelTutorials)
                {
                    duelSettings.chapter = 0;
                }
                if (SoloDisableNoShuffle)
                {
                    duelSettings.noshuffle = false;
                }
                if (duel.IsMyDeck && !duelSettings.IsCustomDuel)
                {
                    DeckInfo deck = duel.GetDeck(GameMode.SoloSingle);
                    if (deck != null)
                    {
                        duelSettings.Deck[DuelSettings.PlayerIndex].CopyFrom(deck);
                        duelSettings.avatar_home[DuelSettings.PlayerIndex] = deck.Accessory.AvBase;
                        duelSettings.sleeve[DuelSettings.PlayerIndex] = deck.Accessory.Sleeve;
                        duelSettings.mat[DuelSettings.PlayerIndex] = deck.Accessory.Field;
                        duelSettings.duel_object[DuelSettings.PlayerIndex] = deck.Accessory.FieldObj;
                        duelSettings.story_deck_id[DuelSettings.PlayerIndex] = 0;
                    }
                }
                duelSettings.avatar[DuelSettings.PlayerIndex] = player.AvatarId;
                duelSettings.icon[DuelSettings.PlayerIndex] = player.IconId;
                duelSettings.icon_frame[DuelSettings.PlayerIndex] = player.IconFrameId;
                duelSettings.wallpaper[DuelSettings.PlayerIndex] = player.Wallpaper;
            }
            return duelSettings;
        }

        void Act_DuelBegin(GameServerWebRequest request)
        {
            //request.StringResponse = @"{""code"":0,""res"":[[10,{""Duel"":{""IsCustomDuel"":true,""OpponentType"":2,""OpponentPartnerType"":0,""RandSeed"":2081258956,""FirstPlayer"":1,""noshuffle"":false,""tag"":false,""dlginfo"":false,""MyID"":0,""MyType"":0,""Type"":0,""MyPartnerType"":1,""PlayableTagPartner"":0,""regulation_id"":0,""duel_start_timestamp"":0,""surrender"":true,""Limit"":0,""GameMode"":0,""cpu"":100,""cpuflag"":null,""LeftTimeMax"":0,""TurnTimeMax"":0,""TotalTimeMax"":0,""Auto"":-1,""rec"":false,""recf"":false,""did"":0,""duel"":null,""is_pvp"":false,""chapter"":0,""bgms"":[],""Deck"":[{""name"":""new ydk"",""ct"":0,""et"":1645640915,""accessory"":{""box"":1080001,""sleeve"":1070001,""field"":1090001,""object"":1100001,""av_base"":0},""pick_cards"":{""ids"":{""1"":0,""2"":0,""3"":0},""r"":{""1"":1,""2"":1,""3"":1}},""Main"":{""CardIds"":[4007,4007,4007,4711,4711,9455,9455,9455,9063,9229,9229,11308,11308,14259,14587,14911,9190,9190,9190,6949,6949,12901,4844,4844,4844,5328,6432,7187,7381,14304,14304,14304,16405,16405,9066,9066,9066,13619,13619,15299,15299],""Rare"":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]},""Extra"":{""CardIds"":[11313,11313,11930,12694,14910,8129,9196,9272,11648,12705,14586,13543,14295,14295,14052],""Rare"":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]},""Side"":{""CardIds"":[],""Rare"":[]}},{""name"":""241244.ydk"",""ct"":0,""et"":0,""accessory"":{""box"":0,""sleeve"":0,""field"":0,""object"":0,""av_base"":0},""pick_cards"":{""ids"":{""1"":0,""2"":0,""3"":0},""r"":{""1"":1,""2"":1,""3"":1}},""Main"":{""CardIds"":[12950,12950,12950,8933,8933,9455,9455,9455,13670,13670,13670,14829,14829,11851,14876,14876,13680,13672,13673,13676,13674,13677,13677,13677,13675,13675,4343,13679,13679,13671,13671,5537,4895,5328,13631,13631,13447,13447,4817,4817],""Rare"":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]},""Extra"":{""CardIds"":[13763,13763,13763,13668,13669,13669,13669,14947,13508,13600,14132,14133,13089,15032,14937],""Rare"":[1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]},""Side"":{""CardIds"":[],""Rare"":[]}},{""name"":null,""ct"":0,""et"":0,""accessory"":{""box"":0,""sleeve"":0,""field"":0,""object"":0,""av_base"":0},""pick_cards"":{""ids"":{""1"":0,""2"":0,""3"":0},""r"":{""1"":1,""2"":1,""3"":1}},""Main"":{""CardIds"":[],""Rare"":[]},""Extra"":{""CardIds"":[],""Rare"":[]},""Side"":{""CardIds"":[],""Rare"":[]}},{""name"":null,""ct"":0,""et"":0,""accessory"":{""box"":0,""sleeve"":0,""field"":0,""object"":0,""av_base"":0},""pick_cards"":{""ids"":{""1"":0,""2"":0,""3"":0},""r"":{""1"":1,""2"":1,""3"":1}},""Main"":{""CardIds"":[],""Rare"":[]},""Extra"":{""CardIds"":[],""Rare"":[]},""Side"":{""CardIds"":[],""Rare"":[]}}],""reg"":[0,0,0,0],""level"":[0,0,0,0],""follow_num"":[0,0,0,0],""pcode"":[0,0,0,0],""rank"":[0,0,0,0],""DuelistLv"":[0,0,0,0],""name"":[""Duelist"",null,null,null],""avatar"":[0,0,0,0],""avatar_home"":[0,0,0,0],""icon"":[1100001,1100001,1100001,1100001],""icon_frame"":[1030001,1030001,1030001,1030001],""sleeve"":[1070001,1070001,1070001,1070001],""mat"":[1090001,1090001,1090001,1090001],""duel_object"":[1100001,1100001,1100001,1100001],""wallpaper"":[1130001,1130001,1130001,1130001],""profile_tag"":[[],[],[],[]],""story_deck_id"":[0,0,0,0]}},0,0]],""remove"":[""Duel"",""DuelResult"",""Result""]}";
            Dictionary<string, object> rule;
            if (Utils.TryGetValue(request.ActParams, "rule", out rule))
            {
                PlayerDuelState duel = request.Player.Duel;
                duel.Mode = (GameMode)Utils.GetValue<int>(rule, "GameMode");
                duel.ChapterId = Utils.GetValue<int>(rule, "chapter");
                DuelSettings duelSettings = null;
                Dictionary<string, object> duelStarterData = Utils.GetDictionary(rule, "duelStarterData");
                if (duelStarterData != null)
                {
                    duelSettings = new DuelSettings();
                    duelSettings.FromDictionary(duelStarterData);
                    duelSettings.IsCustomDuel = true;
                    duelSettings.SetRequiredDefaults();
                }
                else
                {
                    switch (duel.Mode)
                    {
                        case GameMode.SoloSingle:
                            duelSettings = CreateSoloDuelSettingsInstance(request.Player, duel.ChapterId);
                            break;
                    }
                }
                if (duelSettings != null)
                {
                    int firstPlayer;
                    if (Utils.TryGetValue(rule, "FirstPlayer", out firstPlayer))
                    {
                        duelSettings.FirstPlayer = firstPlayer;
                    }
                    if (duelSettings.FirstPlayer <= -1)
                    {
                        duelSettings.FirstPlayer = rand.Next(2);
                    }
                    if (!duelSettings.IsCustomDuel || string.IsNullOrEmpty(duelSettings.name[DuelSettings.PlayerIndex]))
                    {
                        duelSettings.name[DuelSettings.PlayerIndex] = request.Player.Name;
                    }
                    if (!duelSettings.IsCustomDuel || duelSettings.RandSeed == 0)
                    {
                        duelSettings.RandSeed = (uint)rand.Next();
                    }
                    if (duelSettings.bgms.Count == 0)
                    {
                        duelSettings.SetRandomBgm(rand);
                    }
                    request.Response["Duel"] = duelSettings.ToDictionary();
                }
            }
            request.Remove("Duel", "DuelResult", "Result");
        }

        void Act_DuelEnd(GameServerWebRequest request)
        {
            DuelResultType res;
            DuelFinishType finish;
            Dictionary<string, object> endParams;
            if (Utils.TryGetValue(request.ActParams, "params", out endParams) &&
                Utils.TryGetValue(endParams, "res", out res) &&
                Utils.TryGetValue(endParams, "finish", out finish))
            {
                switch (request.Player.Duel.Mode)
                {
                    case GameMode.SoloSingle:

                        bool chapterStatusChanged = false;
                        if (request.Player.Duel.ChapterId != 0 && res != DuelResultType.None)
                        {
                            ChapterStatus oldChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out oldChapterStatus);
                            SoloUpdateChapterStatus(request, request.Player.Duel.ChapterId, res, finish);
                            ChapterStatus newChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out newChapterStatus);
                            chapterStatusChanged = oldChapterStatus != newChapterStatus;
                        }
                        GiveDuelReward(request, DuelRewards, res, finish, chapterStatusChanged);
                        SavePlayer(request.Player);
                        break;
                }
            }
        }

        void GiveDuelReward(GameServerWebRequest request, DuelRewardInfos rewards, DuelResultType result, DuelFinishType finishType, bool chapterStatusChanged)
        {
            if ((rewards.Win.Count == 0 && rewards.Lose.Count == 0) ||
                (rewards.ChapterStatusChangedNoRewards && chapterStatusChanged) ||
                (rewards.ChapterStatusChangedOnly && !chapterStatusChanged) ||
                (result == DuelResultType.Lose && finishType == DuelFinishType.Surrender))
            {
                return;
            }

            request.Remove("Duel", "Solo.Result");

            Dictionary<string, object> duel = request.GetOrCreateDictionary("Duel");
            duel["result"] = 1;
            Dictionary<string, object> duelResult = request.GetOrCreateDictionary("DuelResult");
            duelResult["mode"] = (int)GameMode.Normal;// Use anything other than SoloSingle (as it only shows level / exp)
            Dictionary<string, object> duelResultInfo = Utils.GetOrCreateDictionary(duelResult, "resultInfo");
            duelResultInfo["result"] = 1;
            Dictionary<string, object> duelScoreInfo = Utils.GetOrCreateDictionary(duelResultInfo, "scoreInfo");
            Dictionary<string, object> duelScore = Utils.GetOrCreateDictionary(duelScoreInfo, "score");
            int duelScoreTotal = Utils.GetValue<int>(duelScore, "total");
            List<object> duelRewards = Utils.GetOrCreateList(duelScoreInfo, "rewards");

            // 1 = blue box, 2 = gold box, 3 = breaks client (no back button)
            const int blueBox = 1;
            const int goldBox = 2;
            const int duelScoreRewardValue = 1000;

            foreach (DuelRewardInfo reward in (result == DuelResultType.Win ? rewards.Win : rewards.Lose))
            {
                switch (reward.Type)
                {
                    case DuelCustomRewardType.Gem:
                        {
                            double randValue = rand.NextDouble() * 100;
                            if (reward.Rate >= randValue)
                            {
                                int amount = rewards.GetAmount(rand, reward, chapterStatusChanged);
                                if (amount == 0)
                                {
                                    continue;
                                }
                                request.Player.Gems += amount;
                                WriteItem(request, (int)ItemID.Value.Gem);
                                duelScoreTotal += duelScoreRewardValue;
                                duelRewards.Add(new Dictionary<string, object>()
                                {
                                    { "type", reward.Rare ? goldBox : blueBox },
                                    { "category", (int)ItemID.Category.CONSUME },
                                    { "item_id", (int)ItemID.Value.Gem },
                                    { "num", amount },
                                    { "is_prize", true },
                                });
                            }
                        }
                        break;
                    case DuelCustomRewardType.Item:
                        {
                            double randValue = rand.NextDouble() * 100;
                            if (reward.Rate >= randValue)
                            {
                                HashSet<int> unownedIds = new HashSet<int>();
                                if (reward.Ids != null && reward.Ids.Count > 0)
                                {
                                    foreach (int id in reward.Ids)
                                    {
                                        if (!request.Player.Items.Contains(id))
                                        {
                                            unownedIds.Add(id);
                                        }
                                    }
                                }
                                else
                                {
                                    // Also see LoadPlayer which does the same thing (loads all items)
                                    ItemID.Category[] categories =
                                    {
                                        ItemID.Category.AVATAR,
                                        ItemID.Category.ICON,
                                        ItemID.Category.ICON_FRAME,
                                        ItemID.Category.PROTECTOR,
                                        ItemID.Category.DECK_CASE,
                                        ItemID.Category.FIELD,
                                        ItemID.Category.FIELD_OBJ,
                                        ItemID.Category.AVATAR_HOME,
                                        ItemID.Category.WALLPAPER,
                                    };
                                    foreach (ItemID.Category category in categories)
                                    {
                                        foreach (int id in ItemID.Values[category])
                                        {
                                            if (!request.Player.Items.Contains(id))
                                            {
                                                unownedIds.Add(id);
                                            }
                                        }
                                    }
                                }
                                if (unownedIds.Count > 0)
                                {
                                    int amount = 1;
                                    int id = unownedIds.ElementAt(rand.Next(unownedIds.Count));
                                    if (ItemID.GetCategoryFromID(id) == ItemID.Category.STRUCTURE)
                                    {
                                        GiveStructureDeck(request, id);
                                    }
                                    else if (ItemID.GetCategoryFromID(id) == ItemID.Category.CONSUME)
                                    {
                                        amount = rewards.GetAmount(rand, reward, chapterStatusChanged);
                                        if (amount == 0)
                                        {
                                            continue;
                                        }
                                        request.Player.AddItem(id, amount);
                                    }
                                    else
                                    {
                                        request.Player.Items.Add(id);
                                        WriteItem(request, id);
                                    }
                                    duelScoreTotal += duelScoreRewardValue;
                                    duelRewards.Add(new Dictionary<string, object>()
                                    {
                                        { "type", reward.Rare ? goldBox : blueBox },
                                        { "category", (int)ItemID.GetCategoryFromID(id) },
                                        { "item_id", id },
                                        { "num", amount },
                                        { "is_prize", true },
                                    });
                                }
                            }
                        }
                        break;
                    case DuelCustomRewardType.Card:
                        {
                            PlayerCardKind dismantle = reward.CardNoDismantle && !DisableNoDismantle ? PlayerCardKind.NoDismantle : PlayerCardKind.Dismantle;
                            int numCards = Math.Max(1, Math.Min(reward.MinValue, reward.MaxValue));
                            double randValue = rand.NextDouble() * 100;
                            if (reward.Rate >= randValue)
                            {
                                if (reward.Ids != null && reward.Ids.Count > 0)
                                {
                                    HashSet<int> cardIds = new HashSet<int>();
                                    foreach (int id in reward.Ids)
                                    {
                                        if (reward.CardOwnedLimit == 0 || reward.CardOwnedLimit > request.Player.Cards.GetCount(id))
                                        {
                                            cardIds.Add(id);
                                        }
                                    }
                                    if (cardIds.Count > 0)
                                    {
                                        int cardId = cardIds.ElementAt(rand.Next(cardIds.Count));
                                        request.Player.Cards.Add(cardId, numCards, dismantle, CardStyleRarity.Normal);
                                        WriteCards_have(request, cardId);
                                        duelScoreTotal += duelScoreRewardValue;
                                        duelRewards.Add(new Dictionary<string, object>()
                                        {
                                            { "type", reward.Rare ? goldBox : blueBox },
                                            { "category", (int)ItemID.Category.CARD },
                                            { "item_id", cardId },
                                            { "num", numCards },
                                            { "is_prize", true },
                                        });
                                    }
                                }
                                else if (reward.CardRate != null && reward.CardRate.Count > 0)
                                {
                                    Dictionary<CardRarity, double> accumaltiveRate = new Dictionary<CardRarity, double>();
                                    Dictionary<CardRarity, bool> rare = new Dictionary<CardRarity, bool>();
                                    double totalRate = 0;
                                    for (int i = 0; i < reward.CardRate.Count; i++)
                                    {
                                        totalRate += reward.CardRate[i];
                                        accumaltiveRate[(CardRarity)i + 1] = totalRate;
                                        if (reward.CardRare != null && reward.CardRare.Count > i)
                                        {
                                            rare[(CardRarity)i + 1] = reward.CardRare[i];
                                        }
                                    }
                                    double cardRate = rand.NextDouble() * 100;
                                    CardRarity cardRarity = CardRarity.None;
                                    bool isRare = reward.Rare;
                                    foreach (KeyValuePair<CardRarity, double> rate in accumaltiveRate.OrderBy(x => x.Key))
                                    {
                                        if (cardRate < rate.Value)
                                        {
                                            cardRarity = rate.Key;
                                            if (rare.ContainsKey(rate.Key))
                                            {
                                                isRare = rare[rate.Key];
                                            }
                                            break;
                                        }
                                    }
                                    if (cardRarity != CardRarity.None)
                                    {
                                        Dictionary<int, int> cardRare = GetCardRarities(request.Player);
                                        List<int> cardIds = new List<int>();
                                        foreach (KeyValuePair<int, int> card in cardRare)
                                        {
                                            if (card.Value == (int)cardRarity && (reward.CardOwnedLimit == 0 ||
                                                reward.CardOwnedLimit < request.Player.Cards.GetCount(card.Key)))
                                            {
                                                cardIds.Add(card.Key);
                                            }
                                        }
                                        if (cardIds.Count > 0)
                                        {
                                            int cardId = cardIds[rand.Next(cardIds.Count)];
                                            request.Player.Cards.Add(cardId, numCards, dismantle, CardStyleRarity.Normal);
                                            WriteCards_have(request, cardId);
                                            duelScoreTotal += duelScoreRewardValue;
                                            duelRewards.Add(new Dictionary<string, object>()
                                            {
                                                { "type", isRare ? goldBox : blueBox },
                                                { "category", (int)ItemID.Category.CARD },
                                                { "item_id", cardId },
                                                { "num", numCards },
                                                { "is_prize", true },
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            duelScore["total"] = duelScoreTotal;
        }
    }
}
