using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Reflection;

namespace YgoMaster
{
    partial class GameServer
    {
        static readonly Version highestSupportedClientVersion = new Version(int.MaxValue, int.MaxValue);

        static readonly string deckSearchUrl = "https://ayk-deck.mo.konami.net/ayk/yocgapi/search";
        static readonly string deckSearchDetailUrl = "https://ayk-deck.mo.konami.net/ayk/yocgapi/detail";
        static readonly string deckSearchAttributesUrl = "https://ayk-deck.mo.konami.net/ayk/yocgapi/attributes";

        string sessionServerIP;
        int sessionServerPort;
        string multiplayerPvpClientConnectIP;
        int multiplayerPvpClientConnectPort;

        string bindIP;

        string ygoMasterExePath;
        string dataDirectory;
        string playersDirectory;
        string settingsFile;

        Random rand = new Random();

        Player localPlayer;
        FileSystemWatcher localPlayerDecksFileWatcher;

        object playersLock = new object();
        Dictionary<string, Player> playersByToken = new Dictionary<string, Player>();
        Dictionary<uint, Player> playersById = new Dictionary<uint, Player>();
        /// <summary>
        /// NOTE: Unused if zero IP release time (MultiplayerReleasePlayerIPInHours)
        /// </summary>
        Dictionary<string, HashSet<string>> tokensByIP = new Dictionary<string, HashSet<string>>();

        object duelRoomsLocker = new object();
        Dictionary<uint, DuelRoom> duelRoomsByRoomId = new Dictionary<uint, DuelRoom>();
        Dictionary<uint, DuelRoom> duelRoomsBySpectatorRoomId = new Dictionary<uint, DuelRoom>();
        URNG.LinearCongruentialGenerator duelRoomIdRng;
        URNG.LinearCongruentialGenerator duelRoomSpectatorRoomIdRng;

        int NumDeckSlots;
        bool UnlockAllCards;
        bool UnlockAllCardsHighestRarity;
        bool UnlockAllCardsShine;
        bool UnlockAllItems;
        bool UnlockAllSoloChapters;
        bool SoloDisableNoShuffle;
        HashSet<int> DefaultItems;
        int DefaultGems;
        Dictionary<string, object> DefaultCraftPoints;
        CraftInfo Craft;
        ShopInfo Shop;
        Dictionary<int, DeckInfo> StructureDecks;// <structureid, DeckInfo>
        Dictionary<int, int> CardRare;
        List<int> CardCraftable;
        bool CardCraftableAll;
        Dictionary<string, object> TitleLoop;
        Dictionary<string, object> Regulation;
        Dictionary<string, object> RegulationIcon;
        Dictionary<string, object> RegulationInfo;
        Dictionary<string, object> AccessorySet;
        Dictionary<string, object> SoloData;
        Dictionary<int, DuelSettings> SoloDuels;// <chapterid, DuelSettings>
        Dictionary<int, CardCategory> CardCategories;// <categoryId, CardCategory>
        Dictionary<string, CardCategory> CardCategoriesByName;// <categoryName, CardCategory>
        DuelSettings CustomDuelSettings;
        DateTime CustomDuelLastModified;
        DuelRewardInfos DuelRewards;
        DuelRewardInfos DuelRoomRewards;
        /// <summary>
        /// Cards will only be visible in the trunk if they are obtainable via the shop
        /// </summary>
        bool ProgressiveCardList;
        /// <summary>
        /// Card rarities will be determined based on the following criteria:
        /// - Find the lowest rarity of the given card in all unlocked packs
        /// - If the lowest found rarity is higher than the main rarity list then the pack rarity will be used, else use the main rarity
        /// </summary>
        bool ProgressiveCardRarities;
        /// <summary>
        /// In solo mode show "gate clear" in order to display the secret pack unlock for chapters which aren't flagged as the goal
        /// </summary>
        bool SoloShowGateClearForAllSecretPacks;
        /// <summary>
        /// Put all solo rewards in the duel result screen (as opposed to the reward dialog popup)
        /// </summary>
        bool SoloRewardsInDuelResult;
        /// <summary>
        /// Solo rewards in the duel result screen are flagged as rare (gold box)
        /// </summary>
        bool SoloRewardsInDuelResultAreRare;
        /// <summary>
        /// Show information about YgoMaster in the topics panel on the home screen
        /// </summary>
        bool ShowTopics;
        /// <summary>
        /// Any card which is given the player can be dismantled
        /// </summary>
        bool DisableNoDismantle;
        /// <summary>
        /// How many search results to return for friend searches
        /// </summary>
        int FriendSearchLimit;
        /// <summary>
        /// How long to wait before the client sends a Friend.refresh_info request
        /// </summary>
        int FriendsRefreshInSeconds;
        /// <summary>
        /// How long until a friend is seen as offline
        /// </summary>
        int FriendOfflineInSeconds;
        /// <summary>
        /// Maximum value of a duel room id
        /// </summary>
        int DuelRoomMaxId;
        /// <summary>
        /// Maximum number of results for the duel room list
        /// </summary>
        int DuelRoomMaxSearchResults;
        /// <summary>
        /// Maximum number of spectators allowed in a duel room / duel
        /// </summary>
        int DuelRoomMaxSpectators;
        /// <summary>
        /// How long the comment speech bubbles should appear for in seconds
        /// </summary>
        int DuelRoomCommentTimeoutInSeconds;
        /// <summary>
        /// Cancel the matching if the duel hasn't fully started after this period of time
        /// </summary>
        int DuelRoomTableMatchingTimeoutInSeconds;
        /// <summary>
        /// How long to wait for the pvp client to set up before abandoning the duel
        /// </summary>
        int DuelRoomPvpClientSetupTimeoutInSeconds;
        /// <summary>
        /// Visibility of cards when spectating duels in duel rooms
        /// </summary>
        DuelReplayCardVisibility DuelRoomSpectatorCardVisibility;
        /// <summary>
        /// Default duel room time index to use from the duel room times list
        /// </summary>
        int DuelRoomDefaultTimeIndex;
        /// <summary>
        /// Duel room times (name / time in seconds)
        /// </summary>
        List<DuelTimerInfo> DuelRoomTimes;
        /// <summary>
        /// Game modes where replays will save
        /// </summary>
        List<GameMode> DuelReplaySaveForGameModes;
        /// <summary>
        /// How many replay save files there can be. If another save is made it'll auto delete the oldest file (based on created date)
        /// </summary>
        int DuelReplaySaveFileLimit;
        /// <summary>
        /// Should should replays be flagged as public by default
        /// </summary>
        bool DuelReplayMakePublicByDefault;
        /// <summary>
        /// Visibility of cards in duel replays
        /// </summary>
        DuelReplayCardVisibility DuelReplayCardVisibility;
        /// <summary>
        /// Allows mulitple connections
        /// </summary>
        bool MultiplayerEnabled;
        /// <summary>
        /// Allow the user to specify a token such as "111-111-111" to be given that specific player code
        /// </summary>
        bool MultiplayerAllowUserSpecifiedPlayerCode;
        /// <summary>
        /// If non-null/empty this prefix is required on all user provided tokens. Any requests without this prefix will be ignored
        /// </summary>
        string MultiplayerTokenPrefixSecret;
        /// <summary>
        /// A maximum token limit per IP to avoid spamming of different tokens (which would make many player folders)
        /// </summary>
        int MultiplayerMaxTokensPerIP;
        /// <summary>
        /// Specific max token limits per IP
        /// </summary>
        Dictionary<string, int> MultiplayerMaxTokensPerIPEx;
        /// <summary>
        /// How long to wait until a player is removed from the given IP's player list
        /// </summary>
        int MultiplayerReleaseTokenIPInHours;
        /// <summary>
        /// How long with no response until a session is closed
        /// </summary>
        int MultiplayerPingTimeoutInSeconds;
        /// <summary>
        /// How often to ping a session
        /// </summary>
        int MultiplayerPingInSeconds;
        /// <summary>
        /// Enable NoDelay / don't use the TCP nagle algorithm
        /// </summary>
        public bool MultiplayerNoDelay;
        /// <summary>
        /// Disable deck shuffling under multiplayer
        /// </summary>
        public bool MultiplayerNoShuffle;
        /// <summary>
        /// The seed used in multiplayer duels (-1 is random)
        /// </summary>
        public int MultiplayerSeed;
        /// <summary>
        /// Who should win the coin flip 
        /// </summary>
        public int MultiplayerCoinFlipPlayerIndex;
        /// <summary>
        /// How many times the coin flip counter should count
        /// </summary>
        public int MultiplayerCoinFlipCounter;
        /// <summary>
        /// Disable deck validation in solo and duel rooms
        /// </summary>
        bool DisableDeckValidation;
        /// <summary>
        /// Allows the other player in the trade to add your cards to the trade
        /// </summary>
        public bool TradeAllowOtherPlayerToAddYourCards;
        /// <summary>
        /// Allows the other player to remove your cards from the trade
        /// </summary>
        public bool TradeAllowOtherPlayerToRemoveYourCards;
        /// <summary>
        /// A delay between each enter trade room request to avoid state desync if spamming the button
        /// </summary>
        public float TradeEnterRoomRequestDelayInSeconds;
        /// <summary>
        /// How long an emote can be (in characters). If an emote is longer than this it'll be ignored
        /// </summary>
        public int EmoteMaxLength;
        /// <summary>
        /// Should the pvp client console be visible
        /// </summary>
        public bool MultiplayerPvpClientShowConsole;
        /// <summary>
        /// Keep the pvp client console alive after the duel finishes
        /// </summary>
        public bool MultiplayerPvpClientKeepConsoleAlive;
        /// <summary>
        /// How long to sleep for after each sysact call
        /// </summary>
        public int MultiplayerPvpClientSysActSleepInMilliseconds;
        /// <summary>
        /// How many times to call sysact per loop
        /// </summary>
        public int MultiplayerPvpClientSysActCallsPerSleep;
        /// <summary>
        /// Offset for getting the active player for a given DoCommand in duel.dll
        /// </summary>
        public int MultiplayerPvpClientDoCommandUserOffset;
        /// <summary>
        /// Offset for getting the active player for a given RunDialog in duel.dll
        /// </summary>
        public int MultiplayerPvpClientRunDialogUserOffset;
        /// <summary>
        /// Doesn't update the deck edit time when the deck is edited (this will preserve the deck order of the deck list)
        /// </summary>
        public bool DontUpdateDeckEditTime;
        /// <summary>
        /// Forces the deck list to be sorted alphabetically
        /// </summary>
        public bool DeckListAlphabetical;
        /// <summary>
        /// Forces the deck list to be sorted by deck box then alphabetically
        /// </summary>
        public bool DeckListByBoxThenAlphabetical;

