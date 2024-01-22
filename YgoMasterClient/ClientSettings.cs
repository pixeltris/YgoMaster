using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YgoMaster;

namespace YgoMasterClient
{
    static partial class ClientSettings
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
        public static string ClientSettingsTextFile;
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
        public static bool DeckEditorDisableSorting;
        public static bool DuelClientShowRemainingCardsInDeck;
        public static bool DuelClientMillenniumEye;
        public static bool DuelClientDisableCameraShake;
        public static bool DuelClientDisableCutinAnimations;
        public static bool DuelClientDisableCutinAnimationsForCardsWithCustomImages;
        public static bool DuelClientDisableChains;
        public static double DuelClientSpectatorRapidTimeMultiplayer;
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
        public static DuelReplayCardVisibility DuelReplayCardVisibility;
        public static bool DuelReplayLiveAutoSave;
        public static bool DuelReplayLiveAutoSaveUseSubFolders;
        public static bool DuelReplayAddHomeSubMenuButtons;
        public static float TradeBannerVisibleTimeInSeconds;
        public static int TradeBannerOffsetY;
        public static float TradeActionDelayInSeconds;
        public static bool PvpLogToConsole;
        public static bool PvpLogToFile;
        public static bool PvpDuelTapSyncEnabled;
        public static float EmoteDurationInSeconds;
        public static GameLauncherMode LaunchMode;
        public static List<int> BrokenItems;
        public static uint UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal;
        public static uint UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound;
        public static uint UnityPlayerRVA_AudioClip_CUSTOM_SetData;

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
            ClientSettingsTextFile = Utils.GetValue<string>(data, "ClientSettingsTextFile");
            ShowConsole = Utils.GetValue<bool>(data, "ShowConsole");
            LogIDs = Utils.GetValue<bool>(data, "LogIDs");
            AssetHelperLog = Utils.GetValue<bool>(data, "AssetHelperLog");
            AssetHelperDump = Utils.GetValue<bool>(data, "AssetHelperDump");
            AssetHelperDisableFileErrorPopup = Utils.GetValue<bool>(data, "AssetHelperDisableFileErrorPopup");
            DeckEditorDisableLimits = Utils.GetValue<bool>(data, "DeckEditorDisableLimits");
            DeckEditorConvertStyleRarity = Utils.GetValue<bool>(data, "DeckEditorConvertStyleRarity");
            DeckEditorShowStats = Utils.GetValue<bool>(data, "DeckEditorShowStats");
            DeckEditorDisableSorting = Utils.GetValue<bool>(data, "DeckEditorDisableSorting");
            DuelStarterShowFirstPlayer = Utils.GetValue<bool>(data, "DuelStarterShowFirstPlayer");
            DuelStarterLiveChapterId = Utils.GetValue<int>(data, "DuelStarterLiveChapterId");
            DuelStarterLiveNotLiveTest = Utils.GetValue<bool>(data, "DuelStarterLiveNotLiveTest");
            DuelClientShowRemainingCardsInDeck = Utils.GetValue<bool>(data, "DuelClientShowRemainingCardsInDeck");
            DuelClientMillenniumEye = Utils.GetValue<bool>(data, "DuelClientMillenniumEye");
            DuelClientDisableCameraShake = Utils.GetValue<bool>(data, "DuelClientDisableCameraShake");
            DuelClientDisableCutinAnimations = Utils.GetValue<bool>(data, "DuelClientDisableCutinAnimations");
            DuelClientDisableCutinAnimationsForCardsWithCustomImages = Utils.GetValue<bool>(data, "DuelClientDisableCutinAnimationsForCardsWithCustomImages");
            DuelClientDisableChains = Utils.GetValue<bool>(data, "DuelClientDisableChains");
            DuelClientSpectatorRapidTimeMultiplayer = Utils.GetValue<double>(data, "DuelClientSpectatorRapidTimeMultiplayer");
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
            DuelReplayCardVisibility = Utils.GetValue<DuelReplayCardVisibility>(data, "DuelReplayCardVisibility");
            DuelReplayLiveAutoSave = Utils.GetValue<bool>(data, "DuelReplayLiveAutoSave");
            DuelReplayLiveAutoSaveUseSubFolders = Utils.GetValue<bool>(data, "DuelReplayLiveAutoSaveUseSubFolders");
            DuelReplayAddHomeSubMenuButtons = Utils.GetValue<bool>(data, "DuelReplayAddHomeSubMenuButtons");
            TradeBannerVisibleTimeInSeconds = Utils.GetValue<float>(data, "TradeBannerVisibleTimeInSeconds");
            TradeBannerOffsetY = Utils.GetValue<int>(data, "TradeBannerOffsetY");
            TradeActionDelayInSeconds = Utils.GetValue<float>(data, "TradeActionDelayInSeconds");
            PvpLogToConsole = Utils.GetValue<bool>(data, "PvpLogToConsole");
            PvpLogToFile = Utils.GetValue<bool>(data, "PvpLogToFile");
            PvpDuelTapSyncEnabled = Utils.GetValue<bool>(data, "PvpDuelTapSyncEnabled");
            EmoteDurationInSeconds = Utils.GetValue<float>(data, "EmoteDurationInSeconds");
            LaunchMode = Utils.GetValue<GameLauncherMode>(data, "LaunchMode");
            BrokenItems = Utils.GetIntList(data, "BrokenItems");
            UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal = Utils.GetValue<uint>(data, "UnityPlayerRVA_AudioClip_CUSTOM_Construct_Internal");
            UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound = Utils.GetValue<uint>(data, "UnityPlayerRVA_AudioClip_CUSTOM_CreateUserSound");
            UnityPlayerRVA_AudioClip_CUSTOM_SetData = Utils.GetValue<uint>(data, "UnityPlayerRVA_AudioClip_CUSTOM_SetData");

            return LoadText();
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
