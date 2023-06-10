using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YgoMasterClient;
using IL2CPP;
using YgoMaster;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using YgomSystem.Network;
using System.Diagnostics;

namespace YgomGame.SubMenu
{
    unsafe static class HomeSubMenuViewController
    {
        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        static HomeSubMenuViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("HomeSubMenuViewController", "YgomGame.SubMenu");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, classInfo.GetMethod("OnCreatedView"));
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            SubMenuViewController.SetTitleText(thisPtr, "YgoMaster");
            SubMenuViewController.AddMenuItem(thisPtr, "Load replays", OnLoadReplaysFolderAsSelf);
            SubMenuViewController.AddMenuItem(thisPtr, "Load replays (as opponent)", OnLoadReplaysFolderAsOpponent);
            hookOnCreatedView.Original(thisPtr);
        }

        static void OnLoadReplaysFolderAsSelf()
        {
            YgomGame.Menu.ProfileReplayViewController.IsOpponent = false;
            OnLoadReplaysFolder();
        }

        static void OnLoadReplaysFolderAsOpponent()
        {
            YgomGame.Menu.ProfileReplayViewController.IsOpponent = true;
            OnLoadReplaysFolder();
        }

        static void OnLoadReplaysFolder()
        {
            string decksDir = Path.Combine(Program.DataDir, "Replays");
            try
            {
                Directory.CreateDirectory(decksDir);
            }
            catch
            {
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = decksDir;
            if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath) && Directory.Exists(fbd.SelectedPath))
            {
                IntPtr manager = YgomGame.Menu.ContentViewControllerManager.GetManager();
                if (manager != IntPtr.Zero)
                {
                    YgomGame.Menu.ProfileReplayViewController.IsHacked = true;
                    YgomGame.Menu.ProfileReplayViewController.TargetDirectory = new DirectoryInfo(fbd.SelectedPath);
                    Dictionary<string, object> args = new Dictionary<string, object>()
                    {
                        { "pcode", 0 }
                    };
                    YgomSystem.UI.ViewControllerManager.PushChildViewController(manager, "Profile/ProfileReplay", args);
                }
            }
        }
    }
}

namespace YgomGame.Menu
{
    unsafe static class ProfileReplayViewController
    {
        static Dictionary<long, LiveReplayInfo> LastReplayListData = new Dictionary<long, LiveReplayInfo>();
        static long ActiveDuelReplayDid;
        static bool IsMyReplayList;
        static int DuelTurnNum;
        static int DuelFinish;
        static int DuelRes;
        static string ReplayType;
        public static Dictionary<string, object> LiveDuelBeginData;

        public static DirectoryInfo TargetDirectory;
        public static bool IsHacked;
        public static IntPtr Instance;
        public static bool IsOpponent;

        const string NetHijackCmd = "User.profile";
        static string NetCmd;
        static long NetValue;

        static Dictionary<long, ReplayInfo> replays = new Dictionary<long, ReplayInfo>();
        static List<ReplayInfo> replaysList = new List<ReplayInfo>();
        static int targetReplayIndex = -1;

        delegate void Del_OnCreatedView(IntPtr thisPtr);
        static Hook<Del_OnCreatedView> hookOnCreatedView;

        delegate void Del_OnItemSetData(IntPtr thisPtr, int res);
        static Hook<Del_OnItemSetData> hookOnItemSetData_1;
        static Hook<Del_OnItemSetData> hookOnItemSetData_2;
        static Hook<Del_OnItemSetData> hookOnItemSetData_3;
        static Hook<Del_OnItemSetData>[] hooks = new Hook<Del_OnItemSetData>[3];

        static IL2Field fieldDataIndex;

        static ProfileReplayViewController()
        {
            IL2Assembly assembly = Assembler.GetAssembly("Assembly-CSharp");
            IL2Class classInfo = assembly.GetClass("ProfileReplayViewController", "YgomGame.Menu");
            hookOnCreatedView = new Hook<Del_OnCreatedView>(OnCreatedView, classInfo.GetMethod("OnCreatedView"));

            FindOnItemSetDataMethod(classInfo, "<OnItemSetData>b__1", ref hookOnItemSetData_1, 0);
            FindOnItemSetDataMethod(classInfo, "<OnItemSetData>b__2", ref hookOnItemSetData_2, 1);
            FindOnItemSetDataMethod(classInfo, "<OnItemSetData>b__3", ref hookOnItemSetData_3, 2);
        }

