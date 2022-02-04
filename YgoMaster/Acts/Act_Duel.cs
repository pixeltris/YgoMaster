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
                        if (request.Player.Duel.ChapterId != 0 &&
                            (res == (int)DuelResultType.Win || GetSkippableChapterIds().Contains(request.Player.Duel.ChapterId)))
                        {
                            OnSoloChapterComplete(request, request.Player.Duel.ChapterId);
                        }
                        break;
                }
            }
        }
    }
}
