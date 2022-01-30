using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Look into:
// SoloSelectChapterViewController
// YgomGame.Solo.mainScroll.SoloModeViewController (InfinityScrollView)
// YgomSystem.UI.InfinityScroll.InfinityScrollView

namespace YgoMaster
{
    partial class GameServer
    {
        internal static int GetChapterGateId(int chapterId)
        {
            return chapterId / 10000;
        }

        bool IsValidNonDuelChapter(int chapterId)
        {
            int gateId = GetChapterGateId(chapterId);
            Dictionary<string, object> allChapterData = GetDictionary(SoloData, "chapter");
            if (allChapterData == null)
            {
                return false;
            }
            Dictionary<string, object> chapterGateData = GetDictionary(allChapterData, gateId.ToString());
            if (chapterGateData == null)
            {
                return false;
            }
            Dictionary<string, object> chapterData = GetDictionary(chapterGateData, chapterId.ToString());
            if (chapterData == null)
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(GetValue<string>(chapterData, "begin_sn")))
            {
                // This is a scenario chapter
                return true;
            }
            if (GetValue<int>(chapterData, "unlock_id") != 0)
            {
                // This is a lock
                return true;
            }
            Dictionary<string, object> allGateData = GetDictionary(SoloData, "gate");
            if (allGateData == null)
            {
                return false;
            }
            Dictionary<string, object> gateData = GetDictionary(allGateData, gateId.ToString());
            if (gateData == null)
            {
                return false;
            }
            if (GetValue<int>(gateData, "clear_chapter") == chapterId)
            {
                // This is the goal/clear chapter
                return true;
            }
            return false;
        }