        void LoadSettings()
        {
            ygoMasterExePath = Assembly.GetExecutingAssembly().Location;
            dataDirectory = Utils.GetDataDirectory(true);
            if (!Directory.Exists(dataDirectory))
            {
                Utils.LogWarning("Failed to find data directory '" + dataDirectory + "'");
                return;
            }
            settingsFile = Path.Combine(dataDirectory, "Settings.json");
            playersDirectory = Path.Combine(dataDirectory, "Players");
            if (!MultiplayerEnabled)
            {
                Utils.TryCreateDirectory(GetDecksDirectory(null));
            }

            if (!File.Exists(settingsFile))
            {
                Utils.LogWarning("Failed to load settings file");
                return;
            }

            string clientDataDir = Path.Combine(dataDirectory, "ClientData");
            string clientSettingsFile = Path.Combine(clientDataDir, "ClientSettings.json");
            if (File.Exists(clientSettingsFile))
            {
                Dictionary<string, object> clientValues = MiniJSON.Json.DeserializeStripped(File.ReadAllText(clientSettingsFile)) as Dictionary<string, object>;
                if (clientValues != null)
                {
                    string clientSettingsTextFile = Utils.GetValue<string>(clientValues, "ClientSettingsTextFile");
                    if (!string.IsNullOrEmpty(clientSettingsTextFile))
                    {
                        clientSettingsTextFile = Path.Combine(clientDataDir, clientSettingsTextFile);
                        if (File.Exists(clientSettingsTextFile))
                        {
                            Dictionary<string, object> clientTextValues = MiniJSON.Json.DeserializeStripped(File.ReadAllText(clientSettingsTextFile)) as Dictionary<string, object>;
                            if (clientTextValues != null)
                            {
                                DuelSettings.DefaultNamePlayer = Utils.GetValue<string>(clientTextValues, "CustomTextDuelSettingsDefaultNamePlayer");
                                DuelSettings.DefaultNameCPU = Utils.GetValue<string>(clientTextValues, "CustomTextDuelSettingsDefaultNameCPU");
                            }
                        }
                    }
                }
            }
            DuelSettings.LoadBgmInfo(Path.Combine(dataDirectory, "Bgm.json"));

            ItemID.Load(dataDirectory);
            YdkHelper.LoadIdMap(dataDirectory);

            //DuelSimulator sim = new DuelSimulator(dataDirectory);
            //sim.Init();

            string text = File.ReadAllText(settingsFile);
            Dictionary<string, object> values = MiniJSON.Json.DeserializeStripped(text) as Dictionary<string, object>;
            if (values == null)
            {
                throw new Exception("Failed to parse settings json");
            }

            string baseIP = Utils.GetValue<string>(values, "BaseIP");
            int basePort = Utils.GetValue<int>(values, "BasePort");
            sessionServerPort = Utils.GetValue<int>(values, "SessionServerPort");
            Func<string, string> FixupUrl = (string str) =>
            {
                str = str.Replace("{BaseIP}", baseIP);
                str = str.Replace("{BasePort}", basePort.ToString());
                str = str.Replace("{SessionServerPort}", sessionServerPort.ToString());
                str = str.Replace("{SessionServerIP}", sessionServerIP);
                if (str == "localhost")
                {
                    str = "127.0.0.1";
                }
                return str;
            };

            sessionServerIP = FixupUrl(Utils.GetValue<string>(values, "SessionServerIP"));
            bindIP = FixupUrl(Utils.GetValue<string>(values, "BindIP"));
            multiplayerPvpClientConnectIP = FixupUrl(Utils.GetValue<string>(values, "MultiplayerPvpClientConnectIP"));
            multiplayerPvpClientConnectPort = Utils.GetValue<int>(values, "MultiplayerPvpClientConnectPort");
            if (multiplayerPvpClientConnectPort == 0)
            {
                multiplayerPvpClientConnectPort = sessionServerPort;
            }
            if (string.IsNullOrEmpty(bindIP) ||
                string.IsNullOrEmpty(multiplayerPvpClientConnectIP) || multiplayerPvpClientConnectPort == 0)
            {
                throw new Exception("Failed to get server url settings");
            }

            MultiplayerEnabled = Utils.GetValue<bool>(values, "MultiplayerEnabled");
            MultiplayerAllowUserSpecifiedPlayerCode = Utils.GetValue<bool>(values, "MultiplayerAllowUserSpecifiedPlayerCode");
            MultiplayerTokenPrefixSecret = Utils.GetValue<string>(values, "MultiplayerTokenPrefixSecret");
            MultiplayerMaxTokensPerIP = Utils.GetValue<int>(values, "MultiplayerMaxTokensPerIP", 6);
            MultiplayerMaxTokensPerIPEx = new Dictionary<string, int>();
            Dictionary<string, object> maxPlayersPerIPData = Utils.GetDictionary(values, "MultiplayerMaxTokensPerIPEx");
            if (maxPlayersPerIPData != null && maxPlayersPerIPData.Count > 0)
            {
                foreach (KeyValuePair<string, object> ipEntry in maxPlayersPerIPData)
                {
                    IPAddress ipAddress;
                    if (IPAddress.TryParse(ipEntry.Key, out ipAddress))
                    {
                        MultiplayerMaxTokensPerIPEx[ipEntry.Key] = (int)Convert.ChangeType(ipEntry.Value, typeof(int));
                    }
                }
            }
            MultiplayerReleaseTokenIPInHours = Utils.GetValue<int>(values, "MultiplayerReleaseTokenIPInHours");
            MultiplayerPingTimeoutInSeconds = Utils.GetValue<int>(values, "MultiplayerPingTimeoutInSeconds");
            MultiplayerPingInSeconds = Utils.GetValue<int>(values, "MultiplayerPingInSeconds");
            MultiplayerNoDelay = Utils.GetValue<bool>(values, "MultiplayerNoDelay");
            MultiplayerNoShuffle = Utils.GetValue<bool>(values, "MultiplayerNoShuffle");
            MultiplayerSeed = Utils.GetValue<int>(values, "MultiplayerSeed");
            MultiplayerCoinFlipPlayerIndex = Utils.GetValue<int>(values, "MultiplayerCoinFlipPlayerIndex");
            MultiplayerCoinFlipCounter = Utils.GetValue<int>(values, "MultiplayerCoinFlipCounter");

            NumDeckSlots = Utils.GetValue<int>(values, "DeckSlots", 20);
            Utils.GetIntHashSet(values, "DefaultItems", DefaultItems = new HashSet<int>(), ignoreZero: true);
            DefaultGems = Utils.GetValue<int>(values, "DefaultGems");
            DefaultCraftPoints = Utils.GetDictionary(values, "DefaultCraftPoints");
            UnlockAllCards = Utils.GetValue<bool>(values, "UnlockAllCards");
            UnlockAllCardsHighestRarity = Utils.GetValue<bool>(values, "UnlockAllCardsHighestRarity");
            UnlockAllCardsShine = Utils.GetValue<bool>(values, "UnlockAllCardsShine");
            UnlockAllItems = Utils.GetValue<bool>(values, "UnlockAllItems");
            UnlockAllSoloChapters = Utils.GetValue<bool>(values, "UnlockAllSoloChapters");
            SoloDisableNoShuffle = Utils.GetValue<bool>(values, "SoloDisableNoShuffle");
            SoloShowGateClearForAllSecretPacks = Utils.GetValue<bool>(values, "SoloShowGateClearForAllSecretPacks");
            SoloRewardsInDuelResult = Utils.GetValue<bool>(values, "SoloRewardsInDuelResult");
            SoloRewardsInDuelResultAreRare = Utils.GetValue<bool>(values, "SoloRewardsInDuelResultAreRare");
            ProgressiveCardList = Utils.GetValue<bool>(values, "ProgressiveCardList");
            ProgressiveCardRarities = Utils.GetValue<bool>(values, "ProgressiveCardRarities");
            ShowTopics = Utils.GetValue<bool>(values, "ShowTopics");
            DisableNoDismantle = Utils.GetValue<bool>(values, "DisableNoDismantle");
            DisableDeckValidation = Utils.GetValue<bool>(values, "DisableDeckValidation");
            DontUpdateDeckEditTime = Utils.GetValue<bool>(values, "DontUpdateDeckEditTime");
            DeckListAlphabetical = Utils.GetValue<bool>(values, "DeckListAlphabetical");
            DeckListByBoxThenAlphabetical = Utils.GetValue<bool>(values, "DeckListByBoxThenAlphabetical");
            TradeAllowOtherPlayerToAddYourCards = Utils.GetValue<bool>(values, "TradeAllowOtherPlayerToAddYourCards");
            TradeAllowOtherPlayerToRemoveYourCards = Utils.GetValue<bool>(values, "TradeAllowOtherPlayerToRemoveYourCards");
            TradeEnterRoomRequestDelayInSeconds = Utils.GetValue<float>(values, "TradeEnterRoomRequestDelayInSeconds");
            EmoteMaxLength = Utils.GetValue<int>(values, "EmoteMaxLength");
            MultiplayerPvpClientShowConsole = Utils.GetValue<bool>(values, "MultiplayerPvpClientShowConsole");
            MultiplayerPvpClientKeepConsoleAlive = Utils.GetValue<bool>(values, "MultiplayerPvpClientKeepConsoleAlive");
            MultiplayerPvpClientSysActSleepInMilliseconds = Utils.GetValue<int>(values, "MultiplayerPvpClientSysActSleepInMilliseconds");
            MultiplayerPvpClientSysActCallsPerSleep = Utils.GetValue<int>(values, "MultiplayerPvpClientSysActCallsPerSleep");
            MultiplayerPvpClientDoCommandUserOffset = Utils.GetValue<int>(values, "MultiplayerPvpClientDoCommandUserOffset");
            MultiplayerPvpClientRunDialogUserOffset = Utils.GetValue<int>(values, "MultiplayerPvpClientRunDialogUserOffset");

            FriendSearchLimit = Utils.GetValue<int>(values, "FriendSearchLimit", 100);
            FriendOfflineInSeconds = Utils.GetValue<int>(values, "FriendOfflineInSeconds", 300);

            const int FriendsRefreshInSecondsDefault = 15;
            FriendsRefreshInSeconds = Utils.GetValue<int>(values, "FriendsRefreshInSeconds", FriendsRefreshInSecondsDefault);
            if (FriendsRefreshInSeconds < 1)
            {
                FriendsRefreshInSeconds = FriendsRefreshInSecondsDefault;
            }

            DuelRoomMaxId = Utils.GetValue<int>(values, "DuelRoomMaxId", 999999);
            duelRoomIdRng = new URNG.LinearCongruentialGenerator(rand.Next(), 0, DuelRoomMaxId);
            duelRoomSpectatorRoomIdRng = new URNG.LinearCongruentialGenerator(rand.Next(), 0, DuelRoomMaxId);

            DuelRoomMaxSearchResults = Utils.GetValue<int>(values, "DuelRoomMaxSearchResults", 50);
            DuelRoomMaxSpectators = Utils.GetValue<int>(values, "DuelRoomMaxSpectators", 100);
            DuelRoomCommentTimeoutInSeconds = Utils.GetValue<int>(values, "DuelRoomCommentTimeoutInSeconds", 7);
            DuelRoomTableMatchingTimeoutInSeconds = Utils.GetValue<int>(values, "DuelRoomTableMatchingTimeoutInSeconds", 30);
            DuelRoomPvpClientSetupTimeoutInSeconds = Utils.GetValue<int>(values, "DuelRoomPvpClientSetupTimeoutInSeconds", 30);
            DuelRoomSpectatorCardVisibility = Utils.GetValue<DuelReplayCardVisibility>(values, "DuelRoomSpectatorCardVisibility");
            DuelRoomDefaultTimeIndex = Utils.GetValue<int>(values, "DuelRoomDefaultTimeIndex");
            DuelRoomTimes = new List<DuelTimerInfo>();
            List<object> roomTimes = Utils.GetValue<List<object>>(values, "DuelRoomTimes");
            foreach (object roomTimeObj in roomTimes)
            {
                Dictionary<string, object> roomTimeData = roomTimeObj as Dictionary<string, object>;
                DuelTimerInfo timer = new DuelTimerInfo();
                timer.Name = Utils.GetValue<string>(roomTimeData, "Name");
                timer.Time = Utils.GetValue<int>(roomTimeData, "Time");
                timer.AddTimeAtStartOfTurn = Utils.GetValue<int>(roomTimeData, "AddTimeAtStartOfTurn");
                timer.AddTimeAtEndOfTurn = Utils.GetValue<int>(roomTimeData, "AddTimeAtEndOfTurn");
                if (!Utils.TryGetValue(roomTimeData, "TurnTimeIndicator", out timer.TurnTimeIndicator))
                {
                    timer.TurnTimeIndicator = timer.AddTimeAtStartOfTurn + timer.AddTimeAtEndOfTurn;
                }
                DuelRoomTimes.Add(timer);
            }

            DuelReplaySaveForGameModes = Utils.GetValueTypeList<GameMode>(values, "DuelReplaySaveForGameModes");
            DuelReplaySaveFileLimit = Utils.GetValue<int>(values, "DuelReplaySaveFileLimit");
            DuelReplayMakePublicByDefault = Utils.GetValue<bool>(values, "DuelReplayMakePublicByDefault");
            DuelReplayCardVisibility = Utils.GetValue<DuelReplayCardVisibility>(values, "DuelReplayCardVisibility");

            CardRare = new Dictionary<int, int>();
            string cardListFile = Path.Combine(dataDirectory, "CardList.json");
            if (File.Exists(cardListFile))
            {
                Dictionary<string, object> cardRareDict = MiniJSON.Json.DeserializeStripped(File.ReadAllText(cardListFile)) as Dictionary<string, object>;
                if (cardRareDict != null)
                {
                    foreach (KeyValuePair<string, object> item in cardRareDict)
                    {
                        CardRare[int.Parse(item.Key)] = (int)Convert.ChangeType(item.Value, typeof(int));
                    }
                }
            }

            CardCraftable = new List<int>();
            string cardCraftableListFile = Path.Combine(dataDirectory, "CardCraftableList.json");
            if (File.Exists(cardCraftableListFile))
            {
                List<object> cardCrList = MiniJSON.Json.DeserializeStripped(File.ReadAllText(cardCraftableListFile)) as List<object>;
                if (cardCrList != null)
                {
                    foreach (object id in cardCrList)
                    {
                        CardCraftable.Add((int)Convert.ChangeType(id, typeof(int)));
                    }
                }
            }
            if (Utils.GetValue<bool>(values, "CardCraftableAll"))
            {
                CardCraftableAll = true;
                CardCraftable.Clear();
                foreach (int cardId in CardRare.Keys)
                {
                    CardCraftable.Add(cardId);
                }
            }

            string regulationIconFile = Path.Combine(dataDirectory, "RegulationIcon.json");
            if (File.Exists(regulationIconFile))
            {
                RegulationIcon = MiniJSON.Json.DeserializeStripped(File.ReadAllText(regulationIconFile)) as Dictionary<string, object>;
            }

            string regulationInfoFile = Path.Combine(dataDirectory, "RegulationInfo.json");
            if (File.Exists(regulationInfoFile))
            {
                RegulationInfo = MiniJSON.Json.DeserializeStripped(File.ReadAllText(regulationInfoFile)) as Dictionary<string, object>;
            }

            string accessorySetFile = Path.Combine(dataDirectory, "AccessorySet.json");
            if (File.Exists(accessorySetFile))
            {
                AccessorySet = MiniJSON.Json.DeserializeStripped(File.ReadAllText(accessorySetFile)) as Dictionary<string, object>;
            }
            else
            {
                AccessorySet = new Dictionary<string, object>();
                if (ItemID.Values.ContainsKey(ItemID.Category.FIELD) &&
                    ItemID.Values.ContainsKey(ItemID.Category.FIELD_OBJ) &&
                    ItemID.Values.ContainsKey(ItemID.Category.AVATAR_HOME))
                {
                    Dictionary<string, object> accessory_set_list = Utils.GetOrCreateDictionary(AccessorySet, "accessory_set_list");
                    Dictionary<string, object> item_list = Utils.GetOrCreateDictionary(AccessorySet, "item_list");
                    foreach (int fieldId in ItemID.Values[ItemID.Category.FIELD])
                    {
                        int baseId = fieldId - 1090000;
                        if (baseId <= 0)
                        {
                            continue;
                        }
                        int objId = ItemID.GetFieldObjFromField(fieldId);
                        int avatarId = ItemID.GetFieldAvatarBaseFromField(fieldId);
                        if (ItemID.Values[ItemID.Category.FIELD_OBJ].Contains(objId) &&
                            ItemID.Values[ItemID.Category.AVATAR_HOME].Contains(avatarId))
                        {
                            accessory_set_list[baseId.ToString()] = new Dictionary<string, object>()
                            {
                                { "field", fieldId },
                                { "object", objId },
                                { "av_base", avatarId },
                            };
                            item_list[fieldId.ToString()] = baseId;
                            item_list[objId.ToString()] = baseId;
                            item_list[avatarId.ToString()] = baseId;
                        }
                    }
                }
            }

            string TitleLoopFile = Path.Combine(dataDirectory, "TitleLoop.json");
            if (File.Exists(TitleLoopFile))
            {
                TitleLoop = MiniJSON.Json.DeserializeStripped(File.ReadAllText(TitleLoopFile)) as Dictionary<string, object>;
            }

            string regulationFile = Path.Combine(dataDirectory, "Regulation.json");
            if (File.Exists(regulationFile))
            {
                Regulation = MiniJSON.Json.DeserializeStripped(File.ReadAllText(regulationFile)) as Dictionary<string, object>;

                if (Regulation != null && RegulationInfo != null)
                {
                    DeckInfo.DefaultRegulationId = int.Parse(Utils.GetDictionary(RegulationInfo, "rule_list").FirstOrDefault(x => (string)x.Value == DeckInfo.DefaultRegulationName).Key);

                    Dictionary<string, object> regulations = Utils.GetDictionary(RegulationInfo, "rule_list");
                    foreach (KeyValuePair<string, object> regulation in regulations)
                    {
                        DeckInfo.RegulationIdsByName.Add(regulation.Value as string, int.Parse(regulation.Key));
                        DeckInfo.RegulationNamesById.Add(int.Parse(regulation.Key), regulation.Value as string);
                    }
                }
            }
            if (Utils.GetValue<bool>(values, "DisableBanList") && Regulation != null)
            {
                Dictionary<string, object> defaultRegulation = Utils.GetDictionary(Regulation, DeckInfo.DefaultRegulationId.ToString());
                if (defaultRegulation != null)
                {
                    Dictionary<string, object> available = Utils.GetDictionary(defaultRegulation, "available");
                    if (available != null)
                    {
                        for (int i = 0; i <= 3; i++)
                        {
                            available["a" + i] = new int[0];
                        }
                    }
                }
            }

            Craft = new CraftInfo();
            Craft.FromDictionary(Utils.GetValue(values, "Craft", default(Dictionary<string, object>)));

            LoadStructureDecks();
            LoadCardCategory();
            LoadShop();
            LoadSolo();

            DuelRewards = new DuelRewardInfos();
            DuelRewards.FromDictionary(Utils.GetDictionary(values, "DuelRewards"));

            DuelRoomRewards = new DuelRewardInfos();
            DuelRoomRewards.FromDictionary(Utils.GetDictionary(values, "DuelRoomRewards"));

            if (MultiplayerEnabled)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Console.WriteLine("Loading players...");

                if (Directory.Exists(playersDirectory))
                {
                    foreach (string dir in Directory.GetDirectories(Path.Combine(playersDirectory)))
                    {
                        uint playerCode;
                        if (uint.TryParse(new DirectoryInfo(dir).Name, out playerCode) && playerCode > 0)
                        {
                            string playerFile = Path.Combine(dir, "Player.json");
                            if (File.Exists(playerFile))
                            {
                                Dictionary<string, object> playerData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(playerFile)) as Dictionary<string, object>;
                                uint playerCodeInData;
                                string token;
                                if (playerData != null && Utils.TryGetValue(playerData, "Code", out playerCodeInData) && playerCodeInData == playerCode &&
                                    Utils.TryGetValue(playerData, "Token", out token) && !string.IsNullOrEmpty(token) && GetPlayerIdFromToken(token) == playerCode)
                                {
                                    Player player = playersByToken[token] = playersById[playerCode] = new Player(playerCode);
                                    LoadPlayer(player);
                                }
                                else
                                {
                                    Utils.LogWarning("Failed to load player " + Utils.FormatPlayerCode(playerCode) + " as the token is invalid or the player code in the file doesn't match");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Loaded " + playersById.Count + " players in " + stopwatch.Elapsed.TotalSeconds + " seconds");
            }

            InitDecksWatcher();

            // TODO: Move elsewhere (these are helpers to generate files)
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                bool ranCommand = false;
                for (int i = 1; i < args.Length; i++)
                {
                    bool log = true;
                    string arg = args[i].ToLowerInvariant();
                    switch (arg)
                    {
                        case "--mergeshops":// Gets all logs from "/ShopDumps/" and merges them into "AllShopsMerged.json"
                            MergeShopDumps();
                            break;
                        case "--shopsfromsets":// Scrapes ygo db for sets and creates shops from them based on "OfficialSetsExtraData.json"
                            CreateShopsFromOfficialSets();
                            break;
                        case "--le2master":
                            ConvertLeDataToSolo();
                            break;
                        case "--extractstructure":// Takes "StructureDecks.json" (obtained via "Master.Structure") and expands them into "/StructureDecks/"
                            ExtractStructureDecks();
                            break;
                        case "--updateydk":// Updates "YdkIds.txt" based on a YgoMasterClient "carddata" dump and ygoprodeck cardinfo.php
                            YdkHelper.GenerateIdMap(dataDirectory);
                            break;
#if WITH_WIKI_DUMPER
                        case "--wiki-decks":
                            YgoWikiDecks.Dump(dataDirectory, CardRare);
                            break;
#endif
                        case "--cpucontest":
                            CpuContest cpuContest = new CpuContest(dataDirectory);
                            cpuContest.Run();
                            Environment.Exit(0);
                            break;
                        case "--missing-cards":// Dumps cards which are in the data files but not in the card list
                            DumpMissingCards();
                            break;
                        case "--duel-field-bgms":// Dumps which duel fields give which BGM based on /Data/SoloDuels/ and /Data/BgmDuelLogs/
                            DumpDuelFieldBgms();
                            break;
                        case "--update":// Updates json files by auto fetching various packets
                            if (args.Length <= i + 2)
                            {
                                Console.WriteLine("Expected token args");
                            }
                            else
                            {
                                // base64 token / atoken
                                Updater updater = new Updater(CardRare);
                                updater.Run(args[i + 1], args[i + 2]);
                                i += 2;
                            }
                            break;
                        case "--namedesc":// Dumps card name / desc of a given card id from data in /CardData/
                            if (args.Length <= i + 1)
                            {
                                Console.WriteLine("Expected card id");
                            }
                            else
                            {
                                int cid = int.Parse(args[i + 1]);
                                Dictionary<int, YdkHelper.GameCardInfo> cards = YdkHelper.LoadCardDataFromGame(dataDirectory);
                                YdkHelper.GameCardInfo cardInfo;
                                if (cards.TryGetValue(cid, out cardInfo))
                                {
                                    Console.WriteLine("Name: " + cardInfo.Name);
                                    Console.WriteLine("Desc: " + cardInfo.Desc);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to find card");
                                }
                                i++;
                            }
                            break;
                        case "--convert-deck":// Converts a file (or folder recursively) between ydk / json
                            {
                                if (args.Length <= i + 1)
                                {
                                    Console.WriteLine("Expected src path arg");
                                }
                                else
                                {
                                    string srcPath = args[i + 1];
                                    i++;

                                    Action<FileInfo, DirectoryInfo> copyFile = (FileInfo srcFile, DirectoryInfo dstDir) =>
                                    {
                                        switch (srcFile.Extension.ToLowerInvariant())
                                        {
                                            case ".json":
                                            case ".ydk":
                                                try
                                                {
                                                    DeckInfo deck = new DeckInfo();
                                                    deck.File = srcFile.FullName;
                                                    deck.Load();
                                                    deck.File = Path.Combine(dstDir.FullName, Path.ChangeExtension(srcFile.Name,
                                                        deck.IsYdkDeck ? ".json" : ".ydk"));
                                                    deck.Save();
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine("Error on '" + srcFile.FullName + "' " + e);
                                                }
                                                break;
                                            default:
                                                Console.WriteLine("Skip '" + srcFile.FullName + "'");
                                                break;
                                        }
                                    };
                                    if (Directory.Exists(srcPath))
                                    {
                                        if (args.Length <= i + 1)
                                        {
                                            Console.WriteLine("Expected dst path arg");
                                        }
                                        else
                                        {
                                            string dstPath = args[i + 1];
                                            i++;
                                            Action<DirectoryInfo, DirectoryInfo> copyDir = null;
                                            copyDir = (DirectoryInfo src, DirectoryInfo dst) =>
                                            {
                                                if (!dst.Exists)
                                                {
                                                    dst.Create();
                                                }
                                                foreach (DirectoryInfo subDir in src.GetDirectories())
                                                {
                                                    copyDir(subDir, new DirectoryInfo(Path.Combine(dst.FullName, subDir.Name)));
                                                }
                                                foreach (FileInfo file in src.GetFiles())
                                                {
                                                    copyFile(file, dst);
                                                }
                                            };
                                            copyDir(new DirectoryInfo(srcPath), new DirectoryInfo(dstPath));
                                        }
                                    }
                                    else if (File.Exists(srcPath))
                                    {
                                        copyFile(new FileInfo(srcPath), new DirectoryInfo(Path.GetDirectoryName(srcPath)));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Src path not found");
                                    }
                                }
                            }
                            break;
                        case "--ydlz":
                            {
                                string dir = args[++i];
                                if (Directory.Exists(dir))
                                {
                                    Func<byte[], byte[]> decompress = (byte[] buffer) =>
                                        {
                                            Console.WriteLine(BitConverter.ToString(buffer));
                                            using (BinaryWriter writer = new BinaryWriter(new MemoryStream()))
                                            using (MemoryStream ms = new MemoryStream(buffer))
                                            {
                                                System.IO.Compression.DeflateStream zip = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress);
                                                int totalRead = 0;
                                                int read = 0;
                                                byte[] temp = new byte[65535];
                                                while ((read = zip.Read(temp, 0, 1000)) > 0)
                                                {
                                                    totalRead += read;
                                                    writer.Write(temp, 0, read);
                                                }
                                                return ms.ToArray();
                                            }
                                        };

                                    string outDir = "ydlz-out";
                                    Directory.CreateDirectory(outDir);

                                    foreach (string path in Directory.GetFiles(dir))
                                    {
                                        using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
                                        {
                                            if (new string(br.ReadChars(4)) == "YDLZ")
                                            {
                                                br.BaseStream.Position += 4;
                                                br.BaseStream.Position += 2;// Also skip zlib header
                                                byte[] data = decompress(br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position)));
                                                File.WriteAllBytes(Path.Combine(outDir, Path.GetFileName(path)), data);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "--get-cids":// Gets a list of card ids for a given deck filename
                            {
                                DeckInfo deck = new DeckInfo();
                                deck.File = args[++i];
                                deck.Load();
                                File.WriteAllText("cids.txt", string.Join(",", deck.GetAllCards()));
                            }
                            break;
                        case "--missing-solo":// Logs which chapters are missing from /SoloDuels/
                            if (SoloData != null)
                            {
                                List<int> chapterIds = new List<int>();
                                Dictionary<string, object> allChapterData = Utils.GetDictionary(SoloData, "chapter");
                                Dictionary<string, object> allGateData = Utils.GetDictionary(SoloData, "gate");
                                if (allChapterData != null && allGateData != null)
                                {
                                    foreach (KeyValuePair<string, object> gateChapterData in allChapterData)
                                    {
                                        Dictionary<string, object> chapters = gateChapterData.Value as Dictionary<string, object>;
                                        int gateId;
                                        if (!int.TryParse(gateChapterData.Key, out gateId) || chapters == null)
                                        {
                                            continue;
                                        }
                                        foreach (KeyValuePair<string, object> chapter in chapters)
                                        {
                                            Dictionary<string, object> chapterData = chapter.Value as Dictionary<string, object>;
                                            int chapterId;
                                            if (!int.TryParse(chapter.Key, out chapterId) || chapterData == null)
                                            {
                                                continue;
                                            }

                                            Dictionary<string, object> gateData = Utils.GetDictionary(allGateData, gateId.ToString());
                                            bool isGoal = Utils.GetValue<int>(gateData, "clear_chapter") == chapterId;

                                            bool isScenario = Utils.IsScenarioChapter(Utils.GetValue<string>(chapterData, "begin_sn"));
                                            bool isLock = Utils.GetValue<int>(chapterData, "unlock_id") != 0;

                                            int myDeckSetId = Utils.GetValue<int>(chapterData, "mydeck_set_id");
                                            int loanerDeckSetId = Utils.GetValue<int>(chapterData, "set_id");
                                            if (!isGoal && !isScenario && !isLock && (myDeckSetId != 0 || loanerDeckSetId != 0))
                                            {
                                                chapterIds.Add(chapterId);
                                            }
                                        }
                                    }
                                }
                                List<int> chaptersOnDisk = new List<int>();
                                foreach (string file in Directory.GetFiles(Path.Combine(dataDirectory, "SoloDuels")))
                                {
                                    int chapterId;
                                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out chapterId))
                                    {
                                        chaptersOnDisk.Add(chapterId);
                                    }
                                }
                                Console.WriteLine("Missing chapters: " + string.Join(",", chapterIds.Except(chaptersOnDisk)));
                            }
                            break;
                        case "--missing-deckids":// Logs which solo chapters are missing the npc deck id values
                            {
                                HashSet<int> chapterIds = new HashSet<int>();
                                HashSet<int> chaptersWithDeckId = new HashSet<int>();
                                foreach (string file in Directory.GetFiles(Path.Combine(dataDirectory, "SoloDuels"), "*.json"))
                                {
                                    int chapterId;
                                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out chapterId))
                                    {
                                        Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                                        data = Utils.GetResData(data);
                                        Dictionary<string, object> duelData;
                                        if (!Utils.TryGetValue(data, "Duel", out duelData))
                                        {
                                            continue;
                                        }
                                        int chapterId2;
                                        if (!Utils.TryGetValue(duelData, "chapter", out chapterId2))
                                        {
                                            continue;
                                        }
                                        chapterIds.Add(chapterId2);
                                        DuelSettings duel = new DuelSettings();
                                        duel.FromDictionary(duelData);
                                        if (duel.npc_deck_id != 0)
                                        {
                                            chaptersWithDeckId.Add(chapterId2);
                                        }
                                    }
                                }
                                foreach (string file in Directory.GetFiles(Path.Combine(dataDirectory, "SoloNpcDeckIds"), "*.txt"))
                                {
                                    int chapterId;
                                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out chapterId))
                                    {
                                        int npcDeckId;
                                        if (int.TryParse(File.ReadAllText(file).Trim(), out npcDeckId))
                                        {
                                            chaptersWithDeckId.Add(chapterId);
                                        }
                                    }
                                }
                                Console.WriteLine("Missing npc deck ids:");
                                foreach (int chapterId in chapterIds)
                                {
                                    if (!chaptersWithDeckId.Contains(chapterId))
                                    {
                                        Console.WriteLine("Missing deck id for chapter " + chapterId);
                                    }
                                }
                            }
                            break;
                        case "--merge-deckids":
                            {
                                Dictionary<int, int> chaptersDeckIds = new Dictionary<int, int>();
                                string jsonFile = Path.Combine(Path.Combine(dataDirectory, "SoloNpcDeckIds.json"));
                                if (File.Exists(jsonFile))
                                {
                                    Dictionary<string, object> chapterDatas = MiniJSON.Json.DeserializeStripped(File.ReadAllText(jsonFile)) as Dictionary<string, object>;
                                    if (chapterDatas != null)
                                    {
                                        foreach (KeyValuePair<string, object> chapter in chapterDatas)
                                        {
                                            int deckId = (int)Convert.ChangeType(chapter.Value, typeof(int));
                                            int chapterId;
                                            if (int.TryParse(chapter.Key, out chapterId))
                                            {
                                                chaptersDeckIds[chapterId] = deckId;
                                            }
                                        }
                                    }
                                }
                                foreach (string file in Directory.GetFiles(Path.Combine(dataDirectory, "SoloNpcDeckIds"), "*.txt"))
                                {
                                    int chapterId;
                                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out chapterId))
                                    {
                                        int npcDeckId;
                                        if (int.TryParse(File.ReadAllText(file).Trim(), out npcDeckId))
                                        {
                                            chaptersDeckIds[chapterId] = npcDeckId;
                                        }
                                    }
                                }
                                File.WriteAllText(jsonFile, MiniJSON.Json.Format(MiniJSON.Json.Serialize(chaptersDeckIds)));
                            }
                            break;
                        case "--unknown-alt-cards":// Lists unknown alt card ids (excludes everything already in AltCardsYdk.json)
                            {
                                YdkHelper.LoadIdMap(dataDirectory);

                                HashSet<int> knownAltCardIds = new HashSet<int>();
                                string altCardIdsFile = Path.Combine("..", "Docs", "AltCardsYdk.json");
                                if (File.Exists(altCardIdsFile))
                                {
                                    try
                                    {
                                        Dictionary<string, object> jsonData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(altCardIdsFile)) as Dictionary<string, object>;
                                        foreach (KeyValuePair<string, object> entry in jsonData)
                                        {
                                            int ydkId;
                                            if (int.TryParse(entry.Key, out ydkId))
                                            {
                                                int cardId = (int)Convert.ChangeType(entry.Value, typeof(int));
                                                knownAltCardIds.Add(cardId);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }

                                Dictionary<string, List<int>> cardNameIds = new Dictionary<string, List<int>>();
                                Dictionary<int, YdkHelper.GameCardInfo> cards = YdkHelper.LoadCardDataFromGame(dataDirectory);
                                foreach (YdkHelper.GameCardInfo card in cards.Values)
                                {
                                    if (card.Kind == CardKind.Token || card.Kind == CardKind.TokenTuner)
                                    {
                                        continue;
                                    }
                                    if (card.Id >= 30000 && card.Id <= 30099)
                                    {
                                        // Promotional card ids
                                        continue;
                                    }
                                    if (!cardNameIds.ContainsKey(card.Name))
                                    {
                                        cardNameIds[card.Name] = new List<int>();
                                    }
                                    cardNameIds[card.Name].Add(card.Id);
                                }

                                foreach (KeyValuePair<string, List<int>> card in cardNameIds)
                                {
                                    List<int> altIds = new List<int>();
                                    foreach (int cardId in card.Value)
                                    {
                                        if (YdkHelper.GetYdkId(cardId) == 0)
                                        {
                                            altIds.Add(cardId);
                                        }
                                    }
                                    List<int> unknownAltIds = altIds.Except(knownAltCardIds).ToList();
                                    if (unknownAltIds.Count > 0)
                                    {
                                        Console.WriteLine(card.Key + " = " + string.Join(",", card.Value) + " (unknown:" + string.Join(",", unknownAltIds) + ")");
                                    }
                                }
                            }
                            break;
                        default:
                            log = false;
                            break;
                    }
                    if (log)
                    {
                        ranCommand = true;
                        Console.WriteLine("Done (" + arg + ")");
                    }
                }
                if (ranCommand)
                {
                    Environment.Exit(0);
                }
            }
        }

        void InitDecksWatcher()
        {
            if (MultiplayerEnabled)
            {
                return;
            }
            object updateDecksLocker = new object();
            Action<string, string> updateDecks = (string srcPath, string dstPath) =>
            {
                lock (updateDecksLocker)
                {
                    if (localPlayer != null)
                    {
                        Dictionary<string, DeckInfo> fullPathDecks = new Dictionary<string, DeckInfo>();
                        foreach (DeckInfo deckInfo in localPlayer.Decks.Values)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(deckInfo.File))
                                {
                                    fullPathDecks[Path.GetFullPath(deckInfo.File).ToLowerInvariant()] = deckInfo;
                                }
                            }
                            catch
                            {
                            }
                        }
                        try
                        {
                            if (srcPath != dstPath)
                            {
                                if (File.Exists(dstPath))
                                {
                                    //Console.WriteLine("Deck renamed");
                                    DeckInfo deck;
                                    if (fullPathDecks.TryGetValue(srcPath.ToLowerInvariant(), out deck))
                                    {
                                        deck.File = dstPath;
                                    }
                                    else
                                    {
                                        LoadDeck(localPlayer, dstPath);
                                    }
                                }
                            }
                            else if (File.Exists(srcPath))
                            {
                                DeckInfo deck;
                                if (!fullPathDecks.TryGetValue(srcPath.ToLowerInvariant(), out deck))
                                {
                                    //Console.WriteLine("Deck added");
                                    LoadDeck(localPlayer, srcPath);
                                }
                            }
                            else
                            {
                                DeckInfo deck;
                                if (fullPathDecks.TryGetValue(srcPath.ToLowerInvariant(), out deck))
                                {
                                    //Console.WriteLine("Deck removed");
                                    localPlayer.Decks.Remove(deck.Id);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            };
            localPlayerDecksFileWatcher = new FileSystemWatcher(GetDecksDirectory(null));
            localPlayerDecksFileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            FileSystemEventHandler decksFileWatcherUpdateEvent = (object sender, FileSystemEventArgs e) =>
                {
                    string fullPath = Path.GetFullPath(e.FullPath);
                    updateDecks(fullPath, fullPath);
                };
            localPlayerDecksFileWatcher.Created += decksFileWatcherUpdateEvent;
            localPlayerDecksFileWatcher.Deleted += decksFileWatcherUpdateEvent;
            localPlayerDecksFileWatcher.Renamed += (object sender, RenamedEventArgs e) =>
                {
                    string oldFullPath = Path.GetFullPath(e.OldFullPath);
                    string newFullPath = Path.GetFullPath(e.FullPath);
                    updateDecks(oldFullPath, newFullPath);
                };
            localPlayerDecksFileWatcher.EnableRaisingEvents = true;
        }

        List<int> GetCraftableCards(Player player)
        {
            if (!ProgressiveCardList || CardCraftableAll)
            {
                return CardCraftable;
            }
            HashSet<int> result = new HashSet<int>();
            foreach (ShopItemInfo shopItem in Shop.PackShop.Values)
            {
                if (player.ShopState.GetAvailability(Shop, shopItem) == PlayerShopItemAvailability.Hidden)
                {
                    continue;
                }
                foreach (KeyValuePair<int, CardRarity> card in shopItem.Cards)
                {
                    result.Add(card.Key);
                }
            }
            return result.ToList();
        }

        /// <summary>
        /// Returns all card rarities which should be visible to the game
        /// - When using a progressive card list this will expand over time
        /// - When using progressive card rarities the rarity of any given card can change over time
        /// - Also used to get the card rarities for a specific pack in a shop (which can use specific rarities for that pack)
        /// </summary>
        Dictionary<int, int> GetCardRarities(Player player, ShopItemInfo targetShopItem = null)
        {
            // NOTE: This function is probably a little heavy handed, only call once per packet
            if (targetShopItem != null)
            {
                Dictionary<int, int> packCardRare = new Dictionary<int, int>();
                foreach (KeyValuePair<int, CardRarity> card in targetShopItem.Cards)
                {
                    CardRarity rarity = card.Value;
                    if (rarity == CardRarity.None || !Shop.PerPackRarities)
                    {
                        TryGetCardRarity(card.Key, CardRare, out rarity);
                    }
                    if (rarity == CardRarity.None)
                    {
                        continue;
                    }
                    packCardRare[card.Key] = (int)rarity;
                }
                return packCardRare;
            }
            if ((!ProgressiveCardList && !ProgressiveCardRarities) ||
                player.ShopState.GetAvailability(Shop, Shop.StandardPack) == PlayerShopItemAvailability.Available)
            {
                return CardRare;
            }
            Dictionary<int, int> result = ProgressiveCardList ? new Dictionary<int, int>() : new Dictionary<int, int>(CardRare);
            Dictionary<int, int> lowestPackRarities = ProgressiveCardRarities ? new Dictionary<int, int>() : null;
            foreach (ShopItemInfo shopItem in Shop.PackShop.Values)
            {
                if (ProgressiveCardList && player.ShopState.GetAvailability(Shop, shopItem) == PlayerShopItemAvailability.Hidden)
                {
                    continue;
                }
                foreach (KeyValuePair<int, CardRarity> card in shopItem.Cards)
                {
                    if (ProgressiveCardRarities)
                    {
                        CardRarity rarity;
                        if (!TryGetCardRarity(card.Key, lowestPackRarities, out rarity) || card.Value < rarity)
                        {
                            lowestPackRarities[card.Key] = (int)card.Value;
                        }
                    }
                    if (ProgressiveCardList)
                    {
                        CardRarity rarity;
                        if (!TryGetCardRarity(card.Key, result, out rarity) &&
                            TryGetCardRarity(card.Key, CardRare, out rarity))
                        {
                            result[card.Key] = (int)rarity;
                        }
                    }
                }
            }
            if (ProgressiveCardRarities)
            {
                foreach (KeyValuePair<int, int> card in new Dictionary<int, int>(result))
                {
                    CardRarity rarity;
                    if (TryGetCardRarity(card.Key, lowestPackRarities, out rarity) && rarity > (CardRarity)card.Value)
                    {
                        result[card.Key] = (int)rarity;
                    }
                }
            }
            // Add any cards owned by the player but are missing from the result (otherwise the client will have issues with those cards)
            foreach (int cardId in player.Cards.GetIDs())
            {
                CardRarity rarity;
                if (!result.ContainsKey(cardId) && TryGetCardRarity(cardId, CardRare, out rarity))
                {
                    result[cardId] = (int)rarity;
                }
            }
            return result;
        }

        bool TryGetCardRarity(int cardId, Dictionary<int, int> rarities, out CardRarity result)
        {
            result = CardRarity.None;
            int rarity;
            if (rarities.TryGetValue(cardId, out rarity))
            {
                result = (CardRarity)rarity;
                return true;
            }
            return false;
        }

        void SavePlayer(Player player)
        {
            // To void saving multiple times per packet save after the request has been processed
            player.RequiresSaving = true;
        }

        public void SavePlayerNow(Player player)
        {
            lock (player)
            {
                //LogInfo("Save (player)");
                Dictionary<string, object> data = new Dictionary<string, object>();
                if (MultiplayerEnabled)
                {
                    data["Token"] = player.Token;
                }
                data["Code"] = player.Code;
                data["Name"] = player.Name;
                data["Rank"] = player.Rank;
                data["Rate"] = player.Rate;
                data["Level"] = player.Level;
                data["Exp"] = player.Exp;
                data["Gems"] = player.Gems;
                data["IconId"] = player.IconId;
                data["IconFrameId"] = player.IconFrameId;
                data["AvatarId"] = player.AvatarId;
                data["Wallpaper"] = player.Wallpaper;
                data["WallpaperHome"] = player.WallpaperHome.ToArray();
                data["DuelBgmMode"] = (int)player.DuelBgmMode;
                data["CraftPoints"] = player.CraftPoints.ToDictionary();
                data["OrbPoints"] = player.OrbPoints.ToDictionary();
                data["SelectedDeck"] = player.Duel.SelectedDeckToDictionary();
                data["TitleTags"] = player.TitleTags.ToArray();
                data["CardFavorites"] = player.CardFavorites.ToDictionary();
                data["CardLock"] = player.CardLock.ToDictionary();
                data["Items"] = player.Items.ToArray();
                data["SoloLastPlayedChaterId"] = player.SoloLastPlayedChaterId;
                data["SoloChapters"] = player.SoloChaptersToDictionary();
                data["ShopState"] = player.ShopState.ToDictionary();
                data["Cards"] = player.Cards.ToDictionary();
                if (MultiplayerEnabled)
                {
                    data["Friends"] = GetFriends(player);
                }
                string jsonFormatted = MiniJSON.Json.Format(MiniJSON.Json.Serialize(data));
                string dir = GetPlayerDirectory(player);
                Utils.TryCreateDirectory(dir);
                try
                {
                    File.WriteAllText(Path.Combine(dir, "Player.json"), jsonFormatted);
                }
                catch
                {
                }
                player.RequiresSaving = false;
            }
        }

        string GetLocalPlayerSaveDataDir()
        {
            return Path.Combine(playersDirectory, "Local");
        }

        string GetPlayerDirectory(Player player)
        {
            if (MultiplayerEnabled)
            {
                return Path.Combine(playersDirectory, player.Code.ToString());
            }
            else
            {
                return GetLocalPlayerSaveDataDir();
            }
        }

        string GetDecksDirectory(Player player)
        {
            return Path.Combine(GetPlayerDirectory(player), "Decks");
        }

        string GetReplaysDirectory(Player player)
        {
            return Path.Combine(GetPlayerDirectory(player), "Replays");
        }

        void LoadPlayer(Player player)
        {
            Dictionary<string, object> data = null;
            string path = Path.Combine(GetPlayerDirectory(player), "Player.json");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(path)) as Dictionary<string, object>;
            }
            bool isNewPlayer = data == null;
            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            uint code;
            if (Utils.TryGetValue(data, "Code", out code) && code != 0)
            {
                player.Code = code;
            }
            player.Token = Utils.GetValue<string>(data, "Token");
            player.Name = Utils.GetValue<string>(data, "Name", DuelSettings.DefaultNamePlayer);
            player.Rank = Utils.GetValue<int>(data, "Rank", (int)StandardRank.ROOKIE);
            player.Rate = Utils.GetValue<int>(data, "Rate", (StandardRank)player.Rank == StandardRank.ROOKIE ? 2 : 5);
            player.Level = Utils.GetValue<int>(data, "Level", 1);
            player.Exp = Utils.GetValue<long>(data, "Exp");
            player.Gems = Utils.GetValue<int>(data, "Gems", DefaultGems);
            player.IconId = Utils.GetValue<int>(data, "IconId");
            player.IconFrameId = Utils.GetValue<int>(data, "IconFrameId");
            player.AvatarId = Utils.GetValue<int>(data, "AvatarId");
            player.Wallpaper = Utils.GetValue<int>(data, "Wallpaper");
            Utils.GetIntHashSet(data, "WallpaperHome", player.WallpaperHome, ignoreZero: true);
            player.DuelBgmMode = (DuelBgmMode)Utils.GetValue<int>(data, "DuelBgmMode");

            player.SoloLastPlayedChaterId = Utils.GetValue<int>(data, "SoloLastPlayedChaterId");
            player.SoloChaptersFromDictionary(Utils.GetDictionary(data, "SoloChapters"));
            if (UnlockAllSoloChapters)
            {
                foreach (int chapterId in GetAllSoloChapterIds())
                {
                    player.SoloChapters[chapterId] = ChapterStatus.COMPLETE;
                }
            }
            player.CraftPoints.FromDictionary(Utils.GetDictionary(data, "CraftPoints"));
            if (isNewPlayer && DefaultCraftPoints != null)
            {
                foreach (KeyValuePair<string, object> entry in DefaultCraftPoints)
                {
                    int itemId;
                    if (int.TryParse(entry.Key, out itemId) && itemId >= (int)ItemID.Value.CpN && itemId <= (int)ItemID.Value.CpUR)
                    {
                        player.AddItem(itemId, Convert.ToInt32(entry.Value));
                    }
                }
            }
            player.OrbPoints.FromDictionary(Utils.GetDictionary(data, "OrbPoints"));
            player.ShopState.FromDictionary(Utils.GetDictionary(data, "ShopState"));
            player.CardFavorites.FromDictionary(Utils.GetDictionary(data, "CardFavorites"));
            player.CardLock.FromDictionary(Utils.GetDictionary(data, "CardLock"));
            List<object> titleTags;
            if (Utils.TryGetValue(data, "TitleTags", out titleTags))
            {
                foreach (object tag in titleTags)
                {
                    player.TitleTags.Add((int)Convert.ChangeType(tag, typeof(int)));
                }
            }
            Utils.GetIntHashSet(data, "Items", player.Items, ignoreZero: true);
            foreach (int item in DefaultItems)
            {
                if (item != 0)
                {
                    player.Items.Add(item);
                    switch (ItemID.GetCategoryFromID(item))
                    {
                        case ItemID.Category.ICON: if (!player.Items.Contains(player.IconId)) player.IconId = item; break;
                        case ItemID.Category.ICON_FRAME: if (!player.Items.Contains(player.IconFrameId)) player.IconFrameId = item; break;
                        case ItemID.Category.AVATAR: if (!player.Items.Contains(player.AvatarId)) player.AvatarId = item; break;
                        case ItemID.Category.WALLPAPER: if (!player.Items.Contains(player.Wallpaper)) player.Wallpaper = item; break;
                        case ItemID.Category.FIELD:
                            foreach (int fieldPartItemId in ItemID.GetDuelFieldParts(item))
                            {
                                player.Items.Add(fieldPartItemId);
                            }
                            break;
                    }
                }
            }
            if (UnlockAllItems)
            {
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
                    foreach (int value in ItemID.Values[category])
                    {
                        if (player.Items.Add(value))
                        {
                            player.RequiresSaving = true;
                        }
                    }
                }
            }
            Dictionary<string, object> cards = Utils.GetDictionary(data, "Cards");
            if (cards != null)
            {
                player.Cards.FromDictionary(cards);
            }
            if (UnlockAllCards)
            {
                foreach (int cardId in CardRare.Keys)
                {
                    int count = player.Cards.GetCount(cardId, PlayerCardKind.Dismantle, CardStyleRarity.Normal);
                    if (count < 3)
                    {
                        player.Cards.SetCount(cardId, 3, PlayerCardKind.Dismantle, CardStyleRarity.Normal);
                    }
                }
            }
            if (UnlockAllCardsHighestRarity)
            {
                foreach (KeyValuePair<int, int> card in CardRare)
                {
                    CardStyleRarity style = CardStyleRarity.Normal;
                    switch ((CardRarity)card.Value)
                    {
                        case CardRarity.Normal:
                        case CardRarity.Rare:
                            style = CardStyleRarity.Shine;
                            break;
                        case CardRarity.SuperRare:
                        case CardRarity.UltraRare:
                            style = CardStyleRarity.Royal;
                            break;
                    }
                    int count = player.Cards.GetCount(card.Key, PlayerCardKind.Dismantle, style);
                    if (count < 3)
                    {
                        player.Cards.SetCount(card.Key, 3, PlayerCardKind.Dismantle, style);
                    }
                }
            }
            if (UnlockAllCardsShine)
            {
                foreach (int cardId in CardRare.Keys)
                {
                    int count = player.Cards.GetCount(cardId, PlayerCardKind.Dismantle, CardStyleRarity.Shine);
                    if (count < 3)
                    {
                        player.Cards.SetCount(cardId, 3, PlayerCardKind.Dismantle, CardStyleRarity.Shine);
                    }
                }
            }
            if (player.Cards.Count == 0)
            {
                // Only look for default starting decks if the player has no cards
                foreach (int itemId in player.Items)
                {
                    if (ItemID.GetCategoryFromID(itemId) == ItemID.Category.STRUCTURE)
                    {
                        DeckInfo deck;
                        if (StructureDecks.TryGetValue(itemId, out deck))
                        {
                            foreach (int cardId in deck.GetAllCards())
                            {
                                player.Cards.Add(cardId, 1, DisableNoDismantle ? PlayerCardKind.Dismantle : PlayerCardKind.NoDismantle, CardStyleRarity.Normal);
                            }
                        }
                    }
                }
            }
            string decksDir = GetDecksDirectory(player);
            if (Directory.Exists(decksDir))
            {
                foreach (string file in Directory.GetFiles(decksDir))
                {
                    LoadDeck(player, file);
                }
            }
            SortDecks(player);
            player.Duel.SelectedDeckFromDictionary(Utils.GetDictionary(data, "SelectedDeck"));

            if (MultiplayerEnabled)
            {
                lock (player.Friends)
                {
                    Dictionary<string, object> friends = Utils.GetDictionary(data, "Friends");
                    if (friends != null)
                    {
                        player.Friends.Clear();
                        foreach (KeyValuePair<string, object> friend in friends)
                        {
                            FriendState friendState = FriendState.None;
                            if (friend.Value is string)
                            {
                                friendState = (FriendState)Enum.Parse(typeof(FriendState), friend.Value as string, false);
                            }
                            else
                            {
                                friendState = (FriendState)Convert.ChangeType(friend.Value, typeof(FriendState).GetEnumUnderlyingType());
                            }
                            uint friendId;
                            if (friendState != FriendState.None && uint.TryParse(friend.Key, out friendId))
                            {
                                player.Friends[friendId] = friendState;
                            }
                        }
                    }
                }
            }
        }

        void SaveDeck(DeckInfo deck)
        {
            deck.Save();
        }

        void LoadDeck(Player player, string file)
        {
            if (file.EndsWith(".json") || file.EndsWith(".ydk"))
            {
                try
                {
                    DeckInfo deck = new DeckInfo();
                    deck.File = file;
                    deck.Load();
                    while ((deck.Id = player.NextDeckUId++) == DeckInfo.TradeDeckId)
                    {
                    }
                    player.Decks[deck.Id] = deck;
                }
                catch
                {
                    Utils.LogWarning("Failed to load deck " + file);
                }
            }
        }

        void DeleteDeck(DeckInfo deck)
        {
            try
            {
                if (!string.IsNullOrEmpty(deck.File) && File.Exists(deck.File))
                {
                    File.Delete(deck.File);
                }
            }
            catch
            {
            }
        }

        void SortDecks(Player player, GameServerWebRequest request = null)
        {
            bool sorted = false;
            if (DeckListAlphabetical)
            {
                sorted = true;
                int et = 1;
                foreach (DeckInfo deck in player.Decks.Values.OrderByDescending(x => x.Name))
                {
                    deck.TimeEdited = et++;
                }
            }
            if (DeckListByBoxThenAlphabetical)
            {
                sorted = true;
                int et = 1;
                foreach (DeckInfo deck in player.Decks.Values.OrderByDescending(x => x.Accessory.Box).ThenByDescending(x => x.Name))
                {
                    deck.TimeEdited = et++;
                }
            }
            if (request != null && sorted)
            {
                Dictionary<string, object> deck = request.GetOrCreateDictionary("Deck");
                Dictionary<string, object> list = Utils.GetOrCreateDictionary(deck, "list");
                foreach (DeckInfo deckInfo in request.Player.Decks.Values)
                {
                    list[deckInfo.Id.ToString()] = new Dictionary<string, object>()
                    {
                        { "et", deckInfo.TimeEdited },
                    };
                }
            }
        }

        void LoadStructureDecks()
        {
            StructureDecks = new Dictionary<int, DeckInfo>();
            string dir = Path.Combine(dataDirectory, "StructureDecks");
            if (!Directory.Exists(dir))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(dir, "*.json"))
            {
                Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                if (data != null)
                {
                    DeckInfo deck = new DeckInfo();
                    deck.Id = Utils.GetValue<int>(data, "structure_id");
                    if (deck.Id == 0)
                    {
                        int.TryParse(Path.GetFileNameWithoutExtension(file), out deck.Id);
                    }
                    deck.Accessory.FromDictionary(Utils.GetDictionary(data, "accessory"));
                    deck.DisplayCards.FromDictionary(Utils.GetDictionary(data, "focus"));
                    deck.FromDictionary(Utils.GetDictionary(data, "contents"));
                    StructureDecks[deck.Id] = deck;
                }
            }

            // NOTE: Hacky way to implement "Deck.copy_structure_multi" as otherwise we don't have access to the deck name.
            // for languages other than English this will mean deck names are copied wrong (will always be english)
            foreach (string line in File.ReadAllLines(Path.Combine(dataDirectory, "ItemID.json")))
            {
                foreach (KeyValuePair<int, DeckInfo> deck in StructureDecks)
                {
                    if (line.Contains(deck.Key.ToString()))
                    {
                        int indx = line.IndexOf("//");
                        if (indx > 0)
                        {
                            deck.Value.Name = line.Substring(indx + 2).Trim();
                        }
                    }
                }
            }
        }

        void LoadCardCategory()
        {
            // This data can be extracted from "External/CardCategory/CardCategory" (assets/resourcesassetbundle/external/cardcategory/cardcategory.bytes)
            CardCategories = new Dictionary<int, CardCategory>();
            CardCategoriesByName = new Dictionary<string, CardCategory>();
            string file = Path.Combine(dataDirectory, "CardCategory.bytes");
            if (!File.Exists(file))
            {
                return;
            }
            List<object> data = MessagePack.Unpack(LZ4.Decompress(File.ReadAllBytes(file))) as List<object>;
            foreach (object entry in data)
            {
                Dictionary<string, object> categoryData = entry as Dictionary<string, object>;
                if (categoryData != null)
                {
                    //id, createStart, createEnd, searchStart, searchEnd, nameJa, nameEn, nameIt, nameDe, nameEs, namePt, nameKo, sort
                    CardCategory category = new CardCategory();
                    category.Id = Utils.GetValue<int>(categoryData, "id");
                    category.Name = Utils.GetValue<string>(categoryData, "nameEn");
                    category.Sort = Utils.GetValue<int>(categoryData, "sort");// actually a string
                    CardCategories[category.Id] = category;
                    CardCategoriesByName[category.Name] = category;
                    //Debug.WriteLine(category.Id + " " + category.Name);
                }
            }
        }

        void LoadShop()
        {
            Shop = new ShopInfo();
            LoadShop(Path.Combine(dataDirectory, "Shop.json"), true);
            string dir = Path.Combine(dataDirectory, "Shop");
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir, "*.json"))
                {
                    LoadShop(file, false);
                }
            }
            Dictionary<int, ShopItemInfo>[] allShops = { Shop.PackShop, Shop.StructureShop, Shop.AccessoryShop, Shop.SpecialShop };
            foreach (Dictionary<int, ShopItemInfo> shopTab in allShops)
            {
                foreach (KeyValuePair<int, ShopItemInfo> shopItem in shopTab)
                {
                    if (Shop.AllShops.ContainsKey(shopItem.Key))
                    {
                        Utils.LogWarning("Duplicate shop id " + shopItem.Key);
                    }
                    else
                    {
                        Shop.AllShops[shopItem.Key] = shopItem.Value;
                        switch (shopItem.Value.Category)
                        {
                            case ShopCategory.Pack:
                                Shop.PacksByPackId[shopItem.Value.Id] = shopItem.Value;
                                break;
                        }
                        switch (shopItem.Value.PackType)
                        {
                            case ShopPackType.Secret:
                                if (shopItem.Value.SecretType == ShopItemSecretType.None)
                                {
                                    shopItem.Value.SecretType = ShopItemSecretType.FindOrCraft;
                                }
                                foreach (int cardId in shopItem.Value.Cards.Keys)
                                {
                                    if (!Shop.SecretPacksByCardId.ContainsKey(cardId))
                                    {
                                        Shop.SecretPacksByCardId[cardId] = new List<ShopItemInfo>();
                                    }
                                    Shop.SecretPacksByCardId[cardId].Add(shopItem.Value);
                                }
                                break;
                        }
                    }
                }
            }
            if (Shop.StandardPack == null)
            {
                foreach (ShopItemInfo item in Shop.AllShops.Values)
                {
                    if (item.PackType == ShopPackType.Standard)
                    {
                        Shop.StandardPack = item;
                        break;
                    }
                }
            }
            if (Shop.PutAllCardsInStandardPack && Shop.StandardPack != null)
            {
                Shop.StandardPack.Cards.Clear();
                foreach (int cardId in CardRare.Keys.OrderBy(x => x))
                {
                    Shop.StandardPack.Cards[cardId] = CardRarity.None;
                }
            }

            foreach (ShopItemInfo shopItem in Shop.PackShop.Values)
            {
                switch (shopItem.Category)
                {
                    case ShopCategory.Pack:
                        {
                            if (string.IsNullOrEmpty(shopItem.PackImageName) &&
                                !Shop.PackShopImagesByCardId.TryGetValue(shopItem.IconMrk, out shopItem.PackImageName) &&
                                !Shop.PackShopImagesByCardId.TryGetValue(0, out shopItem.PackImageName))
                            {
                                shopItem.PackImageName = "CardPackTex01_0000";
                            }
                            if (Shop.OverridePackPrice > 0)
                            {
                                ShopItemPrice price = shopItem.Prices.FirstOrDefault(x => x.ItemAmount == 1);
                                if (price == null)
                                {
                                    // Update the price list if there's an existing price with an item amount of more than 1
                                    if (shopItem.Prices.Count != 0)
                                    {
                                        foreach (ShopItemPrice existingPrice in shopItem.Prices)
                                        {
                                            existingPrice.Id++;
                                        }
                                    }
                                    price = new ShopItemPrice();
                                    price.Id = 1;
                                    shopItem.Prices.Insert(0, price);
                                }
                                price.ItemAmount = 1;
                                price.Price = Shop.OverridePackPrice;
                            }
                            if (Shop.OverridePackPriceX10 > 0)
                            {
                                ShopItemPrice price = shopItem.Prices.FirstOrDefault(x => x.ItemAmount > 1);
                                if (price == null)
                                {
                                    price = new ShopItemPrice();
                                    price.Id = shopItem.Prices.Count + 1;
                                    shopItem.Prices.Add(price);
                                }
                                price.ItemAmount = 10;
                                price.Price = Shop.OverridePackPriceX10;
                            }
                            if (shopItem.Prices.Count == 0)
                            {
                                if (Shop.DefaultPackPrice > 0)
                                {
                                    ShopItemPrice price = new ShopItemPrice();
                                    price.Id = shopItem.Prices.Count + 1;
                                    price.ItemAmount = 1;
                                    price.Price = Shop.DefaultPackPrice;
                                    shopItem.Prices.Add(price);
                                }
                                if (Shop.DefaultPackPriceX10 > 0)
                                {
                                    ShopItemPrice price = new ShopItemPrice();
                                    price.Id = shopItem.Prices.Count + 1;
                                    price.ItemAmount = 10;
                                    price.Price = Shop.DefaultPackPriceX10;
                                    shopItem.Prices.Add(price);
                                }
                            }
                            if (shopItem.UnlockSecrets.Count > 0)
                            {
                                if (Shop.OverrideUnlockSecretsAtPercent > 0)
                                {
                                    shopItem.UnlockSecretsAtPercent = Shop.OverrideUnlockSecretsAtPercent;
                                }
                                else if (shopItem.UnlockSecretsAtPercent == 0)
                                {
                                    shopItem.UnlockSecretsAtPercent = Shop.DefaultUnlockSecretsAtPercent;
                                }

                                if (Shop.OverrideUnlockSecretsAtNumDuels > 0)
                                {
                                    shopItem.UnlockSecretsAtNumDuels = Shop.OverrideUnlockSecretsAtNumDuels;
                                }
                                else if (shopItem.UnlockSecretsAtNumDuels == 0)
                                {
                                    shopItem.UnlockSecretsAtNumDuels = Shop.DefaultUnlockSecretsAtNumDuels;
                                }
                            }
                            switch (shopItem.SecretType)
                            {
                                case ShopItemSecretType.None:
                                case ShopItemSecretType.Other:// These aren't given a duration unless explicitly defined
                                    shopItem.SecretDurationInSeconds = 0;
                                    break;
                                default:
                                    if (Shop.OverrideSecretDuration > 0)
                                    {
                                        shopItem.SecretDurationInSeconds = Shop.OverrideSecretDuration;
                                    }
                                    else if (Shop.DefaultSecretDuration > 0 && shopItem.SecretDurationInSeconds == 0)
                                    {
                                        shopItem.SecretDurationInSeconds = Shop.DefaultSecretDuration;
                                    }
                                    else if (shopItem.SecretDurationInSeconds < 0)
                                    {
                                        shopItem.SecretDurationInSeconds = 0;
                                    }
                                    break;
                            }
                            if (Shop.OverridePackCardNum > 0)
                            {
                                shopItem.CardNum = Shop.OverridePackCardNum;
                            }
                            else if (Shop.DefaultPackCardNum > 0 && shopItem.CardNum == 0)
                            {
                                shopItem.CardNum = Shop.DefaultPackCardNum;
                            }
                        }
                        break;
                    case ShopCategory.Structure:
                        {
                            if (Shop.OverrideStructureDeckPrice > 0 || (shopItem.Prices.Count == 0 && Shop.DefaultStructureDeckPrice > 0))
                            {
                                shopItem.Prices.Clear();
                                ShopItemPrice price = new ShopItemPrice();
                                price.Id = 1;
                                price.ItemAmount = 1;
                                price.Price = Shop.OverrideStructureDeckPrice > 0 ? Shop.OverrideStructureDeckPrice : Shop.DefaultStructureDeckPrice;
                                shopItem.Prices.Add(price);
                            }
                        }
                        break;
                }
            }
            

            LoadShopPackOdds(Path.Combine(dataDirectory, "ShopPackOdds.json"));
            dir = Path.Combine(dataDirectory, "ShopPackOdds");
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles("*.json"))
                {
                    LoadShopPackOdds(file);
                }
            }
            LoadShopPackOddsVisuals(Path.Combine(dataDirectory, "ShopPackOddsVisuals.json"));// TODO: Maybe just merge into Shop.json
        }

        void LoadShop(string file, bool isMainShop)
        {
            if (!File.Exists(file))
            {
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            if (data == null)
            {
                return;
            }
            long secretPackDuration;
            if (!Utils.TryGetValue<long>(data, "SecretPackDuration", out secretPackDuration))
            {
                secretPackDuration = -1;// Special value to state that it's not defined (only checked in LoadShopItems)
            }
            if (isMainShop)
            {
                Shop.PutAllCardsInStandardPack = Utils.GetValue<bool>(data, "PutAllCardsInStandardPack");
                Shop.NoDuplicatesPerPack = Utils.GetValue<bool>(data, "NoDuplicatesPerPack");
                Shop.PerPackRarities = Utils.GetValue<bool>(data, "PerPackRarities");
                Shop.DisableCardStyleRarity = Utils.GetValue<bool>(data, "DisableCardStyleRarity");
                Shop.UnlockAllSecrets = Utils.GetValue<bool>(data, "UnlockAllSecrets");
                Shop.DisableUltraRareGuarantee = Utils.GetValue<bool>(data, "DisableUltraRareGuarantee");
                Shop.UpgradeRarityWhenNotFound = Utils.GetValue<bool>(data, "UpgradeRarityWhenNotFound");

                Shop.DefaultSecretDuration = Utils.GetValue<long>(data, "DefaultSecretDuration");
                Shop.OverrideSecretDuration = Utils.GetValue<long>(data, "OverrideSecretDuration");
                Shop.DefaultPackPrice = Utils.GetValue<int>(data, "DefaultPackPrice");
                Shop.DefaultPackPriceX10 = Utils.GetValue<int>(data, "DefaultPackPriceX10");
                Shop.OverridePackPrice = Utils.GetValue<int>(data, "OverridePackPrice");
                Shop.OverridePackPriceX10 = Utils.GetValue<int>(data, "OverridePackPriceX10");
                Shop.DefaultStructureDeckPrice = Utils.GetValue<int>(data, "DefaultStructureDeckPrice");
                Shop.OverrideStructureDeckPrice = Utils.GetValue<int>(data, "OverrideStructureDeckPrice");
                Shop.DefaultUnlockSecretsAtPercent = Utils.GetValue<int>(data, "DefaultUnlockSecretsAtPercent");
                Shop.OverrideUnlockSecretsAtPercent = Utils.GetValue<int>(data, "OverrideUnlockSecretsAtPercent");
                Shop.DefaultUnlockSecretsAtNumDuels = Utils.GetValue<int>(data, "DefaultUnlockSecretsAtNumDuels");
                Shop.OverrideUnlockSecretsAtNumDuels = Utils.GetValue<int>(data, "OverrideUnlockSecretsAtNumDuels");
                Shop.DefaultPackCardNum = Utils.GetValue<int>(data, "DefaultPackCardNum");
                Shop.OverridePackCardNum = Utils.GetValue<int>(data, "OverridePackCardNum");
            }
            LoadShopItems(Shop.PackShop, "PackShop", data, secretPackDuration);
            LoadShopItems(Shop.StructureShop, "StructureShop", data, secretPackDuration);
            LoadShopItems(Shop.AccessoryShop, "AccessoryShop", data, secretPackDuration);
            LoadShopItems(Shop.SpecialShop, "SpecialShop", data, secretPackDuration);

            List<int> specialTimeShops = Utils.GetIntList(data, "SpecialTimePackShops");
            if (specialTimeShops != null)
            {
                foreach (int shopId in specialTimeShops)
                {
                    ShopItemInfo info;
                    if (Shop.PackShop.TryGetValue(shopId, out info))
                    {
                        info.IsSpecialTime = true;
                    }
                }
            }

            List<object> packShopImageStrings = Utils.GetValue(data, "PackShopImages", default(List<object>));
            if (packShopImageStrings != null)
            {
                foreach (object obj in packShopImageStrings)
                {
                    string packShopImage = obj as string;
                    if (!string.IsNullOrEmpty(packShopImage))
                    {
                        int cardId = int.Parse(packShopImage.Split('_')[1]);
                        Shop.PackShopImagesByCardId[cardId] = packShopImage;
                    }
                }
            }
        }

        void LoadShopItems(Dictionary<int, ShopItemInfo> shopItems, string type, Dictionary<string, object> shopData, long secretPackDuration)
        {
            foreach (Dictionary<string, object> data in Utils.GetDictionaryCollection(shopData, type))
            {
                ShopItemInfo info = new ShopItemInfo();
                info.Buylimit = Utils.GetValue<int>(data, "buyLimit");// custom
                info.SecretBuyLimit = Utils.GetValue<int>(data, "secretBuyLimit");// custom
                info.SecretType = (ShopItemSecretType)Utils.GetValue<int>(data, "secretType");// custom
                if (Utils.TryGetValue(data, "secretDuration", out info.SecretDurationInSeconds) && info.SecretDurationInSeconds <= 0)// custom
                {
                    info.SecretDurationInSeconds = -1;// Temporary, set to 0 in the default/override value checks
                }
                info.IconType = (ShopItemIconType)Utils.GetValue<int>(data, "iconType");
                info.IconData = Utils.GetValue<string>(data, "iconData");
                info.SubCategory = Utils.GetValue<int>(data, "subCategory", 1);
                info.TargetCategory = Utils.GetValue<int>(data, "targetCategory");
                info.ProductType = Utils.GetValue<int>(data, "productType");
                object previewObj = Utils.GetValue<object>(data, "preview");
                if (previewObj is string)
                {
                    info.Preview = previewObj as string;
                }
                else if (previewObj != null)
                {
                    info.Preview = MiniJSON.Json.Serialize(previewObj);
                }
                object decorationObj = Utils.GetValue<object>(data, "decoration");
                if (decorationObj is string)
                {
                    info.Decoration = decorationObj as string;
                }
                else if (previewObj != null)
                {
                    info.Decoration = MiniJSON.Json.Serialize(decorationObj);
                }
                List<int> unlockSecrets = Utils.GetIntList(data, "unlockSecrets");// custom
                if (unlockSecrets != null && unlockSecrets.Count > 0)
                {
                    info.UnlockSecrets.AddRange(unlockSecrets);
                    info.UnlockSecretsAtPercent = Utils.GetValue<double>(data, "unlockSecretsAtPercent");
                }
                List<int> setPurchased = Utils.GetIntList(data, "setPurchased");// custom
                if (setPurchased != null && setPurchased.Count > 0)
                {
                    info.SetPurchased.AddRange(setPurchased);
                }
                List<object> searchCategoryList = Utils.GetValue<List<object>>(data, "searchCategory");
                if (searchCategoryList != null)
                {
                    foreach (object entry in searchCategoryList)
                    {
                        if (entry is string)
                        {
                            CardCategory category;
                            int categoryId;
                            if (int.TryParse(entry as string, out categoryId))
                            {
                                if (categoryId > 0)
                                {
                                    info.SearchCategory.Add(categoryId);
                                }
                            }
                            else if (CardCategoriesByName.TryGetValue(entry as string, out category))
                            {
                                if (category.Id > 0)
                                {
                                    info.SearchCategory.Add(category.Id);
                                }
                            }
                        }
                        else
                        {
                            int categoryId = (int)Convert.ChangeType(entry, typeof(int));
                            if (categoryId > 0)
                            {
                                info.SearchCategory.Add(categoryId);
                            }
                        }
                    }
                }
                switch (type)
                {
                    case "PackShop":
                        info.Category = ShopCategory.Pack;
                        info.Id = Utils.GetValue<int>(data, data.ContainsKey("targetId") ? "targetId" : "packId");
                        if (info.Id == 0)
                        {
                            Utils.LogWarning("Invalid pack id id " + info.Id + " in shop data");
                            return;
                        }
                        info.PackType = (ShopPackType)Utils.GetValue<int>(data, "packType");
                        if (info.PackType == 0)
                        {
                            info.PackType = (ShopPackType)(info.TargetCategory > 0 ? info.TargetCategory : 1);
                        }
                        info.CardNum = Utils.GetValue<int>(data, "pack_card_num", 1);
                        info.PackImageName = Utils.GetValue<string>(data, "packImage");// custom
                        object cardListObj;
                        if (data.TryGetValue("cardList", out cardListObj))// custom
                        {
                            if (cardListObj is List<object>)
                            {
                                List<object> cardList = cardListObj as List<object>;
                                foreach (object card in cardList)
                                {
                                    info.Cards[(int)Convert.ChangeType(card, typeof(int))] = CardRarity.None;
                                }
                            }
                            else if (cardListObj is Dictionary<string, object>)
                            {
                                Dictionary<string, object> cardList = cardListObj as Dictionary<string, object>;
                                foreach (KeyValuePair<string, object> card in cardList)
                                {
                                    int cardId;
                                    if (int.TryParse(card.Key, out cardId))
                                    {
                                        info.Cards[cardId] = (CardRarity)(int)Convert.ChangeType(card.Value, typeof(int));
                                    }
                                }
                            }
                        }
                        if (info.Cards.Count == 0)
                        {
                            Utils.LogWarning("Card pack " + info.Id + " doesn't have any cards");
                        }
                        switch (info.PackType)
                        {
                            case ShopPackType.Secret:
                                if (info.SecretType == ShopItemSecretType.None)
                                {
                                    info.SecretType = ShopItemSecretType.FindOrCraft;
                                }
                                break;
                            case ShopPackType.Standard:
                                if (Utils.GetValue<bool>(data, "isMainStandardPack"))// custom
                                {
                                    Shop.StandardPack = info;
                                }
                                break;
                        }
                        break;
                    case "StructureShop":
                        info.Category = ShopCategory.Structure;
                        info.Id = Utils.GetValue<int>(data, "targetId");
                        if (info.Buylimit == 0)
                        {
                            info.Buylimit = Utils.GetValue<int>(data, "limit_buy_count");
                        }
                        if (info.Buylimit == 0)
                        {
                            info.Buylimit = 3;
                        }
                        if (!StructureDecks.ContainsKey(info.Id))
                        {
                            // Unknown structure deck
                            Utils.LogWarning("Unknown structure deck id " + info.Id + " in shop data");
                            return;
                        }
                        break;
                    case "AccessoryShop":
                        info.Category = ShopCategory.Accessory;
                        info.Id = Utils.GetValue<int>(data, "itemId");// There's also "item_id"
                        ItemID.Category itemCategory = ItemID.GetCategoryFromID(info.Id);
                        switch (itemCategory)
                        {
                            case ItemID.Category.AVATAR:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Mate;
                                break;
                            case ItemID.Category.FIELD:
                            case ItemID.Category.FIELD_OBJ:
                            case ItemID.Category.AVATAR_HOME:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Field;
                                break;
                            case ItemID.Category.PROTECTOR:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Protector;
                                break;
                            case ItemID.Category.ICON:
                            case ItemID.Category.ICON_FRAME:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Icon;
                                break;
                            case ItemID.Category.WALLPAPER:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Wallpaper;
                                break;
                            case ItemID.Category.DECK_CASE:
                                info.SubCategory = (int)ShopSubCategoryAccessory.DeckCase;
                                break;
                            case ItemID.Category.COIN:
                                info.SubCategory = (int)ShopSubCategoryAccessory.Coin;
                                break;
                            default:
                                Utils.LogWarning("Unhandled shop accessory type " + itemCategory + " for item id " + info.Id);
                                return;
                        }
                        break;
                    case "SpecialShop":
                        info.Category = ShopCategory.Special;
                        info.Id = Utils.GetValue<int>(data, "shopId");
                        if (info.Id > 0)
                        {
                            info.Id -= info.BaseShopId;
                        }
                        if (info.Buylimit == 0)
                        {
                            info.Buylimit = Utils.GetValue<int>(data, "limit_buy_count");
                        }
                        if (info.Buylimit == 0)
                        {
                            info.Buylimit = 1;
                        }
                        if (info.TargetCategory == 1)
                        {
                            foreach (Dictionary<string, object> priceData in Utils.GetDictionaryCollection(data, "prices"))
                            {
                                string pop = Utils.GetValue<string>(priceData, "POP");
                                Enum.TryParse<ShopBundleType>(pop, true, out info.BundleType);
                            }
                            if (info.BundleType == ShopBundleType.None)
                            {
                                // Unsupported bundle type
                                continue;
                            }
                            List<Dictionary<string, object>> setItems = Utils.GetDictionaryCollection(data, "setItems");
                            if (setItems != null)
                            {
                                foreach (Dictionary<string, object> itemData in setItems)
                                {
                                    ShopBundleItem item = new ShopBundleItem();
                                    item.ItemCategory = Utils.GetValue<int>(itemData, "item_category");
                                    item.ItemId = Utils.GetValue<int>(itemData, "item_id");
                                    item.Num = Utils.GetValue<int>(itemData, "num");
                                    item.Period = Utils.GetValue<bool>(itemData, "is_period");
                                    info.SetItems.Add(item);
                                }
                            }
                            // NOTE: For this to work SpecialShop should always be loaded after other packs
                            if (Utils.GetValue<int>(data, "pickupCardListId") != 0 && !string.IsNullOrEmpty(info.IconData))
                            {
                                int mrk;
                                string packImageInfo = info.IconData.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(x => x.StartsWith("pack=") && x.Contains("_"));
                                if (!string.IsNullOrEmpty(packImageInfo) && int.TryParse(packImageInfo.Split('_')[1], out mrk))
                                {
                                    ShopItemInfo packShop = Shop.PackShop.Values.FirstOrDefault(x => x.IconMrk == mrk);
                                    info.BundlePickupCardListId = packShop.ShopId;
                                }
                            }
                            ShopItemInfo standardPack = Shop.PackShop.Values.FirstOrDefault(x => x.PackType == ShopPackType.Standard);
                            if (standardPack != null)
                            {
                                info.BundleNormalCardListId = standardPack.ShopId;
                            }
                        }
                        else
                        {
                            // Only supporting bundles for now
                            // TODO: Deck box (category 15)
                            continue;
                        }
                        break;
                }
                switch (type)
                {
                    case "PackShop":
                    case "StructureShop":
                    case "SpecialShop":
                        info.NameText = Utils.GetValue<string>(data, "nameTextId");
                        info.DescShortText = Utils.GetValue<string>(data, "descShortTextId");
                        info.DescFullText = Utils.GetValue<string>(data, "descFullTextId");
                        info.DescTextGenerated = Utils.GetValue<bool>(data, "descGenerated");// custom
                        info.ReleaseDate = Utils.ConvertEpochTime(Utils.GetValue<long>(data, "releaseDate"));// custom
                        info.IconMrk = Utils.GetValue<int>(data, "iconMrk");
                        info.Power = Utils.GetValue<int>(data, "power");
                        info.Flexibility = Utils.GetValue<int>(data, "flexibility");
                        info.Difficulty = Utils.GetValue<int>(data, "difficulty");
                        info.OddsName = Utils.GetValue<string>(data, "oddsName");// custom
                        //info.IsSpecialTime = Utils.GetValue<bool>(data, "isSpecialTime");// This will be annoying to maintain. Get it from a seperate list.
                        break;
                }
                foreach (Dictionary<string, object> priceData in Utils.GetDictionaryCollection(data, "prices"))
                {
                    ShopItemPrice price = new ShopItemPrice();
                    price.Id = info.Prices.Count + 1;
                    price.Price = Utils.GetValue<int>(priceData, "use_item_num");
                    List<object> textArgs;
                    if (Utils.TryGetValue(priceData, "textArgs", out textArgs) && textArgs.Count > 0)
                    {
                        price.ItemAmount = Math.Max(1, (int)Convert.ChangeType(textArgs[0], typeof(int)));
                    }
                    else
                    {
                        Utils.TryGetValue(priceData, "itemAmount", out price.ItemAmount);
                    }
                    price.PopArgs = Utils.GetIntList(priceData, "POPArgs");
                    price.ItemAmount = Math.Max(1, price.ItemAmount);
                    info.Prices.Add(price);
                }
                if (info.Prices.Count == 0)
                {
                    // custom
                    int priceX1 = Utils.GetValue<int>(data, "price");
                    if (priceX1 > 0)
                    {
                        info.Prices.Add(new ShopItemPrice()
                        {
                            Id = 1,
                            ItemAmount = 1,
                            Price = priceX1
                        });
                        if (info.Category == ShopCategory.Pack)
                        {
                            int priceX10 = Utils.GetValue<int>(data, "priceX10", priceX1 * 10);
                            info.Prices.Add(new ShopItemPrice()
                            {
                                Id = 2,
                                ItemAmount = 10,
                                Price = priceX10
                            });
                        }
                    }
                }
                if (info.ShopId == 10003001)
                {
                    info.DisableUltraRareGuarantee = true;
                    const int legacyPackPrice = 10;
                    info.Prices.Clear();
                    info.Prices.Add(new ShopItemPrice()
                    {
                        Id = info.Prices.Count + 1,
                        Price = legacyPackPrice,
                        ItemAmount = 1,
                        MultiBuyLimit = 100
                    });
                    info.Prices.Add(new ShopItemPrice()
                    {
                        Id = info.Prices.Count + 1,
                        Price = legacyPackPrice,
                        ItemAmount = 1,
                        MultiBuyLimit = 10
                    });
                    info.Prices.Add(new ShopItemPrice()
                    {
                        Id = info.Prices.Count + 1,
                        Price = legacyPackPrice,
                        ItemAmount = 1,
                        MultiBuyLimit = 1
                    });
                }
                if (shopItems.ContainsKey(info.ShopId))
                {
                    Utils.LogWarning("Duplicate shop id " + info.ShopId);
                    continue;
                }
                shopItems[info.ShopId] = info;
            }
        }

        void LoadShopPackOdds(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            List<object> oddsList = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as List<object>;
            if (oddsList == null)
            {
                return;
            }
            foreach (object dataObj in oddsList)
            {
                Dictionary<string, object> data = dataObj as Dictionary<string, object>;
                if (data == null)
                {
                    continue;
                }
                ShopOddsInfo shopOdds = new ShopOddsInfo();
                shopOdds.Name = Utils.GetValue<string>(data, "name");
                List<object> packTypesList = Utils.GetValue(data, "packTypes", default(List<object>));
                if (packTypesList != null)
                {
                    foreach (object packType in packTypesList)
                    {
                        shopOdds.PackTypes.Add((ShopPackType)(int)Convert.ChangeType(packType, typeof(int)));
                    }
                }
                List<object> cardRateList = Utils.GetValue(data, "cardRateList", default(List<object>));
                if (cardRateList != null)
                {
                    foreach (object cardRateDataObj in cardRateList)
                    {
                        Dictionary<string, object> cardRateData = cardRateDataObj as Dictionary<string, object>;
                        ShopOddsRarity rarityInfo = new ShopOddsRarity();
                        rarityInfo.StartNum = Utils.GetValue<int>(cardRateData, "start_num");
                        rarityInfo.EndNum = Utils.GetValue<int>(cardRateData, "end_num");
                        rarityInfo.Standard = Utils.GetValue<bool>(cardRateData, "standard");
                        rarityInfo.GuaranteeRareMin = (CardRarity)Utils.GetValue<int>(cardRateData, "settle_rare_min");
                        rarityInfo.GuaranteeRareMax = (CardRarity)Utils.GetValue<int>(cardRateData, "settle_rare_max");
                        Dictionary<string, object> rateData = Utils.GetDictionary(cardRateData, "rate");
                        foreach (KeyValuePair<string, object> item in rateData)
                        {
                            CardRarity rarity = (CardRarity)int.Parse(item.Key);
                            if (rarity == CardRarity.None)
                            {
                                Utils.LogWarning("Invalid card rarity in shop odds");
                            }
                            else
                            {
                                rarityInfo.Rate[rarity] = Utils.GetValue<double>(item.Value as Dictionary<string, object>, "rate");
                            }
                        }
                        shopOdds.CardRateList.Add(rarityInfo);
                    }
                }
                List<object> premiereRateList = Utils.GetValue(data, "premiereRateList", default(List<object>));
                if (premiereRateList != null)
                {
                    foreach (object premRateDataObj in premiereRateList)
                    {
                        Dictionary<string, object> premRateData = premRateDataObj as Dictionary<string, object>;
                        List<object> rareData = Utils.GetValue(premRateData, "rare", default(List<object>));
                        Dictionary<string, object> rateData = Utils.GetDictionary(premRateData, "rate");
                        if (rareData != null && rateData != null)
                        {
                            ShopOddsStyleRarity rarityInfo = new ShopOddsStyleRarity();
                            foreach (object rareObj in rareData)
                            {
                                CardRarity rarity = (CardRarity)(int)Convert.ChangeType(rareObj, typeof(int));
                                if (rarity != CardRarity.None)
                                {
                                    rarityInfo.Rarities.Add(rarity);
                                }
                            }
                            foreach (KeyValuePair<string, object> rate in rateData)
                            {
                                CardStyleRarity value = (CardStyleRarity)(int)Convert.ChangeType(rate.Key, typeof(int));
                                rarityInfo.Rate[value] = (double)Convert.ChangeType(rate.Value, typeof(double));
                            }
                            shopOdds.CardStyleRarityRateList.Add(rarityInfo);
                        }
                    }
                }
                foreach (ShopPackType packType in shopOdds.PackTypes)
                {
                    if (Shop.PackOddsByPackType.ContainsKey(packType))
                    {
                        Utils.LogWarning("Duplicate shop odds for pack type " + packType);
                    }
                    else
                    {
                        Shop.PackOddsByPackType[packType] = shopOdds;
                    }
                }
                if (!string.IsNullOrEmpty(shopOdds.Name))
                {
                    if (Shop.PackOddsByName.ContainsKey(shopOdds.Name))
                    {
                        Utils.LogWarning("Duplicate shop odds name '" + shopOdds.Name + "'");
                    }
                    else
                    {
                        Shop.PackOddsByName[shopOdds.Name] = shopOdds;
                    }
                }
            }
        }

        void LoadShopPackOddsVisuals(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            ShopOddsVisualsSettings settings = Shop.PackOddsVisuals;
            settings.RarityJebait = Utils.GetValue<bool>(data, "RarityJebait", true);
            settings.RarityOnCardBack = Utils.GetValue<bool>(data, "RarityOnCardBack", true);
            settings.RarityOnPack = Utils.GetValue<bool>(data, "RarityOnPack", true);
        }

        void LoadSolo()
        {
            string file = Path.Combine(dataDirectory, "Solo.json");
            if (!File.Exists(file))
            {
                Utils.LogWarning("Failed to load solo file");
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
            data = Utils.GetResData(data);
            Dictionary<string, object> masterData;
            if (data != null && Utils.TryGetValue(data, "Master", out masterData))
            {
                data = masterData;
            }
            if (data != null)
            {
                Utils.TryGetValue(data, "Solo", out SoloData);
                if (SoloData != null)
                {
                    Dictionary<string, object> allGateData = Utils.GetDictionary(SoloData, "gate");
                    if (allGateData != null)
                    {
                        foreach (KeyValuePair<string, object> gate in allGateData)
                        {
                            Dictionary<string, object> gateData = gate.Value as Dictionary<string, object>;
                            if (gateData != null)
                            {
                                /*if (!gateData.ContainsKey("open_date"))
                                {
                                    gateData["open_date"] = Utils.GetEpochTime(DateTime.MinValue);
                                }*/
                                if (!gateData.ContainsKey("category"))
                                {
                                    // Category needs to be set otherwise the gate wont show up anywhere
                                    gateData["category"] = 1;// "Stories"
                                }
                            }
                        }
                    }
                    Dictionary<string, object> allChapterData = Utils.GetDictionary(SoloData, "chapter");
                    if (allChapterData != null)
                    {
                        foreach (KeyValuePair<string, object> chapterGate in allChapterData)
                        {
                            Dictionary<string, object> chapters = chapterGate.Value as Dictionary<string, object>;
                            if (chapters != null)
                            {
                                foreach (KeyValuePair<string, object> chapter in chapters)
                                {
                                    Dictionary<string, object> chapterData = chapter.Value as Dictionary<string, object>;
                                    if (chapterData != null && !chapterData.ContainsKey("anime"))
                                    {
                                        chapterData["anime"] = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            LoadSoloDuels();
        }

        void LoadSoloDuels()
        {
            SoloDuels = new Dictionary<int, DuelSettings>();
            int nextStoryDeckId = 1;
            string dir = Path.Combine(dataDirectory, "SoloDuels");
            if (!Directory.Exists(dir))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(dir, "*.json", SearchOption.AllDirectories))
            {
                Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                data = Utils.GetResData(data);
                Dictionary<string, object> duelData;
                if (!Utils.TryGetValue(data, "Duel", out duelData))
                {
                    continue;
                }
                int chapterId;
                if (!Utils.TryGetValue(duelData, "chapter", out chapterId))
                {
                    continue;
                }
                DuelSettings duel = new DuelSettings();
                duel.FromDictionary(duelData);
                for (int i = 0; i < duel.Deck.Length; i++)
                {
                    if (duel.Deck[i].MainDeckCards.Count > 0)
                    {
                        duel.Deck[i].Id = nextStoryDeckId++;
                    }
                }
                duel.SetRequiredDefaults();
                if (SoloDuels.ContainsKey(chapterId))
                {
                    Utils.LogWarning("Duplicate chapter " + chapterId);
                    continue;
                }
                SoloDuels[chapterId] = duel;
            }
            string jsonFile = Path.Combine(Path.Combine(dataDirectory, "SoloNpcDeckIds.json"));
            if (File.Exists(jsonFile))
            {
                Dictionary<string, object> chapterDatas = MiniJSON.Json.DeserializeStripped(File.ReadAllText(jsonFile)) as Dictionary<string, object>;
                if (chapterDatas != null)
                {
                    foreach (KeyValuePair<string, object> chapter in chapterDatas)
                    {
                        int npcDeckId = (int)Convert.ChangeType(chapter.Value, typeof(int));
                        int chapterId;
                        DuelSettings duelSettings;
                        if (int.TryParse(chapter.Key, out chapterId) && SoloDuels.TryGetValue(chapterId, out duelSettings))
                        {
                            duelSettings.npc_deck_id = npcDeckId;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper to merge packet logs of visting the shop to extract out desired information (card packs)
        /// </summary>
        void MergeShopDumps()
        {
            // This is a mess. TODO: Improve this
            string dir = Path.Combine(dataDirectory, "ShopDumps");
            if (!Directory.Exists(dir))
            {
                return;
            }
            Dictionary<int, Dictionary<string, object>> packShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> packShopSecretsOnly = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> structureShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> accessoryShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> specialShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, List<int>> extraCardLists = new Dictionary<int, List<int>>();
            foreach (string file in Directory.GetFiles(dir).OrderBy(x => new FileInfo(x).LastWriteTime))
            {
                Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                data = Utils.GetResData(data);
                if (data != null)
                {
                    Dictionary<string, object> shopData = Utils.GetValue(data, "Shop", default(Dictionary<string, object>));
                    if (shopData != null)
                    {
                        Action<string, Dictionary<int, Dictionary<string, object>>> fetchShop = (string shopName, Dictionary<int, Dictionary<string, object>> shop) =>
                            {
                                bool foundPackShop = false;
                                Dictionary<string, object> items = Utils.GetValue(shopData, shopName, default(Dictionary<string, object>));
                                if (shopName != "PackShop" && items.Count > 0)
                                {
                                    shop.Clear();
                                }
                                foreach (KeyValuePair<string, object> item in items)
                                {
                                    int shopId;
                                    if (int.TryParse(item.Key, out shopId))
                                    {
                                        Dictionary<string, object> itemData = item.Value as Dictionary<string, object>;
                                        if (itemData != null)
                                        {
                                            if (shopName == "PackShop")
                                            {
                                                int packId = Utils.GetValue<int>(itemData, "packId");
                                                int targetId = Utils.GetValue<int>(itemData, "targetId");
                                                const int basePackShopId = 10000000;
                                                if (targetId != 0)
                                                {
                                                    shopId = basePackShopId + targetId;
                                                }
                                                else if (packId != 0)
                                                {
                                                    shopId = basePackShopId + packId;
                                                }

                                                // Temp hacky fix for free pulls on some re-released packs with alt card art
                                                // TODO: Remove this and provide a better shop dumper (unlock all secrets at present time, dump entire shop into one file)
                                                if ((shopId == 10002152 && Path.GetFileName(file) == "Shop-2023-03-15.json") ||
                                                    (shopId == 10002078 && Path.GetFileName(file) == "Shop-2023-03-15.json"))
                                                {
                                                    continue;
                                                }

                                                Dictionary<string, object> allPrices = Utils.GetDictionary(itemData, "prices");
                                                if (allPrices != null)
                                                {
                                                    foreach (KeyValuePair<string, object> priceDataObj in allPrices)
                                                    {
                                                        Dictionary<string, object> priceData = priceDataObj.Value as Dictionary<string, object>;
                                                        if (priceData != null && priceData.ContainsKey("free_num"))
                                                        {
                                                            Utils.LogWarning("Shop " + shopId + " has a free pull. Use it and re-log the shop data");
                                                        }
                                                    }
                                                }

                                                ShopPackType packType = Utils.GetValue<ShopPackType>(itemData, "packType");
                                                if (packType == ShopPackType.None)
                                                {
                                                    packType = Utils.GetValue<ShopPackType>(itemData, "targetCategory");
                                                    if (packType == ShopPackType.None)
                                                    {
                                                        Utils.LogWarning("No pack type for " + shopId + " in " + file);
                                                    }
                                                }
                                                if (packType == ShopPackType.Secret)
                                                {
                                                    packShopSecretsOnly[shopId] = itemData;
                                                }
                                                else
                                                {
                                                    // Special case. We aren't merging pack shops anymore due to old assets not being downlaoded.
                                                    if (!foundPackShop)
                                                    {
                                                        foundPackShop = true;
                                                        shop.Clear();
                                                    }
                                                    shop[shopId] = itemData;
                                                }
                                            }
                                            else
                                            {
                                                shop[shopId] = itemData;
                                            }
                                        }
                                        Dictionary<string, object> gachaData = Utils.GetValue(data, "Gacha", default(Dictionary<string, object>));
                                        if (gachaData != null)
                                        {
                                            Dictionary<string, object> cardListData = Utils.GetValue(gachaData, "cardList", default(Dictionary<string, object>));
                                            if (cardListData != null)
                                            {
                                                int normalCardListId = Utils.GetValue<int>(itemData, "normalCardListId");
                                                int pickupCardListId = Utils.GetValue<int>(itemData, "pickupCardListId");
                                                object cardIdsObj = null;
                                                if (pickupCardListId != 0)
                                                {
                                                    Utils.TryGetValue(cardListData, pickupCardListId.ToString(), out cardIdsObj);
                                                }
                                                else if (normalCardListId != 0)
                                                {
                                                    Utils.TryGetValue(cardListData, normalCardListId.ToString(), out cardIdsObj);
                                                }
                                                if (cardIdsObj != null)
                                                {
                                                    itemData["cardList"] = cardIdsObj;
                                                }
                                            }
                                        }
                                    }
                                }
                            };
                        fetchShop("PackShop", packShop);
                        fetchShop("StructureShop", structureShop);
                        fetchShop("AccessoryShop", accessoryShop);
                        fetchShop("SpecialShop", specialShop);
                    }
                    else if (file.Contains("Gacha-"))
                    {
                        string[] splitted = file.Split('-', '.');
                        int shopId;
                        if (int.TryParse(splitted[splitted.Length - 2], out shopId))
                        {
                            Dictionary<string, object> gachaData = Utils.GetValue(data, "Gacha", default(Dictionary<string, object>));
                            if (gachaData != null)
                            {
                                Dictionary<string, object> cardListData = Utils.GetValue(gachaData, "cardList", default(Dictionary<string, object>));
                                if (cardListData != null)
                                {
                                    if (cardListData.Count > 1)
                                    {
                                        Console.WriteLine("Unexpected gacha count of " + cardListData.Count + " for shop " + shopId);
                                    }
                                    else
                                    {
                                        extraCardLists[shopId] = Utils.GetIntList(cardListData, cardListData.First().Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, Dictionary<string, object>> secretPackShopItem in packShopSecretsOnly)
            {
                packShop[secretPackShopItem.Key] = secretPackShopItem.Value;
            }
            int[] secretPackIds = packShopSecretsOnly.Keys.OrderBy(x => x).ToArray();
            for (int i = 0; i < secretPackIds.Length - 1; i++)
            {
                int secretPackId = secretPackIds[i];
                int nextSecretPackId = secretPackIds[i + 1];
                if (nextSecretPackId != secretPackId + 1 && nextSecretPackId - secretPackId < 10)
                {
                    for (int j = secretPackId + 1; j < nextSecretPackId; j++)
                    {
                        Console.WriteLine("[WARNING] Missing secret pack shop id " + j);
                    }
                }
            }
            foreach (KeyValuePair<int, List<int>> cardList in extraCardLists)
            {
                if (!packShop.ContainsKey(cardList.Key))
                {
                    Console.WriteLine("[WARNING] Couldn't find shop for gacha list " + cardList.Key);
                    continue;
                }
                packShop[cardList.Key]["cardList"] = cardList.Value;
            }
            // NOTE:
            // We reassign the card list ids to their shop ids.
            // On the real server each card list has its own id which is seperate from the shop id.
            // It makes sense for card lists to be seperate in cases like bonus packs where each bonus pack
            // is the same. Though the legacy pack is essentially the "main" / "master" bonus pack
            foreach (KeyValuePair<int, Dictionary<string, object>> packShopItem in new Dictionary<int, Dictionary<string, object>>(packShop))
            {
                int normalCardListId = Utils.GetValue<int>(packShopItem.Value, "normalCardListId");
                int pickupCardListId = Utils.GetValue<int>(packShopItem.Value, "pickupCardListId");
                if (normalCardListId > 0 && normalCardListId < 20000)
                {
                    // This should always be trying to point to the master pack
                    packShopItem.Value["normalCardListId"] = 10000001;
                }
                else if (normalCardListId > 0)
                {
                    packShopItem.Value["normalCardListId"] = packShopItem.Key;
                }
                if (pickupCardListId > 0 && pickupCardListId < 20000)
                {
                    Console.WriteLine("Unexpected pickup id " + pickupCardListId);
                }
                else if (pickupCardListId > 0)
                {
                    packShopItem.Value["pickupCardListId"] = packShopItem.Key;
                }
                if (!packShopItem.Value.ContainsKey("cardList"))
                {
                    Utils.LogWarning("No card list for shop " + packShopItem.Key);
                    packShop.Remove(packShopItem.Key);
                }
            }
            MergeShops(Path.Combine(dataDirectory, "AllShopsMerged.json"), packShop, structureShop, accessoryShop, specialShop);
        }

        void MergeShops(string outputFile,
            Dictionary<int, Dictionary<string, object>> packShop,
            Dictionary<int, Dictionary<string, object>> structureShop,
            Dictionary<int, Dictionary<string, object>> accessoryShop,
            Dictionary<int, Dictionary<string, object>> specialShop)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, Dictionary<int, Dictionary<string, object>>> allShops = new Dictionary<string, Dictionary<int, Dictionary<string, object>>>();
            if (packShop != null)
            {
                allShops["PackShop"] = packShop;
            }
            if (structureShop != null)
            {
                allShops["StructureShop"] = structureShop;
            }
            if (accessoryShop != null)
            {
                allShops["AccessoryShop"] = accessoryShop;
            }
            if (specialShop != null)
            {
                allShops["SpecialShop"] = specialShop;
            }
            foreach (KeyValuePair<string, Dictionary<int, Dictionary<string, object>>> shop in allShops)
            {
                sb.AppendLine("    \"" + shop.Key + "\": {");
                foreach (KeyValuePair<int, Dictionary<string, object>> shopItem in shop.Value.OrderBy(x => x.Key))
                {
                    sb.AppendLine("        \"" + shopItem.Key + "\":" + MiniJSON.Json.Serialize(shopItem.Value) + ",");
                }
                sb.AppendLine("    },");
            }
            File.WriteAllText(outputFile, sb.ToString());
        }

        // TODO: Remove. This is no longer being used (though fetching official sets is still useful)
        void CreateShopsFromOfficialSets()
        {
            // TODO: Change this to scrape wikia/yugipedia to get more verbose info
            string file = Path.Combine(dataDirectory, "OfficialSets.json");
            YgoDbSetCollection collection = new YgoDbSetCollection();
            if (!File.Exists(file))
            {
                Console.WriteLine("Downloading sets...");
                string dumpDir = Path.Combine(dataDirectory, "OfficialSetsDump");
                Utils.TryCreateDirectory(dumpDir);
                collection.DownloadSets(dumpDir);
                collection.Save(file);
                Console.WriteLine("Done");
            }
            collection.Load(file);

            const int basePackId = 9000;
            int nextPackId = basePackId + 1;
            const int baseStructureId = 1120900;// Should be high enough
            int nextStructureId = baseStructureId + 1;

            string extraDataFile = Path.Combine(dataDirectory, "OfficialSetsExtraData.json");
            if (!File.Exists(extraDataFile))
            {
                Console.WriteLine("Couldn't find extra data for official sets");
                return;
            }
            Dictionary<string, object> allExtraData = MiniJSON.Json.DeserializeStripped(File.ReadAllText(extraDataFile)) as Dictionary<string, object>;
            Dictionary<long, Dictionary<string, object>> extraDataCollection = new Dictionary<long, Dictionary<string, object>>();
            List<object> extraDataSets = Utils.GetValue(allExtraData, "sets", default(List<object>));
            foreach (object obj in extraDataSets)
            {
                Dictionary<string, object> extra = obj as Dictionary<string, object>;
                long setId;
                if (extra != null && Utils.TryGetValue(extra, "id", out setId))
                {
                    extraDataCollection[setId] = extra;
                }
            }
            List<object> extraDataAutoSetsListObj = Utils.GetValue(allExtraData, "autoSets", default(List<object>));
            List<DateTime> extraDataAutoSetsList = new List<DateTime>();
            if (extraDataAutoSetsListObj != null)
            {
                foreach (object obj in extraDataAutoSetsListObj)
                {
                    extraDataAutoSetsList.Add(DateTime.Parse(obj as string));
                }
            }
            extraDataAutoSetsList.Sort();
            int nextAutoSetIndex = extraDataAutoSetsList.Count > 0 ? 0 : -1;

            bool isFirstPack = true;
            Dictionary<int, Dictionary<string, object>> packShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, Dictionary<string, object>> structureShop = new Dictionary<int, Dictionary<string, object>>();
            Dictionary<int, CardRarity> lowestCardRarity = new Dictionary<int, CardRarity>();
            HashSet<int> usedCardIds = new HashSet<int>();
            foreach (YgoDbSet set in collection.Sets.Values.OrderBy(x => x.ReleaseDate))
            {
                foreach (KeyValuePair<int, YgoDbSet.CardRarity> card in set.Cards)
                {
                    if (CardRare.ContainsKey(card.Key))
                    {
                        CardRarity rarity = YgoDbSet.ConvertRarity(card.Value);
                        if (rarity > CardRarity.None)
                        {
                            CardRarity currentRarity;
                            if (!lowestCardRarity.TryGetValue(card.Key, out currentRarity) || rarity < currentRarity)
                            {
                                lowestCardRarity[card.Key] = rarity;
                            }
                        }
                    }
                }

                string setName;
                Dictionary<string, object> extraData;
                if (!extraDataCollection.TryGetValue(set.Id, out extraData) ||
                    //!Utils.GetValue<bool>(extraData, "core") ||
                    !Utils.TryGetValue(extraData, "name", out setName))
                {
                    continue;
                }
                int coverCardId = Utils.GetValue<int>(extraData, "cover");
                if (coverCardId == 0)
                {
                    Utils.LogWarning("Set " + set.Id + " doesn't have a cover card");
                    continue;
                }
                if (!CardRare.ContainsKey(coverCardId))
                {
                    Utils.LogWarning("Cover card id " + coverCardId + " is not in the game (set: " + set.Id + ")");
                    coverCardId = 6018;// Mokey mokey
                }
                if (set.Type == YgoDbSetType.StarterDeck)
                {
                    int structureId = nextStructureId++;

                    int sleeve = Utils.GetValue<int>(extraData, "sleeve");
                    int box = Utils.GetValue<int>(extraData, "box");
                    
                    DeckInfo deck = new DeckInfo();
                    deck.Id = structureId;
                    deck.Accessory.Box = box > 0 ? box : (int)ItemID.Value.DefaultDeckCase;
                    deck.Accessory.Sleeve = sleeve > 0 ? sleeve : (int)ItemID.Value.DefaultProtector;

                    foreach (int cardId in set.Cards.Keys)
                    {
                        if (!CardRare.ContainsKey(cardId))
                        {
                            continue;
                        }
                        usedCardIds.Add(cardId);
                        // TODO: Determine way to get the card type
                        switch (cardId)
                        {
                            case 4021:// Flame Swordsman
                            case 4075:// Thousand Dragon
                                deck.ExtraDeckCards.Add(cardId);
                                break;
                            default:
                                deck.MainDeckCards.Add(cardId);
                                break;
                        }
                    }

                    List<int> focusCards = Utils.GetIntList(extraData, "focus");
                    if (focusCards != null && focusCards.Count > 0)
                    {
                        for (int i = 0; i < focusCards.Count && i < 3; i++)
                        {
                            deck.DisplayCards.Add(focusCards[i]);
                        }
                    }
                    // NOTE: Focus cards are required outwise it'll bug out the shop
                    while (deck.DisplayCards.Count < 3)
                    {
                        deck.DisplayCards.Add(6018);// Mokey mokey
                    }
                    structureShop[ShopInfo.GetBaseShopId(ShopCategory.Structure) + structureId] = new Dictionary<string, object>()
                    {
                        { "releaseDate", Utils.GetEpochTime(set.ReleaseDate) },
                        { "category", (int)ShopCategory.Structure },
                        { "subCategory", 1 },
                        { "price", 100 },
                        { "iconMrk", coverCardId },
                        { "iconType", (int)ShopItemIconType.CardThumb },
                        { "iconData", coverCardId.ToString() },
                        //{ "preview", },//TODO
                        //{ "searchCategory", },//TODO
                        { "structure_id", structureId },
                        { "accessory", deck.Accessory.ToDictionary() },
                        { "focus", deck.DisplayCards.ToDictionary() },
                        { "contents", deck.ToDictionary() }
                    };
                    Dictionary<string, object> structureDeckData = deck.ToDictionaryStructureDeck();
                    File.WriteAllText(Path.Combine(dataDirectory, "StructureDecks", structureId + ".json"), MiniJSON.Json.Serialize(structureDeckData));
                }
                else if (set.Type == YgoDbSetType.BoosterPack)
                {
                    int numCardsInGame = 0;
                    foreach (KeyValuePair<int, YgoDbSet.CardRarity> card in set.Cards)
                    {
                        if (CardRare.ContainsKey(card.Key))
                        {
                            numCardsInGame++;
                        }
                    }
                    if (set.Cards.Count - numCardsInGame > 10) //numCardsInGame < 55)
                    {
                        Utils.LogWarning("Skip set " + set.Id + " with only " + numCardsInGame + "/" + set.Cards.Count + " cards in game '" + set.Name + "'");
                        continue;
                    }

                    if (nextAutoSetIndex >= 0 && nextAutoSetIndex < extraDataAutoSetsList.Count)
                    {
                        // TODO
                        DateTime autoSetDate = extraDataAutoSetsList[nextAutoSetIndex];
                        if (autoSetDate < set.ReleaseDate)
                        {
                            int numAdded = 0;
                            int numUltra = 0, numSuper = 0, numRare = 0, numNormal = 0;
                            foreach (KeyValuePair<int, CardRarity> card in lowestCardRarity)
                            {
                                if (!usedCardIds.Contains(card.Key))
                                {
                                    numAdded++;
                                    usedCardIds.Add(card.Key);
                                    switch (card.Value)
                                    {
                                        case CardRarity.UltraRare: numUltra++; break;
                                        case CardRarity.SuperRare: numSuper++; break;
                                        case CardRarity.Rare: numRare++; break;
                                        case CardRarity.Normal: numNormal++; break;
                                    }
                                }
                            }
                            Console.WriteLine("Add " + numAdded + " " + set.ReleaseDate + " " + autoSetDate + " | " + numUltra + " " + numSuper + " " + numRare + " " + numNormal);
                            nextAutoSetIndex++;
                        }
                    }

                    Dictionary<int, int> cards = new Dictionary<int, int>();
                    foreach (KeyValuePair<int, YgoDbSet.CardRarity> card in set.Cards)
                    {
                        if (CardRare.ContainsKey(card.Key))
                        {
                            usedCardIds.Add(card.Key);
                            cards[card.Key] = (int)YgoDbSet.ConvertRarity(card.Value);
                        }
                    }
                    int packId = nextPackId++;
                    packShop[packId] = new Dictionary<string, object>()
                    {
                        { "packId", packId },
                        { "packType", (int)ShopPackType.Standard },
                        { "secretType", (int)(isFirstPack ? ShopItemSecretType.None : ShopItemSecretType.Other) },
                        { "unlockSecrets", new int[] { nextPackId } },
                        { "nameTextId", setName },
                        { "descGenerated", true },
                        { "releaseDate", Utils.GetEpochTime(set.ReleaseDate) },
                        //{ "packImage", "YdbCardPack_" + set.Id },
                        { "pack_card_num", Math.Min(8, Utils.GetValue<int>(extraData, "num", 8)) },
                        //{ "category", (int)ShopCategory.Pack },// Not needed
                        { "subCategory", 1 },
                        { "iconMrk", coverCardId },
                        { "iconType", (int)ShopItemIconType.ItemThumb },
                        { "iconData", "set" + set.Id },
                        { "preview", "[{\"type\":3,\"path\":\"set" + set.Id + "\"}]" },
                        { "packImage", "set" + set.Id },
                        //{ "preview", },//TODO
                        //{ "searchCategory", },//TODO
                        { "cardList", cards },
                        { "price", Utils.GetValue<int>(extraData, "price") },
                        { "oddsName", Utils.GetValue<string>(extraData, "odds") },
                    };
                    isFirstPack = false;
                }
            }
            if (extraDataAutoSetsList.Count > 0)
            {
                // TODO
                int numAddedFinal = 0;
                foreach (KeyValuePair<int, int> card in CardRare)
                {
                    if (!usedCardIds.Contains(card.Key))
                    {
                        numAddedFinal++;
                        usedCardIds.Add(card.Key);
                    }
                }
                Console.WriteLine("Add (final) " + numAddedFinal);
            }
            int numAdditionalCards = 0;
            if (packShop.Count > 0)
            {
                packShop[nextPackId - 1]["unlockSecrets"] = new int[] { nextPackId };
                int packId = nextPackId++;
                List<int> unusedCardIds = new List<int>();
                foreach (int cid in CardRare.Keys)
                {
                    if (!usedCardIds.Contains(cid))
                    {
                        numAdditionalCards++;
                        unusedCardIds.Add(cid);
                    }
                }
                packShop[packId] = new Dictionary<string, object>()
                {
                    { "isMainStandardPack", true },
                    { "packId", packId },
                    { "packType", (int)ShopPackType.Standard },
                    { "secretType", (int)ShopItemSecretType.Other },
                    { "nameTextId", "All remaining cards" },
                    { "descGenerated", true },
                    { "pack_card_num", 8 },
                    { "subCategory", 1 },
                    { "iconMrk", 0 },
                    { "iconType", (int)ShopItemIconType.ItemThumb },
                    { "iconData", "packthumb0001" },
                    { "preview", "[{\"type\":3,\"path\":\"packthumb0001\"}]" },
                    { "cardList", unusedCardIds },
                };
            }
            foreach (KeyValuePair<int, Dictionary<string, object>> shop in structureShop)
            {
                int[] otherIds = structureShop.Keys.ToList().FindAll(x => x != shop.Key).ToArray();
                shop.Value["setPurchased"] = otherIds;
            }
            Console.WriteLine("realPacks: " + usedCardIds.Count + " / " + CardRare.Keys.Count + " additional: " + numAdditionalCards);
            MergeShops(Path.Combine(dataDirectory, "GeneratedShopsFromOfficialSets.json"), packShop, structureShop, null, null);
        }

        // TODO: Remove. This is no longer being used.
        void ConvertLeDataToSolo()
        {
            // TODO: Also pull in the duelist challenges (maybe via sub gates?)
            string leDir = Path.Combine(dataDirectory, "LeData");
            if (!Directory.Exists(leDir))
            {
                Console.WriteLine("Couldn't find link evolution data directory '" + leDir + "'");
                return;
            }
            string duelsDir = Path.Combine(leDir, "Duels");
            string soloDuelsDir = Path.Combine(dataDirectory, "SoloDuels");

            Utils.TryCreateDirectory(soloDuelsDir);

            string dfymooText = "";
            string dfymooFile = Path.Combine(dataDirectory, "ClientData", "LinkEvolution", "chars.dfymoo");
            if (File.Exists(dfymooFile))
            {
                dfymooText = File.ReadAllText(dfymooFile).ToLower();
            }

            StringBuilder soloStrings = new StringBuilder();
            Dictionary<string, object> data = new Dictionary<string, object>();
            Dictionary<string, object> masterData = new Dictionary<string, object>();
            data["Master"] = masterData;
            Dictionary<string, object> soloData = new Dictionary<string, object>();
            masterData["Solo"] = soloData;
            Dictionary<string, object> allGatesData = new Dictionary<string, object>();
            Dictionary<string, object> allChaptersData = new Dictionary<string, object>();
            Dictionary<string, object> allRewardData = new Dictionary<string, object>();
            soloData["gate"] = allGatesData;
            soloData["chapter"] = allChaptersData;
            soloData["unlock"] = new Dictionary<string, object>();
            soloData["unlock_item"] = new Dictionary<string, object>();
            soloData["reward"] = allRewardData;
            Dictionary<string, string> dirs = new Dictionary<string, string>()
            {
                { "YuGiOh", "Yu-Gi-Oh!" },
                { "YuGiOhGX", "Yu-Gi-Oh! GX" },
                { "YuGiOh5D", "Yu-Gi-Oh! 5D's" },
                { "YuGiOhZEXAL", "Yu-Gi-Oh! ZEXAL" },
                { "YuGiOhARCV", "Yu-Gi-Oh! ARC-V" },
                { "YuGiOhVRAINS", "Yu-Gi-Oh! VRAINS" }
            };
            // gate 1 - everything is a practice duel
            // gate 2/3 - shows the wrong card flipping around
            int nextGateId = 4;
            int nextRewardId = 1;
            foreach (KeyValuePair<string, string> dir in dirs)
            {
                int gateId = nextGateId++;

                soloStrings.AppendLine("[IDS_SOLO.GATE" + (gateId.ToString().PadLeft(3, '0')) + "]");
                soloStrings.AppendLine(dir.Value);
                soloStrings.AppendLine("[IDS_SOLO.GATE" + (gateId.ToString().PadLeft(3, '0')) + "_EXPLANATION]]");
                soloStrings.AppendLine("Duels featuring cards from " + dir.Value);

                Dictionary<string, object> gateData = new Dictionary<string, object>();
                gateData["priority"] = gateId;
                gateData["parent_gate"] = 0;
                gateData["view_gate"] = 0;
                gateData["unlock_id"] = 0;
                allGatesData[gateId.ToString()] = gateData;
                Dictionary<string, object> gateChapterData = new Dictionary<string, object>();
                allChaptersData[gateId.ToString()] = gateChapterData;
                Dictionary<int, Dictionary<string, object>> seriesDuelDatas = new Dictionary<int, Dictionary<string, object>>();
                foreach (string file in Directory.GetFiles(Path.Combine(duelsDir, dir.Key)))
                {
                    Dictionary<string, object> duelData = MiniJSON.Json.Deserialize(File.ReadAllText(file)) as Dictionary<string, object>;
                    seriesDuelDatas[Utils.GetValue<int>(duelData, "displayIndex")] = duelData;
                }
                int nextChapterId = (gateId * 10000) + 1;
                int parentChapterId = 0;
                foreach (KeyValuePair<int, Dictionary<string, object>> duelOverviewData in seriesDuelDatas.OrderBy(x => x.Key))
                {
                    string deck1File = Path.Combine(leDir, Utils.GetValue<string>(duelOverviewData.Value, "playerDeck"));
                    string deck2File = Path.Combine(leDir, Utils.GetValue<string>(duelOverviewData.Value, "opponentDeck"));
                    Func<string, bool> validateDeck = (string path) =>
                        {
                            Dictionary<string, object> tempDeckData = MiniJSON.Json.Deserialize(File.ReadAllText(path)) as Dictionary<string, object>;
                            DeckInfo deck = new DeckInfo();
                            deck.FromDictionaryEx(tempDeckData);
                            bool valid = true;
                            foreach (int cardId in deck.GetAllCards())
                            {
                                if (!CardRare.ContainsKey(cardId))
                                {
                                    Console.WriteLine("Missing card " + cardId);
                                    valid = false;
                                }
                            }
                            return valid;
                        };
                    if (!validateDeck(deck1File) || !validateDeck(deck2File))
                    {
                        Console.WriteLine("Missing cards in '" + Utils.GetValue<string>(duelOverviewData.Value, "name") + "'");
                        //continue;
                    }

                    int randSeed = 0;
                    unchecked
                    {
                        byte[] nameBuffer = Encoding.UTF8.GetBytes(Utils.GetValue<string>(duelOverviewData.Value, "name"));
                        for (int i = 0; i < nameBuffer.Length; i += 4)
                        {
                            if (i >= nameBuffer.Length - 4)
                            {
                                randSeed += nameBuffer[i];
                            }
                            else
                            {
                                randSeed += BitConverter.ToInt32(nameBuffer, i);
                            }
                        }
                    }
                    Random rand = new Random(randSeed);
                    for (int j = 0; j < 2; j++)
                    {
                        int chapterId = nextChapterId++;
                        Dictionary<string, object> chapterData = new Dictionary<string, object>();

                        Dictionary<string, object> duelData = new Dictionary<string, object>();
                        List<object> decks = new List<object>();
                        for (int i = 0; i < 2; i++)
                        {
                            string targetFile = i == (j == 0 ? 0 : 1) ? deck1File : deck2File;
                            Dictionary<string, object> inputDeckData = MiniJSON.Json.Deserialize(File.ReadAllText(targetFile)) as Dictionary<string, object>;
                            DeckInfo deck = new DeckInfo();
                            deck.FromDictionaryEx(inputDeckData);
                            foreach (int cid in new List<int>(deck.MainDeckCards.GetIds()))
                            {
                                if (!CardRare.ContainsKey(cid))
                                {
                                    deck.MainDeckCards.RemoveAll(cid);
                                    deck.MainDeckCards.Add(4844);// Pot of greed
                                }
                            }
                            foreach (int cid in new List<int>(deck.ExtraDeckCards.GetIds()))
                            {
                                if (!CardRare.ContainsKey(cid))
                                {
                                    deck.ExtraDeckCards.RemoveAll(cid);
                                }
                            }
                            foreach (int cid in new List<int>(deck.SideDeckCards.GetIds()))
                            {
                                if (!CardRare.ContainsKey(cid))
                                {
                                    deck.SideDeckCards.RemoveAll(cid);
                                }
                            }
                            decks.Add(deck.ToDictionary(true));

                            int rewardId = nextRewardId++;
                            allRewardData[rewardId.ToString()] = new Dictionary<string, object>()
                            {
                                { ((int)ItemID.Category.CONSUME).ToString(), new Dictionary<string, object>() {
                                    { ((int)ItemID.Value.Gem).ToString(), rand.Next(5, 50) }
                                }}
                            };
                            // NOTE: This should probably be pulling the reward from the opponent deck only (currently it pulls from both)
                            /*const int numCardRewards = 1;// Can increase this but the client only shows 1 reward...
                            int rewardId = nextRewardId++;
                            Dictionary<string, object> cardRewards = new Dictionary<string, object>();
                            allRewardData[rewardId.ToString()] = new Dictionary<string, object>()
                            {
                                { ((int)ItemID.Category.CARD).ToString(), cardRewards }
                            };
                            List<int> deckCardIds = deck.GetAllCards(main: true, extra: true);
                            for (int k = 0; k < numCardRewards; k++)
                            {
                                if (deckCardIds.Count > 0)
                                {
                                    int targetCardId = deckCardIds[rand.Next(deckCardIds.Count)];
                                    deckCardIds.RemoveAll(x => x == targetCardId);
                                    cardRewards[targetCardId.ToString()] = 1;
                                }
                            }*/
                            if (i == 0)
                            {
                                chapterData["set_id"] = rewardId;
                            }
                            else
                            {
                                chapterData["mydeck_set_id"] = rewardId;
                            }
                        }
                        duelData["Deck"] = decks;
                        duelData["chapter"] = chapterId;
                        int fieldId = ItemID.GetRandomId(rand, ItemID.Category.FIELD);
                        int fieldObjId = ItemID.GetFieldObjFromField(fieldId);
                        int avatarBaseId = ItemID.GetFieldAvatarBaseFromField(fieldId);
                        duelData["mat"] = new List<int>() { fieldId, fieldId };
                        duelData["duel_object"] = new List<int>() { fieldObjId, fieldObjId };
                        duelData["avatar_home"] = new List<int>() { avatarBaseId, avatarBaseId };
                        duelData["name"] = new List<string>() { "", "CPU" };
                        duelData["sleeve"] = new List<int>() { 0, ItemID.GetRandomId(rand, ItemID.Category.PROTECTOR) };
                        duelData["icon"] = new List<int>() { 0, ItemID.GetRandomId(rand, ItemID.Category.ICON) };
                        duelData["icon_frame"] = new List<int>() { 0, ItemID.GetRandomId(rand, ItemID.Category.ICON_FRAME) };
                        duelData["avatar"] = new List<int>() { 0, ItemID.GetRandomId(rand, ItemID.Category.AVATAR) };
                        duelData["dialog_intro"] = Utils.GetValue<string>(duelOverviewData.Value, "dialogIntro");
                        duelData["dialog_outro"] = Utils.GetValue<string>(duelOverviewData.Value, "dialogOutro");
                        Dictionary<string, object> duelDataContainer = new Dictionary<string, object>();
                        duelDataContainer["Duel"] = duelData;
                        string outputDuelFileName = Path.Combine(soloDuelsDir, chapterId + ".json");
                        File.WriteAllText(outputDuelFileName, MiniJSON.Json.Serialize(duelDataContainer));

                        string duelName = Utils.GetValue<string>(duelOverviewData.Value, "name");
                        string duelDesc = Utils.GetValue<string>(duelOverviewData.Value, "description");
                        if (!string.IsNullOrEmpty(duelName))
                        {
                            soloStrings.AppendLine("[IDS_SOLO.CHAPTER" + chapterId + "_EXPLANATION]");
                            soloStrings.AppendLine(duelName + (j > 0 ? " (reverse duel)" : string.Empty));
                            if (!string.IsNullOrEmpty(duelDesc))
                            {
                                soloStrings.AppendLine();
                                soloStrings.AppendLine(duelDesc);
                            }
                        }

                        chapterData["parent_chapter"] = parentChapterId;
                        chapterData["unlock_id"] = 0;
                        chapterData["begin_sn"] = "";
                        chapterData["npc_id"] = 1;// Not sure if this value matter

                        string char1 = Utils.GetValue<string>(duelOverviewData.Value, "char1");
                        string char2 = Utils.GetValue<string>(duelOverviewData.Value, "char2");
                        if (!string.IsNullOrEmpty(char1) && !string.IsNullOrEmpty(char2))
                        {
                            char1 += "_neutral";
                            char2 += "_neutral";
                            if (dfymooText.Contains(char1.ToLower()) && dfymooText.Contains(char2.ToLower()))
                            {
                                chapterData["p1_img"] = char1;
                                chapterData["p2_img"] = char2;
                            }
                            else
                            {
                                Console.WriteLine("Couldn't find char img for " + char1 + ", " + char2);
                            }
                        }

                        gateChapterData[chapterId.ToString()] = chapterData;

                        if (j == 0)
                        {
                            parentChapterId = chapterId;
                        }
                    }
                }
                if (parentChapterId > 0)
                {
                    gateData["clear_chapter"] = parentChapterId;
                }
            }
            File.WriteAllText(Path.Combine(dataDirectory, "Solo.json"), MiniJSON.Json.Serialize(data));
            File.WriteAllText(Path.Combine(dataDirectory, "IDS_SOLO.txt"), soloStrings.ToString());
        }

        void ExtractStructureDecks()
        {
            string extractFile = Path.Combine(dataDirectory, "StructureDecks.json");
            if (!File.Exists(extractFile))
            {
                Console.WriteLine("Couldn't find '" + extractFile + "'");
                return;
            }
            string targetDir = Path.Combine(dataDirectory, "StructureDecks");
            if (!Utils.TryCreateDirectory(targetDir))
            {
                Console.WriteLine("Couldn't find '" + targetDir + "'");
                return;
            }
            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(extractFile)) as Dictionary<string, object>;
            Dictionary<string, object> structureDecksData = Utils.GetDictionary(data, "Structure");
            if (structureDecksData == null && data.ContainsKey(((int)ItemID.Value.StartingStructureDeck).ToString()))
            {
                structureDecksData = data;
            }
            if (structureDecksData != null)
            {
                foreach (object obj in structureDecksData.Values)
                {
                    Dictionary<string, object> deckData = obj as Dictionary<string, object>;
                    int id = Utils.GetValue<int>(deckData, "structure_id");
                    if (id > 0)
                    {
                        string path = Path.Combine(targetDir, id + ".json");
                        File.WriteAllText(path, MiniJSON.Json.Serialize(obj));
                    }
                }
            }
        }

        void DumpMissingCards()
        {
            using (TextWriter tw = File.CreateText("missing-cards.txt"))
            {
                tw.WriteLine("The following is a list of cards which are in the data files but not the card list");
                tw.WriteLine("---------------------------------------------------");
                Dictionary<int, YdkHelper.GameCardInfo> cards = YdkHelper.LoadCardDataFromGame(dataDirectory);
                foreach (YdkHelper.GameCardInfo card in cards.Values)
                {
                    if (!CardRare.ContainsKey(card.Id) && card.Kind != CardKind.Token)
                    {
                        tw.WriteLine(card.Id.ToString() + " - " + card.Name);
                    }
                }
            }
        }

        void DumpDuelFieldBgms()
        {
            using (TextWriter tw = File.CreateText("field-bgms.txt"))
            {
                string[] dirs =
                {
                    Path.Combine(dataDirectory, "SoloDuels"),
                    Path.Combine(dataDirectory, "BgmDuelLogs"),
                };
                Dictionary<string, HashSet<int>> bgms = new Dictionary<string, HashSet<int>>();
                foreach (string dir in dirs)
                {
                    if (Directory.Exists(dir))
                    {
                        foreach (string file in Directory.GetFiles(dir, "*.json"))
                        {
                            Dictionary<string, object> data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(file)) as Dictionary<string, object>;
                            data = Utils.GetResData(data);
                            Dictionary<string, object> duelData;
                            if (!Utils.TryGetValue(data, "Duel", out duelData))
                            {
                                continue;
                            }
                            DuelSettings duel = new DuelSettings();
                            duel.FromDictionary(duelData);
                            if (duel.bgms.Count > 0)
                            {
                                string bgm = duel.bgms[0];
                                if (!bgms.ContainsKey(bgm))
                                {
                                    bgms[bgm] = new HashSet<int>();
                                }
                                bgms[bgm].Add(duel.mat[0]);
                            }
                        }
                    }
                }
                foreach (KeyValuePair<string, HashSet<int>> bgm in bgms.OrderBy(x => int.Parse(x.Key.Split('_').Last())))
                {
                    tw.WriteLine(bgm.Key + " - " + string.Join(",", bgm.Value.OrderBy(x => x)));
                }
            }
        }
    }
}