        static void FindOnItemSetDataMethod(IL2Class classInfo, string name, ref Hook<Del_OnItemSetData> hook, int i)
        {
            foreach (IL2Class type in classInfo.GetNestedTypes())
            {
                IL2Method method = type.GetMethods().FirstOrDefault(x => x.Name == name);
                if (method != null)
                {
                    fieldDataIndex = type.GetField("dataindex");
                    hook = new Hook<Del_OnItemSetData>((IntPtr thisPtr, int res) =>
                    {
                        targetReplayIndex = fieldDataIndex.GetValue(thisPtr).GetValueRef<int>();
                        hooks[i].Original(thisPtr, res);
                    }, method);
                    hooks[i] = hook;
                    return;
                }
            }
            Utils.LogWarning("Failed to find " + name + " function for ProfileReplayViewController");
        }

        static void OnCreatedView(IntPtr thisPtr)
        {
            Instance = thisPtr;
            hookOnCreatedView.Original(thisPtr);
        }

        public static void OnPopChildViewController(IntPtr manager, IntPtr popTarget)
        {
            if (Instance != IntPtr.Zero && popTarget == Instance)
            {
                TargetDirectory = null;
                IsHacked = false;
                Instance = IntPtr.Zero;
                IsOpponent = false;
                NetCmd = null;
                NetValue = 0;
                replays.Clear();
                replaysList.Clear();
                LastReplayListData.Clear();
                targetReplayIndex = -1;
            }
        }

