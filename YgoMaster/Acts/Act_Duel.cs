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
                player.SoloChapters.TryGetValue(chapterId, out chapterStatus) &&
                chapterStatus == ChapterStatus.COMPLETE)
            {
                FileInfo customDuelFile = new FileInfo(Path.Combine(dataDirectory, "CustomDuel.json"));
                if (customDuelFile.Exists)
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
                                TryGetValue(duelData, "targetChapterId", out targetChapterId) &&
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
                                                LoadDeck(deckInfo);
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
                                LogWarning("Failed to load custom duel");
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
            Dictionary<string, object> rule;
            if (TryGetValue(request.ActParams, "rule", out rule))
            {
                PlayerDuelState duel = request.Player.Duel;
                duel.Mode = (GameMode)GetValue<int>(rule, "GameMode");
                duel.ChapterId = GetValue<int>(rule, "chapter");
                DuelSettings duelSettings = null;
                switch (duel.Mode)
                {
                    case GameMode.SoloSingle:
                        duelSettings = CreateSoloDuelSettingsInstance(request.Player, duel.ChapterId);
                        break;
                }
                if (duelSettings != null)
                {
                    int firstPlayer;
                    if (TryGetValue(rule, "FirstPlayer", out firstPlayer))
                    {
                        duelSettings.FirstPlayer = firstPlayer;
                    }
                    else if (duelSettings.FirstPlayer == -1)
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
                    request.Response["Duel"] = duelSettings.ToDictionary();
                }
            }
            request.Remove("Duel", "DuelResult", "Result");
        }

        void Act_DuelEnd(GameServerWebRequest request)
        {
            int res, finish;
            Dictionary<string, object> endParams;
            if (TryGetValue(request.ActParams, "params", out endParams) &&
                TryGetValue(endParams, "res", out res) &&
                TryGetValue(endParams, "finish", out finish))
            {
                switch (request.Player.Duel.Mode)
                {
                    case GameMode.SoloSingle:
                        bool chapterStatusChanged = false;
                        if (request.Player.Duel.ChapterId != 0 && res != (int)DuelResultType.None)
                        {
                            ChapterStatus oldChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out oldChapterStatus);
                            SoloUpdateChapterStatus(request, request.Player.Duel.ChapterId, (DuelResultType)res);
                            ChapterStatus newChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out newChapterStatus);
                            chapterStatusChanged = oldChapterStatus != newChapterStatus;
                        }
                        GiveDuelReward(request, DuelRewards, (DuelResultType)res, chapterStatusChanged);
                        SavePlayer(request.Player);
                        break;
                }
            }
        }

        void GiveDuelReward(GameServerWebRequest request, DuelRewardInfos rewards, DuelResultType result, bool chapterStatusChanged)
        {
            if ((rewards.Win.Count == 0 && rewards.Lose.Count == 0) ||
                (rewards.ChapterStatusChangedNoRewards && chapterStatusChanged) ||
                (rewards.ChapterStatusChangedOnly && !chapterStatusChanged))
            {
                return;
            }

            request.Remove("Duel", "Solo.Result");

            Dictionary<string, object> duel = request.GetOrCreateDictionary("Duel");
            duel["result"] = 1;
            Dictionary<string, object> duelResult = request.GetOrCreateDictionary("DuelResult");
            duelResult["mode"] = (int)GameMode.Normal;// Use anything other than SoloSingle (as it only shows level / exp)
            Dictionary<string, object> duelResultInfo = GetOrCreateDictionary(duelResult, "resultInfo");
            duelResultInfo["result"] = 1;
            Dictionary<string, object> duelScoreInfo = GetOrCreateDictionary(duelResultInfo, "scoreInfo");
            Dictionary<string, object> duelScore = GetOrCreateDictionary(duelScoreInfo, "score");
            int duelScoreTotal = GetValue<int>(duelScore, "total");
            List<object> duelRewards = GetOrCreateList(duelScoreInfo, "rewards");

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
                                WriteItem(request, (int)ItemID.CONSUME.ID0001);
                                duelScoreTotal += duelScoreRewardValue;
                                duelRewards.Add(new Dictionary<string, object>()
                                {
                                    { "type", reward.Rare ? goldBox : blueBox },
                                    { "category", (int)ItemID.Category.CONSUME },
                                    { "item_id", (int)ItemID.CONSUME.ID0001 },
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
                                    Type[] enumTypes =
                                    {
                                        typeof(ItemID.AVATAR),
                                        typeof(ItemID.ICON),
                                        typeof(ItemID.ICON_FRAME),
                                        typeof(ItemID.PROTECTOR),
                                        typeof(ItemID.DECK_CASE),
                                        typeof(ItemID.FIELD),
                                        typeof(ItemID.FIELD_OBJ),
                                        typeof(ItemID.AVATAR_HOME),
                                        typeof(ItemID.WALLPAPER),
                                    };
                                    foreach (Type enumType in enumTypes)
                                    {
                                        foreach (int id in Enum.GetValues(enumType))
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
                            PlayerCardKind dismantle = reward.CardNoDismantle ? PlayerCardKind.NoDismantle : PlayerCardKind.Dismantle;
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
