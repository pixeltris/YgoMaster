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
        public static string SessionServerIP;
        public static int SessionServerPort;
        public static string ServerUrl;
        public static string ServerPollUrl;
        public static string MultiplayerToken;
        public static int MultiplayerPingTimeoutInSeconds;
        public static int MultiplayerConnectDelayInSeconds;
        public static bool MultiplayerNoDelay;
        public static bool MultiplayerLogConnectionState;
        public static int DuelDllActiveUserDoCommandOffset;
        public static int DuelDllActiveUserSetIndexOffset;
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
        public static bool DuelClientDisableCutinAnimationsForCardsWithCustomImages;
        public static bool DuelClientDisableChains;
        public static double DuelClientTimeMultiplier;
        public static bool ReplayControlsAlwaysEnabled;
        public static double ReplayControlsTimeMultiplier;
        public static double TimeMultiplier;
        public static bool DisableVSync;
        public static bool ChangeWindowTitleOnLiveMod;
        public static bool AlwaysWin;
        public static bool CustomDuelCmdLog;
        public static bool RandomDecksNonRecursive;
        public static bool RandomDecksDontSetCpuName;

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

            string baseIP = Utils.GetValue<string>(data, "BaseIP");
            int basePort = Utils.GetValue<int>(data, "BasePort");
            SessionServerPort = Utils.GetValue<int>(data, "SessionServerPort");
            Func<string, string> FixupUrl = (string str) =>
            {
                str = str.Replace("{BaseIP}", baseIP);
                str = str.Replace("{BasePort}", basePort.ToString());
                str = str.Replace("{SessionServerPort}", SessionServerPort.ToString());
                if (str == "localhost")
                {
                    str = "127.0.0.1";
                }
                return str;
            };

            SessionServerIP = FixupUrl(Utils.GetValue<string>(data, "SessionServerIP"));
            ServerUrl = FixupUrl(Utils.GetValue<string>(data, "ServerUrl"));
            ServerPollUrl = FixupUrl(Utils.GetValue<string>(data, "ServerPollUrl"));
            MultiplayerToken = Utils.GetValue<string>(data, "MultiplayerToken");
            MultiplayerPingTimeoutInSeconds = Utils.GetValue<int>(data, "MultiplayerPingTimeoutInSeconds");
            MultiplayerConnectDelayInSeconds = Utils.GetValue<int>(data, "MultiplayerConnectDelayInSeconds");
            MultiplayerNoDelay = Utils.GetValue<bool>(data, "MultiplayerNoDelay");
            MultiplayerLogConnectionState = Utils.GetValue<bool>(data, "MultiplayerLogConnectionState");
            DuelDllActiveUserDoCommandOffset = Utils.GetValue<int>(data, "DuelDllActiveUserDoCommandOffset");
            DuelDllActiveUserSetIndexOffset = Utils.GetValue<int>(data, "DuelDllActiveUserSetIndexOffset");
            ShowConsole = Utils.GetValue<bool>(data, "ShowConsole");
            LogIDs = Utils.GetValue<bool>(data, "LogIDs");
            AssetHelperLog = Utils.GetValue<bool>(data, "AssetHelperLog");
            AssetHelperDump = Utils.GetValue<bool>(data, "AssetHelperDump");
            AssetHelperDisableFileErrorPopup = Utils.GetValue<bool>(data, "AssetHelperDisableFileErrorPopup");
            DeckEditorDisableLimits = Utils.GetValue<bool>(data, "DeckEditorDisableLimits");
            DeckEditorConvertStyleRarity = Utils.GetValue<bool>(data, "DeckEditorConvertStyleRarity");
            DeckEditorShowStats = Utils.GetValue<bool>(data, "DeckEditorShowStats");
            DuelStarterShowFirstPlayer = Utils.GetValue<bool>(data, "DuelStarterShowFirstPlayer");
            DuelStarterLiveChapterId = Utils.GetValue<int>(data, "DuelStarterLiveChapterId");
            DuelStarterLiveNotLiveTest = Utils.GetValue<bool>(data, "DuelStarterLiveNotLiveTest");
            DuelClientShowRemainingCardsInDeck = Utils.GetValue<bool>(data, "DuelClientShowRemainingCardsInDeck");
            DuelClientMillenniumEye = Utils.GetValue<bool>(data, "DuelClientMillenniumEye");
            DuelClientDisableCameraShake = Utils.GetValue<bool>(data, "DuelClientDisableCameraShake");
            DuelClientDisableCutinAnimations = Utils.GetValue<bool>(data, "DuelClientDisableCutinAnimations");
            DuelClientDisableCutinAnimationsForCardsWithCustomImages = Utils.GetValue<bool>(data, "DuelClientDisableCutinAnimationsForCardsWithCustomImages");
            DuelClientDisableChains = Utils.GetValue<bool>(data, "DuelClientDisableChains");
            DuelClientTimeMultiplier = Utils.GetValue<double>(data, "DuelClientTimeMultiplier");
            ReplayControlsAlwaysEnabled = Utils.GetValue<bool>(data, "ReplayControlsAlwaysEnabled");
            ReplayControlsTimeMultiplier = Utils.GetValue<double>(data, "ReplayControlsTimeMultiplier");
            TimeMultiplier = Utils.GetValue<double>(data, "TimeMultiplier");
            DisableVSync = Utils.GetValue<bool>(data, "DisableVSync");
            ChangeWindowTitleOnLiveMod = Utils.GetValue<bool>(data, "ChangeWindowTitleOnLiveMod");
            AlwaysWin = Utils.GetValue<bool>(data, "AlwaysWin");
            CustomDuelCmdLog = Utils.GetValue<bool>(data, "CustomDuelCmdLog");
            RandomDecksNonRecursive = Utils.GetValue<bool>(data, "RandomDecksNonRecursive");
            RandomDecksDontSetCpuName = Utils.GetValue<bool>(data, "RandomDecksDontSetCpuName");
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