        public static void OnNetworkComplete(IntPtr requestStructurePtr, string cmd)
        {
            if (cmd == "User.replay_list")
            {
                ReplayType = null;
                LastReplayListData.Clear();
                try
                {
                    string jsonPath = IsMyReplayList ? "User.replay" : "Friend.replay";
                    Dictionary<string, object> allReplays = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath(jsonPath)) as Dictionary<string, object>;
                    if (allReplays != null)
                    {
                        foreach (KeyValuePair<string, object> replayData in allReplays)
                        {
                            long did;
                            Dictionary<string, object> data = replayData.Value as Dictionary<string, object>;
                            if (long.TryParse(replayData.Key, out did) && data != null)
                            {
                                LiveReplayInfo replayInfo = new LiveReplayInfo();
                                replayInfo.Did = did;
                                replayInfo.Time = Utils.GetValue<long>(data, "time");
                                replayInfo.Data = data;
                                LastReplayListData[did] = replayInfo;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Error handling " + cmd + ". Error: " + e);
                }
            }
            else if (!string.IsNullOrEmpty(cmd) && cmd.EndsWith("duel_history") && !cmd.StartsWith("PvP"))
            {
                LastReplayListData.Clear();
                try
                {
                    int dotIndex = cmd.IndexOf(".");
                    if (dotIndex > 0)
                    {
                        ReplayType = cmd.Substring(0, dotIndex);
                    }
                    else
                    {
                        ReplayType = null;
                    }
                    Dictionary<string, object> duelHistory = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("DuelHistory")) as Dictionary<string, object>;
                    if (duelHistory != null)
                    {
                        foreach (KeyValuePair<string, object> entry in duelHistory)
                        {
                            Dictionary<string, object> dict = entry.Value as Dictionary<string, object>;
                            if (dict != null)
                            {
                                foreach (KeyValuePair<string, object> subEntry in dict)
                                {
                                    Dictionary<string, object> data = subEntry.Value as Dictionary<string, object>;
                                    long did;
                                    if (data != null && data.ContainsKey(DuelSettings.ExpectedDuelDataKey) &&
                                        Utils.TryGetValue(data, "did", out did))
                                    {
                                        LiveReplayInfo replayInfo = new LiveReplayInfo();
                                        replayInfo.Did = did;
                                        replayInfo.Time = Utils.GetValue<long>(data, "time");
                                        replayInfo.Data = data;
                                        LastReplayListData[did] = replayInfo;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogWarning("Error handling " + cmd + ". Error: " + e);
                }
            }

            if (Program.IsLive && cmd == "Duel.end" && LiveDuelBeginData != null && LiveDuelBeginData.ContainsKey("GameMode"))
            {
                GameMode gameMode = Utils.GetValue<GameMode>(LiveDuelBeginData, "GameMode");
                if (ClientSettings.DuelReplayLiveAutoSave && (gameMode == GameMode.Replay || gameMode == GameMode.SoloSingle))
                {
                    long did;
                    bool hasDid = Utils.TryGetValue(LiveDuelBeginData, "did", out did);

                    LiveReplayInfo replayInfo = null;
                    if (gameMode == GameMode.Replay)
                    {
                        if (LastReplayListData.TryGetValue(ActiveDuelReplayDid, out replayInfo))
                        {
                            did = replayInfo.Did;
                        }
                        else
                        {
                            hasDid = false;
                        }
                    }

                    string dir = Path.Combine(Program.DataDir, "Replays");
                    if (ClientSettings.DuelReplayLiveAutoSaveUseSubFolders)
                    {
                        if (gameMode == GameMode.Replay)
                        {
                            dir = Path.Combine(dir, gameMode.ToString() + (string.IsNullOrEmpty(ReplayType) ? string.Empty : " (" + ReplayType + ")"));
                        }
                        else
                        {
                            dir = Path.Combine(dir, gameMode.ToString());
                        }
                    }
                    Utils.TryCreateDirectory(dir);

                    Dictionary<string, object> duelEndData = MiniJSON.Json.Deserialize(YgomSystem.Utility.ClientWork.SerializePath("Duel")) as Dictionary<string, object>;

                    Dictionary<string, object> endParams;
                    if (Utils.TryGetValue(duelEndData, "params", out endParams))
                    {
                        if (!LiveDuelBeginData.ContainsKey("MaxTurn") && endParams.ContainsKey("MaxTurn"))
                        {
                            LiveDuelBeginData["MaxTurn"] = endParams["MaxTurn"];
                        }

                        if (endParams.ContainsKey("finish")) LiveDuelBeginData["finish"] = endParams["finish"];
                        if (endParams.ContainsKey("res")) LiveDuelBeginData["res"] = endParams["res"];
                    }

                    if (replayInfo != null && replayInfo.Data != null)
                    {
                        if (!LiveDuelBeginData.ContainsKey("res")) LiveDuelBeginData["res"] = Utils.GetValue<int>(replayInfo.Data, "res");
                        if (!LiveDuelBeginData.ContainsKey("finish")) LiveDuelBeginData["finish"] = Utils.GetValue<int>(replayInfo.Data, "finish");
                        if (!LiveDuelBeginData.ContainsKey("turn")) LiveDuelBeginData["turn"] = Utils.GetValue<int>(replayInfo.Data, "turn");
                    }

                    switch (gameMode)
                    {
                        case GameMode.Audience:
                        case GameMode.Replay:
                            break;
                        default:
                            if (DuelTurnNum > 0 && Utils.GetValue<int>(LiveDuelBeginData, "turn") == 0 && Utils.GetValue<int>(LiveDuelBeginData, "MaxTurn") == 0)
                            {
                                LiveDuelBeginData["turn"] = LiveDuelBeginData["MaxTurn"] = DuelTurnNum;
                            }
                            if (!LiveDuelBeginData.ContainsKey("finish")) LiveDuelBeginData["finish"] = DuelFinish;
                            if (!LiveDuelBeginData.ContainsKey("res")) LiveDuelBeginData["res"] = DuelRes;
                            break;
                    }

                    if (gameMode == GameMode.SoloSingle && DuelDll.ReplayData.Count > 0)
                    {
                        LiveDuelBeginData["replaym"] = YgomSystem.Network.API.GetReplayDataString(LiveDuelBeginData);
                    }

                    try
                    {
                        if (hasDid)
                        {
                            string file = Path.Combine(dir, did + ".json");
                            File.WriteAllText(file, MiniJSON.Json.Serialize(LiveDuelBeginData));
                        }
                    }
                    catch
                    {
                    }
                }
                LiveDuelBeginData = null;
                ActiveDuelReplayDid = 0;
            }

            if (!IsHacked || string.IsNullOrEmpty(NetCmd) || cmd != NetHijackCmd)
            {
                return;
            }

            switch (NetCmd)
            {
                case "User.profile":
                    {
                        //YgomSystem.Utility.ClientWork.DeleteByJsonPath("User.profile");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Friend.profile");

                        if (targetReplayIndex >= 0 && targetReplayIndex < replaysList.Count)
                        {
                            ReplayInfo replay = replaysList[targetReplayIndex];

                            DuelSettings duelSettings = new DuelSettings();
                            try
                            {
                                if (File.Exists(replay.Path))
                                {
                                    Dictionary<string, object> duelData = MiniJSON.Json.Deserialize(File.ReadAllText(replay.Path)) as Dictionary<string, object>;
                                    duelSettings.FromDictionary(duelData);
                                }
                            }
                            catch
                            {
                                return;
                            }

                            Dictionary<string, object> rootData = new Dictionary<string, object>();
                            Dictionary<string, object> friendData = Utils.GetOrCreateDictionary(rootData, "Friend");
                            Dictionary<string, object> profileData = Utils.GetOrCreateDictionary(friendData, "profile");

                            profileData["name"] = duelSettings.name[replay.OpponentID];
                            profileData["rank"] = duelSettings.rank[replay.OpponentID];
                            profileData["rate"] = duelSettings.rate[replay.OpponentID];
                            profileData["level"] = duelSettings.level[replay.OpponentID];
                            profileData["exp"] = 0;
                            profileData["need_exp"] = 0;
                            profileData["icon_id"] = duelSettings.icon[replay.OpponentID];
                            profileData["icon_frame_id"] = duelSettings.icon_frame[replay.OpponentID];
                            profileData["avatar_id"] = duelSettings.avatar[replay.OpponentID];
                            profileData["wallpaper"] = duelSettings.wallpaper[replay.OpponentID];
                            profileData["tag"] = duelSettings.profile_tag[replay.OpponentID].ToArray();
                            profileData["follow_num"] = duelSettings.follow_num[replay.OpponentID];
                            profileData["follower_num"] = duelSettings.follower_num[replay.OpponentID];

                            profileData["is_follow"] = false;
                            profileData["is_block"] = false;
                            profileData["is_ps_block"] = false;
                            profileData["is_xbox_block"] = false;

                            YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(rootData));
                        }
                    }
                    break;
                case "User.replay_list":
                    {
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("User.replay");

                        replays.Clear();
                        if (TargetDirectory.Exists)
                        {
                            Dictionary<string, object> rootData = new Dictionary<string, object>();
                            Dictionary<string, object> userData = Utils.GetOrCreateDictionary(rootData, "User");
                            Dictionary<string, object> allReplayData = Utils.GetOrCreateDictionary(userData, "replay");

                            foreach (FileInfo fileInfo in TargetDirectory.GetFiles("*.json"))
                            {
                                try
                                {
                                    Dictionary<string, object> duelData = MiniJSON.Json.Deserialize(File.ReadAllText(fileInfo.FullName)) as Dictionary<string, object>;
                                    if (duelData != null)
                                    {
                                        DuelSettings duelSettings = new DuelSettings();
                                        duelSettings.FromDictionary(duelData);
                                        while (replays.ContainsKey(duelSettings.did))
                                        {
                                            duelSettings.did++;
                                        }
                                        FixupDuelSettingsForIsOpponent(duelSettings, true);
                                        ReplayInfo replayInfo = new ReplayInfo()
                                        {
                                            Did = duelSettings.did,
                                            MyID = duelSettings.MyID,
                                            Path = fileInfo.FullName,
                                            Pcode = duelSettings.pcode
                                        };
                                        replays[duelSettings.did] = replayInfo;
                                        replaysList.Add(replayInfo);
                                        Dictionary<string, object> replayData = duelSettings.ToDictionaryForReplayList();
                                        allReplayData[duelSettings.did.ToString()] = replayData;
                                        replayInfo.Time = Utils.GetValue<long>(replayData, "time");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Utils.LogWarning("Load replay file '" + fileInfo.Name + "' failed. Error: " + e);
                                }
                            }

                            // NOTE: Two duels with the same time might result in our list being off from the games list
                            // (this would impact viewing those specific profiles but not viewing their duels)
                            replaysList.Sort((x, y) => -x.Time.CompareTo(y.Time));

                            YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(rootData));
                        }
                    }
                    break;
                case "PvP.get_replay_deck":
                    {
                        ReplayInfo replay;
                        if (replays.TryGetValue(NetValue, out replay))
                        {
                            try
                            {
                                if (File.Exists(replay.Path))
                                {
                                    Dictionary<string, object> duelData = MiniJSON.Json.Deserialize(File.ReadAllText(replay.Path)) as Dictionary<string, object>;
                                    if (duelData != null)
                                    {
                                        DuelSettings duelSettings = new DuelSettings();
                                        duelSettings.FromDictionary(duelData);
                                        DeckInfo deckInfo = duelSettings.Deck[replay.OpponentID];
                                        if (deckInfo != null)
                                        {
                                            Dictionary<string, object> rootData = new Dictionary<string, object>();
                                            Dictionary<string, object> allDeckListData = Utils.GetOrCreateDictionary(rootData, "RDeckList");
                                            allDeckListData[replay.Did.ToString()] = deckInfo.ToDictionary();
                                            YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(rootData));
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Utils.LogWarning("Failed to get opponents deck for replay " + NetValue + "'. Error: " + e);
                            }
                        }
                        break;
                    }
                case "PvP.set_replay_open":
                    {
                        // Making replays public / private here doesn't make much sense
                    }
                    break;
                case "PvP.remove_replay":
                    {
                        ReplayInfo replay;
                        if (replays.TryGetValue(NetValue, out replay))
                        {
                            try
                            {
                                if (File.Exists(replay.Path))
                                {
                                    File.Delete(replay.Path);
                                    replays.Remove(replay.Did);
                                    replaysList.Remove(replay);
                                    YgomSystem.Utility.ClientWork.DeleteByJsonPath("User.replay." + NetValue);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                    break;
                case "PvP.replay_duel":
                    {
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Response");
                        ReplayInfo replay;
                        if (replays.TryGetValue(NetValue, out replay))
                        {
                            Dictionary<string, object> rootData = new Dictionary<string, object>();
                            Dictionary<string, object> responseData = Utils.GetOrCreateDictionary(rootData, "Response");
                            responseData["UrlScheme"] = "duel:push?GameMode=" + (int)GameMode.Replay + "&did=" + NetValue;
                            YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(rootData));
                        }
                    }
                    break;
                case "Duel.begin":
                    {
                        bool failed = true;

                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("DuelResult");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Result");

                        ReplayInfo replay;
                        if (replays.TryGetValue(NetValue, out replay))
                        {
                            try
                            {
                                if (File.Exists(replay.Path))
                                {
                                    Dictionary<string, object> duelData = MiniJSON.Json.Deserialize(File.ReadAllText(replay.Path)) as Dictionary<string, object>;
                                    if (duelData != null)
                                    {
                                        Dictionary<string, object> rootData = new Dictionary<string, object>();

                                        DuelSettings duelSettings = new DuelSettings();
                                        duelSettings.FromDictionary(duelData);

                                        duelSettings.MyType = (int)DuelPlayerType.Replay;
                                        duelSettings.chapter = 0;
                                        duelSettings.GameMode = (int)GameMode.Replay;
                                        FixupDuelSettingsForIsOpponent(duelSettings);

                                        Dictionary<string, object> dict = DuelSettings.FixupReplayRequirements(duelSettings, duelSettings.ToDictionary());
                                        dict["publicLevel"] = (int)ClientSettings.DuelReplayCardVisibility;
                                        rootData["Duel"] = dict;

                                        YgomSystem.Utility.ClientWork.UpdateJson(MiniJSON.Json.Serialize(rootData));

                                        failed = false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Utils.LogWarning("Failed to get opponents deck for replay " + NetValue + "'. Error: " + e);
                            }
                        }

                        if (failed)
                        {
                            // To avoid getting get stuck "loading" forever (NOTE: No error message appears, it just does nothing)
                            YgomSystem.Network.RequestStructure.SetCode(requestStructurePtr, (int)ResultCodes.DuelCode.ERROR);
                        }
                    }
                    break;
                case "Duel.end":
                    {
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Duel");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("User.review");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Solo.Result");
                        YgomSystem.Utility.ClientWork.DeleteByJsonPath("Achievement");
                    }
                    break;
            }
        }

        static void FixupDuelSettingsForIsOpponent(DuelSettings duelSettings, bool fixOpponentPcode = false)
        {
            int opponentId = duelSettings.MyID == 0 ? 1 : 0;
            if (IsOpponent)
            {
                duelSettings.MyID = opponentId;
                opponentId = 1 - opponentId;
                switch ((DuelResultType)duelSettings.res)
                {
                    case DuelResultType.Lose:
                        duelSettings.res = (int)DuelResultType.Win;
                        break;
                    case DuelResultType.Win:
                        duelSettings.res = (int)DuelResultType.Lose;
                        break;
                }
            }
            if (duelSettings.pcode[opponentId] == 0 && fixOpponentPcode)
            {
                // Need a pcode otherwise it'll think it's the users profile
                duelSettings.pcode[opponentId] = 1;
            }
        }

        public static void OnNetworkEntry(ref IntPtr commandPtr, ref IntPtr paramPtr, ref float timeOut)
        {
            string commandString = new IL2String(commandPtr).ToString();
            if (!string.IsNullOrEmpty(commandString))
            {
                Dictionary<string, object> param = MiniJSON.Json.Deserialize(YgomMiniJSON.Json.Serialize(paramPtr)) as Dictionary<string, object>;

                switch (commandString)
                {
                    case "User.replay_list":
                        IsMyReplayList = Utils.GetValue<long>(param, "pcode") == 0;
                        break;
                    case "Duel.begin":
                        DuelTurnNum = 0;
                        DuelFinish = 0;
                        DuelRes = 0;
                        break;
                    case "Duel.end":
                        DuelTurnNum = YgomGame.Duel.Engine.GetTurnNum() + 1;
                        Dictionary<string, object> endParams;
                        if (Utils.TryGetValue(param, "params", out endParams))
                        {
                            DuelFinish = Utils.GetValue<int>(endParams, "finish");
                            DuelRes = Utils.GetValue<int>(endParams, "res");
                        }
                        break;
                    case "PvP.replay_duel":
                    case "PvP.replay_duel_history":
                        ActiveDuelReplayDid = Utils.GetValue<long>(param, "did");
                        break;
                }

                if (!IsHacked)
                {
                    return;
                }

                NetCmd = commandString;
                switch (commandString)
                {
                    case "User.profile":
                        NetValue = Utils.GetValue<long>(param, "pcode");
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "User.replay_list":
                        NetValue = 0;
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "PvP.get_replay_deck":
                        NetValue = Utils.GetValue<long>(param, "did");
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "PvP.set_replay_open":
                        NetValue = Utils.GetValue<long>(param, "did");
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "PvP.remove_replay":
                        NetValue = Utils.GetValue<long>(param, "did");
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "PvP.replay_duel":
                        //Utils.GetValue<long>(param, "pcode");
                        NetValue = Utils.GetValue<long>(param, "did");
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "Duel.begin":
                        Dictionary<string, object> rule;
                        if (Utils.TryGetValue(param, "rule", out rule))
                        {
                            NetValue = Utils.GetValue<long>(rule, "did");
                        }
                        else
                        {
                            NetValue = 0;
                        }
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    case "Duel.end":
                        ReplaceNetworkEntry(ref commandPtr, ref paramPtr);
                        break;
                    default:
                        NetCmd = null;
                        break;
                }
            }
        }

        static void ReplaceNetworkEntry(ref IntPtr commandPtr, ref IntPtr paramPtr)
        {
            Dictionary<string, object> param = new Dictionary<string, object>()
            {
                { "pcode", 0 }
            };
            paramPtr = YgomMiniJSON.Json.Deserialize(MiniJSON.Json.Serialize(param));
            commandPtr = new IL2String(NetHijackCmd).ptr;
        }

        class LiveReplayInfo
        {
            public long Did;
            public long Time;
            public Dictionary<string, object> Data;
        }

        class ReplayInfo
        {
            public long Did;
            public int MyID;
            public int[] Pcode;
            public string Path;
            public long Time;

            public uint PlayerCode
            {
                get { return (uint)(Pcode != null && Pcode.Length >= 2 ? Pcode[MyID] : 0); }
            }
            public uint OpponentPlayerCode
            {
                get { return (uint)(Pcode != null && Pcode.Length >= 2 ? Pcode[OpponentID] : 0); }
            }
            public int OpponentID
            {
                get { return MyID == 0 ? 1 : 0; }
            }

            public DuelSettings Load()
            {
                try
                {
                    if (!string.IsNullOrEmpty(Path) && File.Exists(Path))
                    {
                        DuelSettings duelSettings = new DuelSettings();
                        duelSettings.FromDictionary(MiniJSON.Json.Deserialize(File.ReadAllText(Path)) as Dictionary<string, object>);
                    }
                }
                catch
                {
                }
                return null;
            }
        }
    }
}