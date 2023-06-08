using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        const string expectedDuelDataKey = "DuelBeginTime";

        void Act_ReplayList(GameServerWebRequest request)
        {
            request.Player.RecentlyListedReplayFilesByDid.Clear();

            uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
            if (pcode == 0)
            {
                pcode = request.Player.Code;
            }

            Player player;
            lock (playersLock)
            {
                if (!playersById.TryGetValue(pcode, out player))
                {
                    return;
                }
            }

            request.Remove("User.replay");

            Dictionary<string, object> userData = request.GetOrCreateDictionary("User");
            Dictionary<string, object> allReplayData = Utils.GetOrCreateDictionary(userData, "replay");

            string replaysDir = GetReplaysDirectory(player);
            if (!Directory.Exists(replaysDir))
            {
                return;
            }

            foreach (string file in Directory.GetFiles(replaysDir, "*.json"))
            {
                try
                {
                    Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(file)) as Dictionary<string, object>;
                    if (data != null && data.ContainsKey(expectedDuelDataKey))
                    {
                        DuelSettings duelSettings = new DuelSettings();
                        duelSettings.FromDictionary(data);

                        if (pcode != 0 && pcode != request.Player.Code && !duelSettings.open)
                        {
                            continue;
                        }

                        Player[] players = new Player[2];
                        lock (playersLock)
                        {
                            for (int i = 0; i < players.Length; i++)
                            {
                                playersById.TryGetValue((uint)duelSettings.pcode[i], out players[i]);
                            }
                        }

                        long did = duelSettings.DuelBeginTime;
                        while (request.Player.RecentlyListedReplayFilesByDid.ContainsKey(did))
                        {
                            did++;
                        }
                        duelSettings.did = did;
                        request.Player.RecentlyListedReplayFilesByDid[did] = file;
                        allReplayData[did.ToString()] = duelSettings.ToDictionaryForReplayList(players);
                    }
                }
                catch
                {
                }
            }
        }

        void Act_ReplaySetOpen(GameServerWebRequest request)
        {
            long did = Utils.GetValue<long>(request.ActParams, "did");
            bool open = Utils.GetValue<bool>(request.ActParams, "open");
            string replayPath;
            if (request.Player.RecentlyListedReplayFilesByDid.TryGetValue(did, out replayPath))
            {
                try
                {
                    if (File.Exists(replayPath))
                    {
                        Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(replayPath)) as Dictionary<string, object>;
                        if (data != null && data.ContainsKey(expectedDuelDataKey))
                        {
                            DuelSettings duelSettings = new DuelSettings();
                            duelSettings.FromDictionary(data);
                            if (duelSettings.open != open)
                            {
                                duelSettings.open = open;
                                File.WriteAllText(replayPath, MiniJSON.Json.Serialize(duelSettings.ToDictionary()));
                            }
                            Dictionary<string, object> userData = request.GetOrCreateDictionary("User");
                            Dictionary<string, object> allReplayData = Utils.GetOrCreateDictionary(userData, "replay");
                            Dictionary<string, object> replayData = Utils.GetOrCreateDictionary(allReplayData, did.ToString());
                            replayData["open"] = open;
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Failed to update replay public status for deck " + did + " '" + replayPath + "'. Error: " + e);
                }
            }
        }

        void Act_ReplayGetDeck(GameServerWebRequest request)
        {
            long did = Utils.GetValue<long>(request.ActParams, "did");
            string replayPath;
            if (request.Player.RecentlyListedReplayFilesByDid.TryGetValue(did, out replayPath))
            {
                try
                {
                    if (File.Exists(replayPath))
                    {
                        Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(replayPath)) as Dictionary<string, object>;
                        if (data != null && data.ContainsKey(expectedDuelDataKey))
                        {
                            DuelSettings duelSettings = new DuelSettings();
                            duelSettings.FromDictionary(data);

                            DeckInfo deckInfo = duelSettings.Deck[duelSettings.MyID == 0 ? 0 : 1];
                            if (deckInfo != null)
                            {
                                Dictionary<string, object> allDeckListData = request.GetOrCreateDictionary("RDeckList");
                                allDeckListData[did.ToString()] = deckInfo.ToDictionary();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Failed to get opponents deck for replay " + did + " '" + replayPath + "'. Error: " + e);
                }
            }
        }

        void Act_ReplayRemove(GameServerWebRequest request)
        {
            long did = Utils.GetValue<long>(request.ActParams, "did");
            string replayPath;
            if (request.Player.RecentlyListedReplayFilesByDid.TryGetValue(did, out replayPath))
            {
                try
                {
                    if (File.Exists(replayPath))
                    {
                        File.Delete(replayPath);
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Failed delete replay " + did + " '" + replayPath + "'. Error: " + e);
                }
            }
            request.Remove("User.replay." + did);
        }

        void Act_ReplayDuel(GameServerWebRequest request)
        {
            // TODO: Handle pcode (we currently assume the client can only request based on the previous file list)

            uint pcode = Utils.GetValue<uint>(request.ActParams, "pcode");
            long did = Utils.GetValue<long>(request.ActParams, "did");

            request.Remove("Response");
            bool loaded = false;

            string replayPath;
            if (request.Player.RecentlyListedReplayFilesByDid.TryGetValue(did, out replayPath))
            {
                try
                {
                    if (File.Exists(replayPath))
                    {
                        Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(replayPath)) as Dictionary<string, object>;
                        if (data != null && data.ContainsKey(expectedDuelDataKey))
                        {
                            //duel:push?GameMode=7&did=___DID___&pl_info={"___PCODE1___":{"is_same_os":false,"online_id":""},"___PCODE2___":{"is_same_os":false,"online_id":""}}&r_myid=1&is_db=1
                            Dictionary<string, object> responseData = request.GetOrCreateDictionary("Response");
                            responseData["UrlScheme"] = "duel:push?GameMode=" + (int)GameMode.Replay + "&did=" + did;

                            loaded = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Failed play replay " + did + " '" + replayPath + "'. Error: " + e);
                }
            }

            if (!loaded)
            {
                request.ResultCode = (int)ResultCodes.PvPCode.NO_REPLAY_DATA;
            }
        }

        void Act_DuelBeginReplay(GameServerWebRequest request)
        {
            Dictionary<string, object> rule = Utils.GetDictionary(request.ActParams, "rule");
            long did = Utils.GetValue<long>(rule, "did");

            request.Remove("Duel", "DuelResult", "Result");
            bool loaded = false;

            string replayPath;
            if (request.Player.RecentlyListedReplayFilesByDid.TryGetValue(did, out replayPath))
            {
                try
                {
                    if (File.Exists(replayPath))
                    {
                        Dictionary<string, object> data = MiniJSON.Json.Deserialize(File.ReadAllText(replayPath)) as Dictionary<string, object>;
                        if (data != null && data.ContainsKey(expectedDuelDataKey))
                        {
                            DuelSettings duelSettings = new DuelSettings();
                            duelSettings.FromDictionary(data);

                            duelSettings.MyType = (int)DuelPlayerType.Replay;
                            duelSettings.chapter = 0;
                            duelSettings.GameMode = (int)GameMode.Replay;
                            request.Response["Duel"] = duelSettings.ToDictionary();
                            
                            // Fix up some things which are required by replays (duel hangs at end without these)
                            var dict = (request.Response["Duel"] as Dictionary<string, object>);
                            dict["rank"] = new List<object>()
                            {
                                new Dictionary<string, object>()
                                {
                                    { "rank", duelSettings.rank[0] },
                                    { "rate", duelSettings.rate[0] }
                                },
                                new Dictionary<string, object>()
                                {
                                    { "rank", duelSettings.rank[1] },
                                    { "rate", duelSettings.rate[1] }
                                }
                            };
                            dict["xuid"] = new int[2];
                            dict["online_id"] = new int[2];
                            dict["is_same_os"] = new bool[2];

                            dict["MaxTurn"] = duelSettings.turn;
                            dict["publicLevel"] = (int)DuelReplayCardVisibility.AllOpen;
                            //dict["hand_open"] = true;
                            //dict["blockRelativeCard"] = false;

                            loaded = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Failed play replay " + did + " '" + replayPath + "'. Error: " + e);
                }
            }

            if (!loaded)
            {
                request.ResultCode = (int)ResultCodes.PvPCode.NO_REPLAY_DATA;
            }
        }
    }
}
