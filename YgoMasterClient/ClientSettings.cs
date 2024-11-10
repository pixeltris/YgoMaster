using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YgoMaster;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace YgoMasterClient
{
    static partial class ClientSettings
    {
        public static string BaseIP;
        public static int BasePort;
        public static int ProxyPort;
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
        public static bool DuelStarterShowAllBgms;
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
        public static uint UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create;
        public static uint UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative;
        public static bool DisableAsyncImageLoad;
        public static bool WallpaperDisabled;
        public static bool WallpaperCycleEnabled;
        public static bool WallpaperCycleOnEveryHomeVisit;
        public static float WallpaperCycleDelayInSeconds;
        public static float WallpaperCycleFadeWaitInSeconds;
        public static bool CustomBackgroundEnabled;
        public static bool CustomBackgroundBehindParticles;
        public static float CustomBackgroundFadeHideInSeconds;
        public static float CustomBackgroundFadeShowInSeconds;
        public static bool HomeDisableUnusedHeaders;
        public static bool HomeDisableUnusedBanners;
        public static bool HomeDisableUnusedTopics;
        public static bool DontAutoRunServerExe;

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
            UpdateClientTokenIfEmpty();
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

            BaseIP = Utils.GetValue<string>(data, "BaseIP");
            BasePort = Utils.GetValue<int>(data, "BasePort");
            UpdateProxyPort();
            SessionServerPort = Utils.GetValue<int>(data, "SessionServerPort");
            Func<string, string> FixupUrl = (string str) =>
            {
                str = str.Replace("{BaseIP}", BaseIP);
                str = str.Replace("{BasePort}", BasePort.ToString());
                str = str.Replace("{ProxyPort}", ProxyPort.ToString());
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
            DuelStarterShowAllBgms = Utils.GetValue<bool>(data, "DuelStarterShowAllBgms");
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
            UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create = Utils.GetValue<uint>(data, "UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_Create");
            UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative = Utils.GetValue<uint>(data, "UnityPlayerRVA_DownloadHandlerTexture_CUSTOM_InternalGetTextureNative");
            DisableAsyncImageLoad = Utils.GetValue<bool>(data, "DisableAsyncImageLoad");
            WallpaperDisabled = Utils.GetValue<bool>(data, "WallpaperDisabled");
            WallpaperCycleEnabled = Utils.GetValue<bool>(data, "WallpaperCycleEnabled");
            WallpaperCycleOnEveryHomeVisit = Utils.GetValue<bool>(data, "WallpaperCycleOnEveryHomeVisit");
            WallpaperCycleDelayInSeconds = Utils.GetValue<float>(data, "WallpaperCycleDelayInSeconds");
            WallpaperCycleFadeWaitInSeconds = Utils.GetValue<float>(data, "WallpaperCycleFadeWaitInSeconds");
            CustomBackgroundEnabled = Utils.GetValue<bool>(data, "CustomBackgroundEnabled");
            CustomBackgroundBehindParticles = Utils.GetValue<bool>(data, "CustomBackgroundBehindParticles");
            CustomBackgroundFadeHideInSeconds = Utils.GetValue<float>(data, "CustomBackgroundFadeHideInSeconds");
            CustomBackgroundFadeShowInSeconds = Utils.GetValue<float>(data, "CustomBackgroundFadeShowInSeconds");
            HomeDisableUnusedHeaders = Utils.GetValue<bool>(data, "HomeDisableUnusedHeaders");
            HomeDisableUnusedBanners = Utils.GetValue<bool>(data, "HomeDisableUnusedBanners");
            HomeDisableUnusedTopics = Utils.GetValue<bool>(data, "HomeDisableUnusedTopics");
            DontAutoRunServerExe = Utils.GetValue<bool>(data, "DontAutoRunServerExe");

            return LoadText();
        }

        public static bool UpdateClientTokenIfEmpty()
        {
            // regex instead of minijson, to keep comment
            try
            {
                string content = File.ReadAllText(FilePath);
                if (string.IsNullOrEmpty(content))
                {
                    return false;
                }
                string emptyTokenPattern = @"""MultiplayerToken""\s*:\s*""""\s*,";
                if (!Regex.IsMatch(content, emptyTokenPattern))
                {
                    return false;
                }
                string randomToken = Guid.NewGuid().ToString();
                content = Regex.Replace(content, emptyTokenPattern, $"\"MultiplayerToken\": \"{randomToken}\",");
                File.WriteAllText(FilePath, content);
                return true;
            } catch
            {
                return false;
            }
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

        static void UpdateProxyPort()
        {
            // Each YgoMasterClient needs its own proxy server, attempt to find a free port
            // NOTE: Shuffle to reduce chance of duplicate port being used when opening two clients at the same time
            List<int> portsToCheck = new List<int>();
            for (int port = BasePort + 1; port <= BasePort + 20; port++)
            {
                portsToCheck.Add(port);
            }
            foreach (int port in Utils.Shuffle(Utils.Rand, portsToCheck))
            {
                if (IsPortAvailable(port))
                {
                    ProxyPort = port;
                    break;
                }
            }
        }

        static bool IsPortAvailable(int port)
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start();
                tcpListener.Stop();
                return true;
            }
            catch
            {
            }
            return false;
        }

        public static string ResolveIP(string nameOrIP)
        {
            string ip = nameOrIP;
            try
            {
                ip = ip.Replace("https://", string.Empty).Replace("http://", string.Empty);
                if (Uri.CheckHostName(ip) == UriHostNameType.Dns)
                {
                    foreach (IPAddress address in Dns.GetHostAddresses(ip))
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ip = address.ToString();
                            if (MultiplayerLogConnectionState)
                            {
                                Console.WriteLine("Resolved " + nameOrIP + " to " + ip);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return ip;
        }
    }
}
