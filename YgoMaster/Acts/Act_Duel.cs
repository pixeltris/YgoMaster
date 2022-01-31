using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        DuelSettings CreateSoloDuelSettings(Player player, int chapterId)
        {
            DuelSettings duelSettings = null;
            PlayerDuelState duel = player.Duel;
            DuelSettings ds;
            if (SoloDuels.TryGetValue(chapterId, out ds))
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
                if (duel.IsMyDeck)
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
                        duelSettings = CreateSoloDuelSettings(request.Player, duel.ChapterId);
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
                    duelSettings.name[DuelSettings.PlayerIndex] = request.Player.Name;
                    duelSettings.RandSeed = (uint)rand.Next();
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
                        if (res == (int)DuelResultType.Win && request.Player.Duel.ChapterId != 0)
                        {
                            OnSoloChapterComplete(request, request.Player.Duel.ChapterId);
                        }
                        break;
                }
            }
        }
    }
}
