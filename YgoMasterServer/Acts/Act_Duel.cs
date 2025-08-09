using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YgoMaster.Net;
using YgoMaster.Net.Message;

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
                if (SoloDisableNoShuffle)
                {
                    duelSettings.noshuffle = false;
                }
                duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.AVATAR, player.AvatarId);
                if (duel.IsMyDeck && !duelSettings.IsCustomDuel)
                {
                    DeckInfo deck = duel.GetDeck(GameMode.SoloSingle);
                    if (deck != null)
                    {
                        duelSettings.Deck[DuelSettings.PlayerIndex].CopyFrom(deck);
                        if (deck.Accessory.AvatarId > 0)
                        {
                            duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.AVATAR, deck.Accessory.AvatarId);
                        }
                        duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.AVATAR_HOME, deck.Accessory.AvBase);
                        duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.PROTECTOR, deck.Accessory.Sleeve);
                        duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.FIELD, deck.Accessory.Field);
                        duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.FIELD_OBJ, deck.Accessory.FieldObj);
                        duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.COIN, deck.Accessory.Coin);
                        duelSettings.story_deck_id[DuelSettings.PlayerIndex] = 0;
                        if (player.DuelBgmMode == DuelBgmMode.Myself && !duelSettings.OverrideUserBgm)
                        {
                            duelSettings.SetBgm(player.DuelBgmMode);
                        }
                    }
                }
                duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.ICON, player.IconId);
                duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.ICON_FRAME, player.IconFrameId);
                duelSettings.SetP1ItemValue(duel.IsMyDeck, ItemID.Category.WALLPAPER, player.Wallpaper);
            }
            return duelSettings;
        }

        void Act_DuelBegin(GameServerWebRequest request)
        {
            Dictionary<string, object> rule;
            if (Utils.TryGetValue(request.ActParams, "rule", out rule))
            {
                PlayerDuelState duel = request.Player.Duel;
                duel.Mode = (GameMode)Utils.GetValue<int>(rule, "GameMode");
                duel.ChapterId = Utils.GetValue<int>(rule, "chapter");

                switch (duel.Mode)
                {
                    case GameMode.Room:
                        Act_DuelBeginPvp(request);
                        return;
                    case GameMode.Audience:
                        Act_RoomWatchDuel(request, true);
                        break;
                    case GameMode.Replay:
                        Act_DuelBeginReplay(request);
                        return;
                }

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
                        duelSettings.SetP1Name(duel.IsMyDeck, request.Player.Name);
                    }
                    if (!duelSettings.IsCustomDuel || duelSettings.RandSeed == 0)
                    {
                        duelSettings.RandSeed = (uint)rand.Next();
                    }
                    if (duelSettings.bgms.Count == 0)
                    {
                        duelSettings.SetBgm(request.Player.DuelBgmMode);
                    }
                    
                    duelSettings.pcode[0] = (int)request.Player.Code;
                    duelSettings.follow_num[0] = request.Player.Friends.Count(x => x.Value.HasFlag(FriendState.Following));
                    duelSettings.follower_num[0] = request.Player.Friends.Count(x => x.Value.HasFlag(FriendState.Follower));
                    duelSettings.level[0] = request.Player.Level;
                    duelSettings.rank[0] = request.Player.Rank;
                    duelSettings.rate[0] = request.Player.Rate;

                    request.Player.ActiveDuelSettings.CopyFrom(duelSettings);
                    request.Player.ActiveDuelSettings.HasSavedReplay = false;
                    request.Player.ActiveDuelSettings.DuelBeginTime = Utils.GetEpochTime();
                    request.Response["Duel"] = duelSettings.ToDictionary();
                }
            }
            request.Remove("Duel", "DuelResult", "Result");
        }

        void Act_DuelEnd(GameServerWebRequest request)
        {
            request.Remove("Duel", "User.review", "Solo.Result", "Achievement");

            GameMode gameMode = request.Player.Duel.Mode;
            switch (gameMode)
            {
                case GameMode.Replay:
                    return;
                case GameMode.Audience:
                    ClearSpectatingDuel(request.Player);
                    return;
            }

            DuelResultType res;
            DuelFinishType finish;
            Dictionary<string, object> endParams;
            if (Utils.TryGetValue(request.ActParams, "params", out endParams) &&
                Utils.TryGetValue(endParams, "res", out res) &&
                Utils.TryGetValue(endParams, "finish", out finish))
            {
                request.Player.ActiveDuelSettings.DuelEndTime = Utils.GetEpochTime();
                request.Player.ActiveDuelSettings.res = (int)res;
                request.Player.ActiveDuelSettings.finish = (int)finish;
                request.Player.ActiveDuelSettings.turn = Utils.GetValue<int>(endParams, "turn");
                string replayData = Utils.GetValue<string>(endParams, "replayData");

                if (!string.IsNullOrEmpty(replayData) && !request.Player.ActiveDuelSettings.HasSavedReplay &&
                    DuelReplaySaveForGameModes.Contains(gameMode) && DuelReplaySaveFileLimit > 0 &&
                    gameMode != GameMode.Audience)
                {
                    request.Player.ActiveDuelSettings.replaym = replayData;
                    request.Player.ActiveDuelSettings.HasSavedReplay = true;
                    request.Player.ActiveDuelSettings.open = DuelReplayMakePublicByDefault;
                    string replaysDir = GetReplaysDirectory(request.Player);
                    Utils.TryCreateDirectory(replaysDir);
                    try
                    {
                        bool canSaveReplay = true;
                        string replayPath = Path.Combine(replaysDir, request.Player.ActiveDuelSettings.DuelBeginTime + ".json");
                        if (!File.Exists(replayPath))
                        {
                            List<string> replays = Directory.GetFiles(replaysDir, "*.json", SearchOption.TopDirectoryOnly).ToList();

                            // Auto saved replays are files where their name is number
                            long temp;
                            FileInfo[] autoSavedReplaysByCreationDate = replays
                                .Select(filePath => new FileInfo(filePath))
                                .Where(fileInfo => long.TryParse(Path.GetFileNameWithoutExtension(fileInfo.Name), out temp))
                                .OrderBy(x => x.CreationTimeUtc)
                                .ToArray();

                            // Delete auto saved replays when we reach the save limit for auto saved replays
                            if (autoSavedReplaysByCreationDate.Length >= DuelReplaySaveFileLimit)
                            {
                                canSaveReplay = false;
                                int deletedReplays = 0;
                                foreach (FileInfo replayFileInfo in autoSavedReplaysByCreationDate)
                                {
                                    try
                                    {
                                        try
                                        {
                                            // TODO: Cache this data so that we don't have to keep reopening all replay files when the player reaches their limit
                                            DuelSettings duelSettings = new DuelSettings();
                                            duelSettings.FromDictionary(MiniJSON.Json.Deserialize(File.ReadAllText(replayFileInfo.FullName)) as Dictionary<string, object>);
                                            if (duelSettings.IsReplayLocked)
                                            {
                                                continue;
                                            }
                                        }
                                        catch
                                        {
                                        }

                                        replayFileInfo.Delete();
                                        deletedReplays++;
                                        if (replays.Count - deletedReplays < DuelReplaySaveFileLimit)
                                        {
                                            canSaveReplay = true;
                                            break;
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                        if (canSaveReplay)
                        {
                            File.WriteAllText(replayPath, MiniJSON.Json.Serialize(request.Player.ActiveDuelSettings.ToDictionary()));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to save replay data. Error: " + e);
                    }
                }

                switch (gameMode)
                {
                    case GameMode.SoloSingle:
                        bool chapterStatusChanged = false;
                        if (request.Player.Duel.ChapterId != 0 && res != DuelResultType.None && !request.Player.Duel.IsCustomSoloDuel)
                        {
                            ChapterStatus oldChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out oldChapterStatus);
                            SoloUpdateChapterStatus(request, request.Player.Duel.ChapterId, res, finish);
                            ChapterStatus newChapterStatus;
                            request.Player.SoloChapters.TryGetValue(request.Player.Duel.ChapterId, out newChapterStatus);
                            chapterStatusChanged = oldChapterStatus != newChapterStatus;
                        }
                        GiveDuelReward(request, request.Player, DuelRewards, res, finish, chapterStatusChanged);
                        SavePlayer(request.Player);
                        break;

                    case GameMode.Room:
                        NetClient opponentClient = null;
                        Player opponentPlayer = GetDuelingOpponent(request.Player);
                        if (opponentPlayer != null)
                        {
                            opponentClient = opponentPlayer.NetClient;
                        }
                        DuelRoom duelRoom = request.Player.DuelRoom;
                        if (duelRoom == null)
                        {
                            return;
                        }
                        DuelRoomTable duelRoomTable = duelRoom.GetTable(request.Player);
                        if (duelRoomTable == null)
                        {
                            return;
                        }

                        DuelResultType opponentResult;
                        switch (res)
                        {
                            case DuelResultType.Win: opponentResult = DuelResultType.Lose; break;
                            case DuelResultType.Lose: opponentResult = DuelResultType.Win; break;
                            case DuelResultType.Draw: opponentResult = DuelResultType.Draw; break;
                            default: return;
                        }

                        if (duelRoom.ViewReplays)
                        {
                            lock (duelRoom.Replays)
                            {
                                if (duelRoomTable.Replay == null)
                                {
                                    duelRoomTable.Replay = new DuelRoomReplay();
                                }
                                if (!duelRoomTable.Replay.IsComplete)
                                {
                                    duelRoomTable.Replay.AddReplay(request.Player);
                                    if (duelRoomTable.Replay.IsComplete)
                                    {
                                        duelRoom.AddReplay(duelRoomTable.Replay);
                                    }
                                }
                            }
                        }

                        lock (duelRoomTable.Rewards)
                        {
                            if (duelRoomTable.Rewards.Player1Rewards == null && duelRoomTable.Rewards.Player2Rewards == null)
                            {
                                GiveDuelReward(request, request.Player, DuelRoomRewards, res, DuelFinishType.None, false);
                                duelRoomTable.Rewards.Player1 = request.Player;
                                duelRoomTable.Rewards.Player1Rewards = request.Response;
                                request.Response = new Dictionary<string, object>();

                                GiveDuelReward(request, opponentPlayer, DuelRoomRewards, opponentResult, DuelFinishType.None, false);
                                duelRoomTable.Rewards.Player2 = opponentPlayer;
                                duelRoomTable.Rewards.Player2Rewards = request.Response;
                                request.Response = new Dictionary<string, object>();

                                UpdateUnlockedSecretsForCompletedDuels(request.Player, res, finish);
                                UpdateUnlockedSecretsForCompletedDuels(opponentPlayer, opponentResult, finish);

                                SavePlayerNow(request.Player);
                                SavePlayerNow(opponentPlayer);

                                DuelRoomRecord playerDuelRoomRecords;
                                if (duelRoom.Members.TryGetValue(request.Player, out playerDuelRoomRecords))
                                {
                                    switch (res)
                                    {
                                        case DuelResultType.Win: playerDuelRoomRecords.Win++; break;
                                        case DuelResultType.Lose: playerDuelRoomRecords.Loss++; break;
                                        case DuelResultType.Draw: playerDuelRoomRecords.Draw++; break;
                                    }
                                }

                                DuelRoomRecord opponentDuelRoomRecords;
                                if (duelRoom.Members.TryGetValue(opponentPlayer, out opponentDuelRoomRecords))
                                {
                                    switch (res)
                                    {
                                        case DuelResultType.Win: opponentDuelRoomRecords.Loss++; break;
                                        case DuelResultType.Lose: opponentDuelRoomRecords.Win++; break;
                                        case DuelResultType.Draw: opponentDuelRoomRecords.Draw++; break;
                                    }
                                }
                            }

                            if (request.Player == duelRoomTable.Rewards.Player1)
                            {
                                request.Response = duelRoomTable.Rewards.Player1Rewards;
                            }
                            else if (request.Player == duelRoomTable.Rewards.Player2)
                            {
                                request.Response = duelRoomTable.Rewards.Player2Rewards;
                            }

                            if (res == DuelResultType.Lose && (finish == DuelFinishType.Surrender || finish == DuelFinishType.TimeOut))
                            {
                                lock (duelRoomTable.Spectators)
                                {
                                    byte finn = (byte)finish;

                                    DuelSpectatorDataMessage message = new DuelSpectatorDataMessage();
                                    if (request.Player == duelRoomTable.Player1)
                                    {
                                        message.Buffer = new byte[]
                                        {
                                            0x22, 0x00, finn, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x05, 0x00, 0x02, 0x00, 0x00, 0x00, finn, 0x00
                                        };
                                    }
                                    else
                                    {
                                        message.Buffer = new byte[]
                                        {
                                            0x22, 0x80, finn, 0x00, 0x00, 0x00, 0x00, 0x00,
                                            0x05, 0x00, 0x01, 0x00, finn, 0x00, 0x00, 0x00
                                        };
                                    }

                                    Player p1 = duelRoomTable.Player1;
                                    uint p1Code = p1 == null ? 0 : p1.Code;
                                    if (p1Code != 0)
                                    {
                                        duelRoomTable.SpectatorData.AddRange(message.Buffer);
                                        foreach (Player spectator in new HashSet<Player>(duelRoomTable.Spectators))
                                        {
                                            NetClient spectatorClient = spectator.NetClient;
                                            if (spectatorClient != null && spectator.SpectatingPlayerCode == p1Code)
                                            {
                                                spectatorClient.Send(message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (opponentClient != null)
                        {
                            opponentClient.Send(new OpponentDuelEndedMessage()
                            {
                                Result = res,
                                Finish = finish
                            });

                            NetClient pvpClient = duelRoomTable.PvpClient;
                            if (pvpClient != null)
                            {
                                pvpClient.Send(new OpponentDuelEndedMessage());
                            }
                        }
                        break;
                }
            }
        }

        void GiveDuelReward(GameServerWebRequest request, Player player, DuelRewardInfos rewards, DuelResultType result, DuelFinishType finishType, bool chapterStatusChanged)
        {
            Player temp = request.Player;
            request.Player = player;
            GiveDuelRewardImpl(request, rewards, result, finishType, chapterStatusChanged);
            request.Player = temp;
        }

        void GiveDuelRewardImpl(GameServerWebRequest request, DuelRewardInfos rewards, DuelResultType result, DuelFinishType finishType, bool chapterStatusChanged)
        {
            int turn = 0;
            Dictionary<string, object> endParams;
            if (Utils.TryGetValue(request.ActParams, "params", out endParams))
            {
                turn = Utils.GetValue<int>(endParams, "turn");
            }

            int minimumTurnsForSurrenderRewards = 0;
            switch (result)
            {
                case DuelResultType.Win: minimumTurnsForSurrenderRewards = rewards.MinimumTurnsForSurrenderRewardsWin; break;
                case DuelResultType.Lose: minimumTurnsForSurrenderRewards = rewards.MinimumTurnsForSurrenderRewardsLose; break;
                case DuelResultType.Draw: minimumTurnsForSurrenderRewards = rewards.MinimumTurnsForSurrenderRewardsDraw; break;
            }

            if ((rewards.Win.Count == 0 && rewards.Lose.Count == 0 && rewards.Draw.Count == 0) ||
                (rewards.ChapterStatusChangedNoRewards && chapterStatusChanged) ||
                (rewards.ChapterStatusChangedOnly && !chapterStatusChanged) ||
                (turn >= minimumTurnsForSurrenderRewards && finishType == DuelFinishType.Surrender))
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
            if (request.Player.ActiveDuelSettings != null)
            {
                int playerIndex = request.Player.ActiveDuelSettings.pcode[0] == request.Player.Code || request.Player.ActiveDuelSettings.pcode[0] == 0 ? 0 : 1;
                int avatarId = request.Player.ActiveDuelSettings.avatar[playerIndex];
                if (avatarId > 0)
                {
                    duelResultInfo["avatar"] = avatarId;
                }
            }
            Dictionary<string, object> duelScoreInfo = Utils.GetOrCreateDictionary(duelResultInfo, "scoreInfo");
            Dictionary<string, object> duelScore = Utils.GetOrCreateDictionary(duelScoreInfo, "score");
            int duelScoreTotal = Utils.GetValue<int>(duelScore, "total");
            List<object> duelRewards = Utils.GetOrCreateList(duelScoreInfo, "rewards");

            // 1 = blue box, 2 = gold box, 3 = breaks client (no back button)
            const int blueBox = 1;
            const int goldBox = 2;
            const int duelScoreRewardValue = 1000;

            List<DuelRewardInfo> rewardInfos;
            switch (result)
            {
                case DuelResultType.Win: rewardInfos = rewards.Win; break;
                case DuelResultType.Lose: rewardInfos = rewards.Lose; break;
                case DuelResultType.Draw: rewardInfos = rewards.Draw; break;
                default: return;
            }

            foreach (DuelRewardInfo reward in rewardInfos)
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
                                        if (ItemID.GetCategoryFromID(id) == ItemID.Category.CONSUME || !request.Player.Items.Contains(id))
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
                                        ItemID.Category.COIN
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
                                                reward.CardOwnedLimit > request.Player.Cards.GetCount(card.Key)))
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

        void UpdateUnlockedSecretsForCompletedDuels(Player player, DuelResultType result, DuelFinishType finishType)
        {
            if (finishType == DuelFinishType.Surrender)
            {
                return;
            }
            player.ShopState.DuelsCompletedForNextSecretUnlock++;
            foreach (ShopItemInfo shopItem in Shop.AllShops.Values.OrderBy(x => x.ShopId))
            {
                if (shopItem.UnlockSecrets.Count > 0 && shopItem.UnlockSecretsAtNumDuels > 0 &&
                    player.ShopState.GetAvailability(Shop, shopItem) == PlayerShopItemAvailability.Available &&
                    !shopItem.HasUnlockedAllSecrets(player, Shop))
                {
                    shopItem.DoUnlockSecrets(player, Shop);
                    break;
                }
            }
        }
    }
}
