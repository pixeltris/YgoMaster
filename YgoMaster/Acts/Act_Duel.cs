using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_DuelBegin(GameServerWebRequest request)
        {
            Dictionary<string, object> rule;
            if (TryGetValue(request.ActParams, "rule", out rule))
            {
                PlayerActiveDuel duel = request.Player.ActiveDuel;
                duel.Mode = (GameMode)GetValue<int>(rule, "GameMode");
                duel.ChapterId = GetValue<int>(rule, "chapter");
                DuelSettings duelSettings = null;
                switch (duel.Mode)
                {
                    case GameMode.SoloSingle:
                        {
                            DuelSettings ds;
                            if (SoloDuels.TryGetValue(duel.ChapterId, out ds))
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
                            }
                        }
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
                switch (request.Player.ActiveDuel.Mode)
                {
                    case GameMode.SoloSingle:
                        if (/*res == (int)DuelResultType.Win && */request.Player.ActiveDuel.ChapterId != 0)
                        {
                            OnSoloChapterComplete(request, request.Player.ActiveDuel.ChapterId);
                        }
                        break;
                }
            }
        }
    }
}