        void OnSoloChapterComplete(GameServerWebRequest request, int chapterId)
        {
            // TODO: "Solo.Result.gate_clear" when finishing goal gates (unlock a random secret)
            // "gate_clear":{"gate":6,"pack":{"nameTextId":"IDS_CARDPACK_ID2081_NAME","shopId":10002081,"iconMrk":13579,"is_extend":true}}

            ChapterStatus currentStatus;
            if (request.Player.SoloChapters.TryGetValue(chapterId, out currentStatus))
            {
                switch (currentStatus)
                {
                    case ChapterStatus.MYDECK_CLEAR:
                        if (request.Player.ActiveDuel.IsMyDeck)
                        {
                            return;
                        }
                        break;
                    case ChapterStatus.RENTAL_CLEAR:
                        if (!request.Player.ActiveDuel.IsMyDeck)
                        {
                            return;
                        }
                        break;
                    case ChapterStatus.COMPLETE:
                        return;
                }
            }

            int gateId = GetChapterGateId(chapterId);

            ChapterStatus newStatus = ChapterStatus.OPEN;
            Dictionary<string, object> allChapterData = GetDictionary(SoloData, "chapter");
            if (allChapterData == null)
            {
                LogWarning("Failed to get all chapter data");
                return;
            }
            Dictionary<string, object> chapterGateData = GetDictionary(allChapterData, gateId.ToString());
            if (chapterGateData == null)
            {
                LogWarning("Failed to get gate data");
                return;
            }
            Dictionary<string, object> chapterData = GetDictionary(chapterGateData, chapterId.ToString());
            if (chapterData == null)
            {
                LogWarning("Failed to get chapter data");
                return;
            }
            int myDeckSetId = GetValue<int>(chapterData, "mydeck_set_id");
            int loanerDeckSetId = GetValue<int>(chapterData, "set_id");
            int targetSetId = 0;
            if (request.Player.ActiveDuel.IsMyDeck)
            {
                if (myDeckSetId == 0)
                {
                    LogWarning("Completed chapter with own deck but no option for it");
                    return;
                }
                targetSetId = myDeckSetId;
                if (loanerDeckSetId == 0 || currentStatus > ChapterStatus.OPEN)
                {
                    newStatus = ChapterStatus.COMPLETE;
                }
                else
                {
                    newStatus = ChapterStatus.MYDECK_CLEAR;
                }
            }
            else
            {
                if (loanerDeckSetId == 0)
                {
                    LogWarning("Completed chapter with loaner deck but no option for it");
                    return;
                }
                targetSetId = loanerDeckSetId;
                if (myDeckSetId == 0 || currentStatus > ChapterStatus.OPEN)
                {
                    newStatus = ChapterStatus.COMPLETE;
                }
                else
                {
                    newStatus = ChapterStatus.RENTAL_CLEAR;
                }
            }
            Dictionary<string, object> allRewardData = GetDictionary(SoloData, "reward");
            if (allRewardData == null)
            {
                LogWarning("Failed to get all reward data");
                return;
            }
            List<object> resultRewards = new List<object>();
            Dictionary<string, object> rewardData = GetDictionary(allRewardData, targetSetId.ToString());
            foreach (KeyValuePair<string, object> reward in rewardData)
            {
                Dictionary<string, object> rewardItems = reward.Value as Dictionary<string, object>;
                int rewardCategoryI32;
                if (!int.TryParse(reward.Key, out rewardCategoryI32))
                {
                    continue;
                }
                ItemID.Category category = (ItemID.Category)rewardCategoryI32;
                foreach (KeyValuePair<string, object> item in rewardItems)
                {
                    int itemId;
                    int count = (int)Convert.ChangeType(item.Value, typeof(int));
                    if (!int.TryParse(item.Key, out itemId))
                    {
                        continue;
                    }
                    bool valid = true;
                    switch (category)
                    {
                        case ItemID.Category.CONSUME:
                            {
                                switch ((ItemID.CONSUME)itemId)
                                {
                                    case ItemID.CONSUME.ID0001: request.Player.Gems += count; break;
                                    case ItemID.CONSUME.ID0003: request.Player.CraftPoints.Add(CardRarity.Normal, count); break;
                                    case ItemID.CONSUME.ID0004: request.Player.CraftPoints.Add(CardRarity.Rare, count); break;
                                    case ItemID.CONSUME.ID0005: request.Player.CraftPoints.Add(CardRarity.SuperRare, count); break;
                                    case ItemID.CONSUME.ID0006: request.Player.CraftPoints.Add(CardRarity.UltraRare, count); break;
                                    case ItemID.CONSUME.ID0008: request.Player.OrbPoints.Add(OrbType.Dark, count); break;
                                    case ItemID.CONSUME.ID0009: request.Player.OrbPoints.Add(OrbType.Light, count); break;
                                    case ItemID.CONSUME.ID0010: request.Player.OrbPoints.Add(OrbType.Earth, count); break;
                                    case ItemID.CONSUME.ID0011: request.Player.OrbPoints.Add(OrbType.Water, count); break;
                                    case ItemID.CONSUME.ID0012: request.Player.OrbPoints.Add(OrbType.Fire, count); break;
                                    case ItemID.CONSUME.ID0013: request.Player.OrbPoints.Add(OrbType.Wind, count); break;
                                    default:
                                        LogWarning("Unhandled CONSUME item " + itemId);
                                        valid = false;
                                        break;
                                }
                            }
                            break;
                        case ItemID.Category.CARD:
                            {
                                request.Player.Cards.Add(itemId, count, PlayerCardKind.Dismantle, CardStyleRarity.Normal);
                            }
                            break;
                        case ItemID.Category.STRUCTURE:
                            {
                                count = 1;
                                GiveStructureDeck(request, itemId);
                            }
                            break;
                        case ItemID.Category.AVATAR:
                        case ItemID.Category.ICON:
                        case ItemID.Category.ICON_FRAME:
                        case ItemID.Category.PROTECTOR:
                        case ItemID.Category.DECK_CASE:
                        case ItemID.Category.AVATAR_HOME:
                            {
                                count = 1;
                                request.Player.Items.Add(itemId);
                            }
                            break;
                        case ItemID.Category.FIELD:
                            count = 1;
                            foreach (int fieldPartItemId in ItemID.GetDuelFieldParts(itemId))
                            {
                                request.Player.Items.Add(fieldPartItemId);
                            }
                            break;
                        case ItemID.Category.PROFILE_TAG:
                            // Ignore (currently all tags are auto added)
                            valid = false;
                            break;
                        case ItemID.Category.PACK_TICKET:
                            // Ignore (currently don't handle pack tickets)
                            valid = false;
                            break;
                        default:
                            LogWarning("Unhandled reward category " + category);
                            valid = false;
                            break;
                    }
                    if (valid)
                    {
                        WriteItem(request, itemId);
                        resultRewards.Add(new Dictionary<string, object>()
                        {
                            { "category", (int)category },
                            { "item_id", itemId },
                            { "num", count },
                            { "type", (int)category },
                        });
                    }
                }
            }
            request.Player.SoloChapters[chapterId] = newStatus;
            SavePlayer(request.Player);

            Dictionary<string, object> soloData = request.GetOrCreateDictionary("Solo");
            Dictionary<string, object> clearedData = GetOrCreateDictionary(soloData, "cleared");
            Dictionary<string, object> gateClearedData = GetOrCreateDictionary(clearedData, gateId.ToString());
            gateClearedData[chapterId.ToString()] = (int)newStatus;

            Dictionary<string, object> resultData = GetOrCreateDictionary(soloData, "Result");
            resultData["rewards"] = resultRewards;

            /*Dictionary<string, object> duelResultData = request.GetOrCreateDictionary("DuelResult");
            duelResultData["mode"] = (int)GameMode.SoloSingle;
            Dictionary<string, object> resultInfo = GetOrCreateDictionary(duelResultData, "resultInfo");
            resultInfo["result"] = 1;*/

            request.Remove("Solo.Result");
        }

