using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YgoMaster;

namespace YgoMasterClient
{
    static class ClientSettings
    {
        public static string ServerUrl;
        public static string ServerPollUrl;
        public static string MultiplayerToken;
        public static bool ShowConsole;
        public static bool LogIDs;
        public static bool AssetHelperLog;
        public static bool AssetHelperDump;
        public static bool AssetHelperDisableFileErrorPopup;
        public static bool DuelStarterShowFirstPlayer;
        public static int DuelStarterLiveChapterId;
        public static bool DuelStarterLiveNotLiveTest;
        public static bool DeckEditorDisableLimits;
        public static bool DeckEditorConvertStyleRarity;
        public static bool DeckEditorShowStats;
        public static bool DuelClientShowRemainingCardsInDeck;
        public static bool DuelClientMillenniumEye;
        public static bool DuelClientDisableCameraShake;
        public static bool DuelClientDisableCutinAnimations;
        public static bool DuelClientDisableChains;
        public static double DuelClientTimeMultiplier;
        public static bool ReplayControlsAlwaysEnabled;
        public static double ReplayControlsTimeMultiplier;
        public static double TimeMultiplier;
        public static bool DisableVSync;
        public static bool ChangeWindowTitleOnLiveMod;
        public static bool AlwaysWin;
        public static bool CustomDuelCmdLog;

        public static string FilePath
        {
            get { return Path.Combine(Program.ClientDataDir, "ClientSettings.json"); }
        }

        public static bool Load()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }
            Dictionary<string, object> data = null;
            try
            {
                data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(FilePath)) as Dictionary<string, object>;
                if (data == null)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            ServerUrl = YgoMaster.Utils.GetValue<string>(data, "ServerUrl");
            ServerPollUrl = YgoMaster.Utils.GetValue<string>(data, "ServerPollUrl");
            MultiplayerToken = YgoMaster.Utils.GetValue<string>(data, "MultiplayerToken");
            ShowConsole = YgoMaster.Utils.GetValue<bool>(data, "ShowConsole");
            LogIDs = YgoMaster.Utils.GetValue<bool>(data, "LogIDs");
            AssetHelperLog = YgoMaster.Utils.GetValue<bool>(data, "AssetHelperLog");
            AssetHelperDump = YgoMaster.Utils.GetValue<bool>(data, "AssetHelperDump");
            AssetHelperDisableFileErrorPopup = YgoMaster.Utils.GetValue<bool>(data, "AssetHelperDisableFileErrorPopup");
            DeckEditorDisableLimits = YgoMaster.Utils.GetValue<bool>(data, "DeckEditorDisableLimits");
            DeckEditorConvertStyleRarity = YgoMaster.Utils.GetValue<bool>(data, "DeckEditorConvertStyleRarity");
            DeckEditorShowStats = YgoMaster.Utils.GetValue<bool>(data, "DeckEditorShowStats");
            DuelStarterShowFirstPlayer = YgoMaster.Utils.GetValue<bool>(data, "DuelStarterShowFirstPlayer");
            DuelStarterLiveChapterId = YgoMaster.Utils.GetValue<int>(data, "DuelStarterLiveChapterId");
            DuelStarterLiveNotLiveTest = YgoMaster.Utils.GetValue<bool>(data, "DuelStarterLiveNotLiveTest");
            DuelClientShowRemainingCardsInDeck = YgoMaster.Utils.GetValue<bool>(data, "DuelClientShowRemainingCardsInDeck");
            DuelClientMillenniumEye = YgoMaster.Utils.GetValue<bool>(data, "DuelClientMillenniumEye");
            DuelClientDisableCameraShake = YgoMaster.Utils.GetValue<bool>(data, "DuelClientDisableCameraShake");
            DuelClientDisableCutinAnimations = Utils.GetValue<bool>(data, "DuelClientDisableCutinAnimations");
            DuelClientDisableChains = Utils.GetValue<bool>(data, "DuelClientDisableChains");
            DuelClientTimeMultiplier = Utils.GetValue<double>(data, "DuelClientTimeMultiplier");
            ReplayControlsAlwaysEnabled = YgoMaster.Utils.GetValue<bool>(data, "ReplayControlsAlwaysEnabled");
            ReplayControlsTimeMultiplier = Utils.GetValue<double>(data, "ReplayControlsTimeMultiplier");
            TimeMultiplier = Utils.GetValue<double>(data, "TimeMultiplier");
            DisableVSync = YgoMaster.Utils.GetValue<bool>(data, "DisableVSync");
            ChangeWindowTitleOnLiveMod = YgoMaster.Utils.GetValue<bool>(data, "ChangeWindowTitleOnLiveMod");
            AlwaysWin = YgoMaster.Utils.GetValue<bool>(data, "AlwaysWin");
            CustomDuelCmdLog = YgoMaster.Utils.GetValue<bool>(data, "CustomDuelCmdLog");
            return true;
        }

        public static void LoadTimeMultipliers()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(FilePath)) as Dictionary<string, object>;
                    DuelClientTimeMultiplier = Utils.GetValue<double>(data, "DuelClientTimeMultiplier");
                    ReplayControlsTimeMultiplier = Utils.GetValue<double>(data, "ReplayControlsTimeMultiplier");
                    TimeMultiplier = Utils.GetValue<double>(data, "TimeMultiplier");
                }
            }
            catch
            {
            }
        }
    }
}