        void Act_SoloInfo(GameServerWebRequest request)
        {
            if (GetValue<bool>(request.ActParams, "back"))
            {
                return;
            }
            request.Response["Master"] = new Dictionary<string, object>()
            {
                { "Solo", SoloData }
            };
            request.Response["Solo"] = new Dictionary<string, object>()
            {
                { "deck_info", new Dictionary<string, object>() {
                    { "deck_id", request.Player.ActiveDuel.DeckId },
                    { "valid", true },
                    { "possession", request.Player.ActiveDuel.IsMyDeck }
                }},
                { "cleared", request.Player.SoloChaptersToDictionary() }
            };
            request.Remove("Master.solo", "Solo");
        }

        void Act_SoloDetail(GameServerWebRequest request)
        {
            int chapterId;
            if (TryGetValue(request.ActParams, "chapter", out chapterId))
            {
                Dictionary<string, object> chapterInfo = new Dictionary<string, object>();
                DuelSettings duel;
                if (SoloDuels.TryGetValue(chapterId, out duel))
                {
                    if (duel.Deck[DuelSettings.PlayerIndex].MainDeckCards.Count > 0)
                    {
                        chapterInfo["story_deck"] = duel.Deck[DuelSettings.PlayerIndex].ToDictionary();
                        chapterInfo["story_deck_id"] = duel.Deck[DuelSettings.PlayerIndex].Id;
                    }
                    if (duel.Deck[DuelSettings.CpuIndex].MainDeckCards.Count > 0)
                    {
                        chapterInfo["npc_deck"] = duel.Deck[DuelSettings.CpuIndex].ToDictionary();
                        chapterInfo["npc_deck_id"] = duel.Deck[DuelSettings.CpuIndex].Id;
                    }
                    if (duel.FirstPlayer >= 0)
                    {
                        chapterInfo["FirstPlayer"] = duel.FirstPlayer;
                    }
                }
                else if (!IsValidNonDuelChapter(chapterId))
                {
                    LogWarning("Failed to find info for chapter " + chapterId);
                }
                request.Response["Solo"] = new Dictionary<string, object>()
                {
                    { "chapter", new Dictionary<string, object>() {
                        { chapterId.ToString(), chapterInfo }
                    }}
                };
            }
        }

        void Act_SoloSetUseDeckType(GameServerWebRequest request)
        {
            int chapterId, deckType;
            if (TryGetValue(request.ActParams, "chapter", out chapterId) && TryGetValue(request.ActParams, "deck_type", out deckType))
            {
                request.Player.ActiveDuel.IsMyDeck = (SoloDeckType)deckType == SoloDeckType.POSSESSION;
            }
            request.Remove("Solo.rental_deck");
        }

        void Act_SoloStart(GameServerWebRequest request)
        {
            int chapterId;
            if (TryGetValue<int>(request.ActParams, "chapter", out chapterId))
            {
                DuelSettings duel;
                if (SoloDuels.TryGetValue(chapterId, out duel))
                {
                    request.Response["Duel"] = new Dictionary<string, object>()
                    {
                        { "avatar", duel.avatar },
                        { "icon", duel.icon },
                        { "icon_frame", duel.icon_frame },
                        { "sleeve", duel.sleeve },
                        { "mat", duel.mat },
                        { "duel_object", duel.duel_object },
                        { "avatar_home", duel.avatar_home }
                    };
                }
                else if (IsValidNonDuelChapter(chapterId))
                {
                    OnSoloChapterComplete(request, chapterId);
                }
                else
                {
                    LogWarning("Failed to start chapter " + chapterId);
                }
            }
            request.Remove("Solo.Result", "Duel", "DuelResult");
        }

        void Act_SoloSkip(GameServerWebRequest request)
        {
            int chapterId;
            if (TryGetValue<int>(request.ActParams, "chapter", out chapterId))
            {
                // Assumes the client knows what it should / shouldn't skip
                OnSoloChapterComplete(request, chapterId);
            }
            request.Remove("Duel", "DuelResult", "Solo.DuelResult");
        }
    }
}
