using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YgoMaster;

namespace YgoMasterClient
{
    static partial class ClientSettings
    {
        /// <summary>
        /// YgoMaster
        /// </summary>
        public static string CustomTextYgoMaster;
        /// <summary>
        /// Error
        /// </summary>
        public static string CustomTextError;
        /// <summary>
        /// Confirmation
        /// </summary>
        public static string CustomTextConfirmation;
        /// <summary>
        /// Info
        /// </summary>
        public static string CustomTextInfo;
        /// <summary>
        /// OK
        /// </summary>
        public static string CustomTextOK;
        /// <summary>
        /// Default
        /// </summary>
        public static string CustomTextDefault;
        /// <summary>
        /// Random
        /// </summary>
        public static string CustomTextRandom;
        /// <summary>
        /// None
        /// </summary>
        public static string CustomTextNone;
        /// <summary>
        /// An exception occurred\n{exception}
        /// </summary>
        public static string CustomTextExceptionOccurred;
        /// <summary>
        /// Yes
        /// </summary>
        public static string CustomTextYes;
        /// <summary>
        /// No
        /// </summary>
        public static string CustomTextNo;
        /// <summary>
        /// True
        /// </summary>
        public static string CustomTextTrue;
        /// <summary>
        /// False
        /// </summary>
        public static string CustomTextFalse;
        /// <summary>
        /// On
        /// </summary>
        public static string CustomTextOn;
        /// <summary>
        /// Off
        /// </summary>
        public static string CustomTextOff;
        /// <summary>
        /// Duel Starter
        /// </summary>
        public static string CustomTextDuelStarterTitle;
        /// <summary>
        /// Start Duel
        /// </summary>
        public static string CustomTextDuelStarterDuelButton;
        /// <summary>
        /// Duel
        /// </summary>
        public static string CustomTextDuelStarterPveOrPvpTitle;
        /// <summary>
        /// Duel Room or Duel Starter?
        /// </summary>
        public static string CustomTextDuelStarterPveOrPvpText;
        /// <summary>
        /// Duel Room (PvP)
        /// </summary>
        public static string CustomTextDuelStarterPveOrPvpTextBtnPvP;
        /// <summary>
        /// Duel Starter (PvE)
        /// </summary>
        public static string CustomTextDuelStarterPveOrPvpTextBtnPvE;
        /// <summary>
        /// Decks
        /// </summary>
        public static string CustomTextDuelStarterDecks;
        /// <summary>
        /// Deck
        /// </summary>
        public static string CustomTextDuelStarterDeck;
        /// <summary>
        /// Load deck from
        /// </summary>
        public static string CustomTextDuelStarterLoadDeckFrom;
        /// <summary>
        /// Game
        /// </summary>
        public static string CustomTextDuelStarterLoadDeckFromGame;
        /// <summary>
        /// File
        /// </summary>
        public static string CustomTextDuelStarterLoadDeckFromFile;
        /// <summary>
        /// Folder (random deck)
        /// </summary>
        public static string CustomTextDuelStarterLoadDeckFromFolder;
        /// <summary>
        /// Settings
        /// </summary>
        public static string CustomTextDuelStarterSettings;
        /// <summary>
        /// Starting player
        /// </summary>
        public static string CustomTextDuelStarterStartingPlayer;
        /// <summary>
        /// Life points
        /// </summary>
        public static string CustomTextDuelStarterLifePoints;
        /// <summary>
        /// Hand
        /// </summary>
        public static string CustomTextDuelStarterHand;
        /// <summary>
        /// Field
        /// </summary>
        public static string CustomTextDuelStarterField;
        /// <summary>
        /// Field sync
        /// </summary>
        public static string CustomTextDuelStarterFieldSync;
        /// <summary>
        /// Duel type
        /// </summary>
        public static string CustomTextDuelStarterDuelType;
        /// <summary>
        /// Advanced settings
        /// </summary>
        public static string CustomTextDuelStarterAdvancedSettings;
        /// <summary>
        /// Seed
        /// </summary>
        public static string CustomTextDuelStarterSeed;
        /// <summary>
        /// Shuffle
        /// </summary>
        public static string CustomTextDuelStarterShuffle;
        /// <summary>
        /// Cpu
        /// </summary>
        public static string CustomTextDuelStarterCpu;
        /// <summary>
        /// CpuFlag
        /// </summary>
        public static string CustomTextDuelStarterCpuFlag;
        /// <summary>
        /// Limit
        /// </summary>
        public static string CustomTextDuelStarterLimit;
        /// <summary>
        /// BGM
        /// </summary>
        public static string CustomTextDuelStarterBGM;
        /// <summary>
        /// BGM1
        /// </summary>
        public static string CustomTextDuelStarterBGM1;
        /// <summary>
        /// BGM2
        /// </summary>
        public static string CustomTextDuelStarterBGM2;
        /// <summary>
        /// BGM3
        /// </summary>
        public static string CustomTextDuelStarterBGM3;
        /// <summary>
        /// P1
        /// </summary>
        public static string CustomTextDuelStarterP1;
        /// <summary>
        /// Player
        /// </summary>
        public static string CustomTextDuelStarterPlayer;
        /// <summary>
        /// CPU
        /// </summary>
        public static string CustomTextDuelStarterCPU;
        /// <summary>
        /// LP1
        /// </summary>
        public static string CustomTextDuelStarterLP1;
        /// <summary>
        /// LP2
        /// </summary>
        public static string CustomTextDuelStarterLP2;
        /// <summary>
        /// Hand1
        /// </summary>
        public static string CustomTextDuelStarterHand1;
        /// <summary>
        /// Hand2
        /// </summary>
        public static string CustomTextDuelStarterHand2;
        /// <summary>
        /// Sleeve1
        /// </summary>
        public static string CustomTextDuelStarterSleeve1;
        /// <summary>
        /// Sleeve2
        /// </summary>
        public static string CustomTextDuelStarterSleeve2;
        /// <summary>
        /// Field1
        /// </summary>
        public static string CustomTextDuelStarterField1;
        /// <summary>
        /// Field2
        /// </summary>
        public static string CustomTextDuelStarterField2;
        /// <summary>
        /// FieldPart1
        /// </summary>
        public static string CustomTextDuelStarterFieldPart1;
        /// <summary>
        /// FieldPart2
        /// </summary>
        public static string CustomTextDuelStarterFieldPart2;
        /// <summary>
        /// Mate1
        /// </summary>
        public static string CustomTextDuelStarterMate1;
        /// <summary>
        /// Mate2
        /// </summary>
        public static string CustomTextDuelStarterMate2;
        /// <summary>
        /// MateBase1
        /// </summary>
        public static string CustomTextDuelStarterMateBase1;
        /// <summary>
        /// MateBase2
        /// </summary>
        public static string CustomTextDuelStarterMateBase2;
        /// <summary>
        /// Icon1
        /// </summary>
        public static string CustomTextDuelStarterIcon1;
        /// <summary>
        /// Icon2
        /// </summary>
        public static string CustomTextDuelStarterIcon2;
        /// <summary>
        /// IconFrame1
        /// </summary>
        public static string CustomTextDuelStarterIconFrame1;
        /// <summary>
        /// IconFrame2
        /// </summary>
        public static string CustomTextDuelStarterIconFrame2;
        /// <summary>
        /// Coin1
        /// </summary>
        public static string CustomTextDuelStarterCoin1;
        /// <summary>
        /// Coin2
        /// </summary>
        public static string CustomTextDuelStarterCoin2;
        /// <summary>
        /// Load / Save
        /// </summary>
        public static string CustomTextDuelStarterLoadSave;
        /// <summary>
        /// Load (including decks)
        /// </summary>
        public static string CustomTextDuelStarterLoadIncludingDecks;
        /// <summary>
        /// Load
        /// </summary>
        public static string CustomTextDuelStarterLoad;
        /// <summary>
        /// Save
        /// </summary>
        public static string CustomTextDuelStarterSave;
        /// <summary>
        /// Extra
        /// </summary>
        public static string CustomTextDuelStarterExta;
        /// <summary>
        /// Open deck editor
        /// </summary>
        public static string CustomTextDuelStarterOpenDeckEditor;
        /// <summary>
        /// Clear selected decks
        /// </summary>
        public static string CustomTextDuelStarterClearSelectedDecks;
        /// <summary>
        /// P1's duel field
        /// </summary>
        public static string CustomTextDuelStarterP1DuelField;
        /// <summary>
        /// P2's duel field
        /// </summary>
        public static string CustomTextDuelStarterP2DuelField;
        /// <summary>
        /// Load replays
        /// </summary>
        public static string CustomTextHomeSubMenuLoadReplays;
        /// <summary>
        /// Load replays (as opponent)
        /// </summary>
        public static string CustomTextHomeSubMenuLoadReplaysAsOpponent;
        /// <summary>
        /// From clipboard (YDKe / YDK / JSON / TXT)
        /// </summary>
        public static string CustomTextDeckEditLoadFromClipboard;
        /// <summary>
        /// To clipboard (YDKe)
        /// </summary>
        public static string CustomTextDeckEditToClipboard;
        /// <summary>
        /// Load file
        /// </summary>
        public static string CustomTextDeckEditLoadFile;
        /// <summary>
        /// Save file
        /// </summary>
        public static string CustomTextDeckEditSaveFile;
        /// <summary>
        /// Open decks folder
        /// </summary>
        public static string CustomTextDeckEditOpenDecksFolder;
        /// <summary>
        /// Clear deck
        /// </summary>
        public static string CustomTextDeckEditClearDeck;
        /// <summary>
        /// Card collection stats
        /// </summary>
        public static string CustomTextDeckEditCardCollectionStats;
        /// <summary>
        /// {NumOwnedCards} / {NumCards} owned ({NumFilteredCards} filtered)
        /// </summary>
        public static string CustomTextDeckEditBanner;
        /// <summary>
        /// {NumOwnedCards} / {NumCards} owned ({NumFilteredCards} filtered)\n{ActiveCollection}
        /// </summary>
        public static string CustomTextDeckEditBannerTrading;
        /// <summary>
        /// Failed to rows
        /// </summary>
        public static string CustomTextDeckEditTextParseBadRows;
        /// <summary>
        /// Failed to find name / quantity for row
        /// </summary>
        public static string CustomTextDeckEditTextParseBadRow;
        /// <summary>
        /// Failed to find name / quantity columns
        /// </summary>
        public static string CustomTextDeckEditTextParseBadColumns;
        /// <summary>
        /// Failed to copy {numFailed} items. Text has been copied to the clipboard.\n\n{failedSb}
        /// </summary>
        public static string CustomTextDeckEditTextParseFailed;
        /// <summary>
        /// Failed to load card data
        /// </summary>
        public static string CustomTextDeckEditLoadCardDataFailed;
        /// <summary>
        /// Are you sure you want to clear the deck?
        /// </summary>
        public static string CustomTextDeckEditClearDeckConfirm;
        /// <summary>
        /// Failed to find / create decks folder
        /// </summary>
        public static string CustomTextDeckEditFolderNotFound;
        /// <summary>
        /// Card collection info
        /// </summary>
        public static string CustomTextCardStatsTitle;
        /// <summary>
        /// Owned total: 
        /// </summary>
        public static string CustomTextCardStatsOwnedTotal;
        /// <summary>
        /// [Card pool]
        /// </summary>
        public static string CustomTextCardStatsCardPool;
        /// <summary>
        /// Total: 
        /// </summary>
        public static string CustomTextCardStatsTotal;
        /// <summary>
        /// N: 
        /// </summary>
        public static string CustomTextCardStatsN;
        /// <summary>
        /// R: 
        /// </summary>
        public static string CustomTextCardStatsR;
        /// <summary>
        /// SR: 
        /// </summary>
        public static string CustomTextCardStatsSR;
        /// <summary>
        /// UR: 
        /// </summary>
        public static string CustomTextCardStatsUR;
        /// <summary>
        /// [Owned cards (1x)]
        /// </summary>
        public static string CustomTextCardStatsOwned1x;
        /// <summary>
        /// [Owned cards (up to 3x)]
        /// </summary>
        public static string CustomTextCardStatsOwned3x;
        /// <summary>
        /// [Extra cards (over 3x)]
        /// </summary>
        public static string CustomTextCardStatsOwned3xEx;
        /// <summary>
        /// Normal: 
        /// </summary>
        public static string CustomTextCardStatsNormal;
        /// <summary>
        /// Shine: 
        /// </summary>
        public static string CustomTextCardStatsShine;
        /// <summary>
        /// Royal: 
        /// </summary>
        public static string CustomTextCardStatsRoyal;
        /// <summary>
        /// [Rarity (1x)]
        /// </summary>
        public static string CustomTextCardStatsRarity1x;
        /// <summary>
        /// [Rarity (up to 3x)]
        /// </summary>
        public static string CustomTextCardStatsRarity3x;
        /// <summary>
        /// [Rarity (over 3x)]
        /// </summary>
        public static string CustomTextCardStatsRarity3xEx;
        /// <summary>
        /// ======== Index ========\n- \"Card pool\" is all cards in the game\n- \"Owned cards (1x)\" is all cards you own capped to 1x per card\n- \"Owned cards (up to 3x)\" is all cards you own capped to 3x per card\n- \"Extra cards (over 3x)\" is all cards you own over 3x (excludes the first 3x)\n- \"Rarity (1x)\" is all rarities you own capped to 1x per card\n- \"Rarity (up to 3x)\" is all rarities you own capped to 3x per card\n- \"Rarity (over 3x)\" is all rarities you own over 3x (excludes the first 3x)
        /// </summary>
        public static string CustomTextCardStatsIndex;
        /// <summary>
        /// Trade
        /// </summary>
        public static string CustomTextProfileTradeButton;
        /// <summary>
        /// {name} ({code}) is in another trading room
        /// </summary>
        public static string CustomTextTradeBannerAlreadyTrading;
        /// <summary>
        /// {name} ({code}) entered the trading room
        /// </summary>
        public static string CustomTextTradeBannerEnterTrade;
        /// <summary>
        /// {name} ({code}) left the trading room
        /// </summary>
        public static string CustomTextTradeBannerLeaveTrade;
        /// <summary>
        /// My entire collection
        /// </summary>
        public static string CustomtextTradeMyEntireCollection;
        /// <summary>
        /// My tradable collection
        /// </summary>
        public static string CustomtextTradeMyTradableCollection;
        /// <summary>
        /// Their tradable collection
        /// </summary>
        public static string CustomtextTradeTheirEntireCollection;
        /// <summary>
        /// Their tradable collection
        /// </summary>
        public static string CustomtextTradeTheirTradableCollection;
        /// <summary>
        /// Trade
        /// </summary>
        public static string CustomTextTradeDialogTitle;
        /// <summary>
        /// Trade complete
        /// </summary>
        public static string CustomTextTradeComplete;
        /// <summary>
        /// Trade failed
        /// </summary>
        public static string CustomTextTradeFailed;
        /// <summary>
        /// Waiting...
        /// </summary>
        public static string CustomTextTradeWaiting;
        /// <summary>
        /// Wait
        /// </summary>
        public static string CustomTextTradeWait;
        /// <summary>
        /// Cancel
        /// </summary>
        public static string CustomTextTradeCancel;
        /// <summary>
        /// Trade
        /// </summary>
        public static string CustomTextTradeCanTrade;
        /// <summary>
        /// Trade!
        /// </summary>
        public static string CustomTextTradeCanTradePartnerReady;
        /// <summary>
        /// {name} ({code}) sent you a duel invite
        /// </summary>
        public static string CustomTextDuelInvite;
        /// <summary>
        /// Emotes
        /// </summary>
        public static string CustomTextEmotesListHeader;
        /// <summary>
        /// How to fix this error:\ngithub.com/pixeltris/YgoMaster/blob/master/Docs/FileLoadError.md\n\n
        /// </summary>
        public static string CustomTextFileLoadErrorEx;
        /// <summary>
        /// Next
        /// </summary>
        public static string CustomTextVisualNovelNext;
        /// <summary>
        /// Skip
        /// </summary>
        public static string CustomTextVisualNovelSkip;

        static bool LoadText()
        {
            Dictionary<string, object> data = null;
            string path = Path.Combine(Program.ClientDataDir, ClientSettingsTextFile);
            try
            {
                data = MiniJSON.Json.DeserializeStripped(File.ReadAllText(path)) as Dictionary<string, object>;
                if (data == null)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            CustomTextYgoMaster = Utils.GetValue<string>(data, "CustomTextYgoMaster");
            CustomTextError = Utils.GetValue<string>(data, "CustomTextError");
            CustomTextConfirmation = Utils.GetValue<string>(data, "CustomTextConfirmation");
            CustomTextInfo = Utils.GetValue<string>(data, "CustomTextInfo");
            CustomTextOK = Utils.GetValue<string>(data, "CustomTextOK");
            CustomTextDefault = Utils.GetValue<string>(data, "CustomTextDefault");
            CustomTextRandom = Utils.GetValue<string>(data, "CustomTextRandom");
            CustomTextNone = Utils.GetValue<string>(data, "CustomTextNone");
            CustomTextExceptionOccurred = Utils.GetValue<string>(data, "CustomTextExceptionOccurred");
            CustomTextYes = Utils.GetValue<string>(data, "CustomTextYes");
            CustomTextNo = Utils.GetValue<string>(data, "CustomTextNo");
            CustomTextTrue = Utils.GetValue<string>(data, "CustomTextTrue");
            CustomTextFalse = Utils.GetValue<string>(data, "CustomTextFalse");
            CustomTextOn = Utils.GetValue<string>(data, "CustomTextOn");
            CustomTextOff = Utils.GetValue<string>(data, "CustomTextOff");

            CustomTextDuelStarterTitle = Utils.GetValue<string>(data, "CustomTextDuelStarterTitle");
            CustomTextDuelStarterDuelButton = Utils.GetValue<string>(data, "CustomTextDuelStarterDuelButton");
            CustomTextDuelStarterPveOrPvpTitle = Utils.GetValue<string>(data, "CustomTextDuelStarterPveOrPvpTitle");
            CustomTextDuelStarterPveOrPvpText = Utils.GetValue<string>(data, "CustomTextDuelStarterPveOrPvpText");
            CustomTextDuelStarterPveOrPvpTextBtnPvP = Utils.GetValue<string>(data, "CustomTextDuelStarterPveOrPvpTextBtnPvP");
            CustomTextDuelStarterPveOrPvpTextBtnPvE = Utils.GetValue<string>(data, "CustomTextDuelStarterPveOrPvpTextBtnPvE");
            CustomTextDuelStarterDecks = Utils.GetValue<string>(data, "CustomTextDuelStarterDecks");
            CustomTextDuelStarterDeck = Utils.GetValue<string>(data, "CustomTextDuelStarterDeck");
            CustomTextDuelStarterLoadDeckFrom = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadDeckFrom");
            CustomTextDuelStarterLoadDeckFromGame = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadDeckFromGame");
            CustomTextDuelStarterLoadDeckFromFile = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadDeckFromFile");
            CustomTextDuelStarterLoadDeckFromFolder = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadDeckFromFolder");
            CustomTextDuelStarterSettings = Utils.GetValue<string>(data, "CustomTextDuelStarterSettings");
            CustomTextDuelStarterStartingPlayer = Utils.GetValue<string>(data, "CustomTextDuelStarterStartingPlayer");
            CustomTextDuelStarterLifePoints = Utils.GetValue<string>(data, "CustomTextDuelStarterLifePoints");
            CustomTextDuelStarterHand = Utils.GetValue<string>(data, "CustomTextDuelStarterHand");
            CustomTextDuelStarterField = Utils.GetValue<string>(data, "CustomTextDuelStarterField");
            CustomTextDuelStarterFieldSync = Utils.GetValue<string>(data, "CustomTextDuelStarterFieldSync");
            CustomTextDuelStarterDuelType = Utils.GetValue<string>(data, "CustomTextDuelStarterDuelType");
            CustomTextDuelStarterAdvancedSettings = Utils.GetValue<string>(data, "CustomTextDuelStarterAdvancedSettings");
            CustomTextDuelStarterSeed = Utils.GetValue<string>(data, "CustomTextDuelStarterSeed");
            CustomTextDuelStarterShuffle = Utils.GetValue<string>(data, "CustomTextDuelStarterShuffle");
            CustomTextDuelStarterCpu = Utils.GetValue<string>(data, "CustomTextDuelStarterCpu");
            CustomTextDuelStarterCpuFlag = Utils.GetValue<string>(data, "CustomTextDuelStarterCpuFlag");
            CustomTextDuelStarterLimit = Utils.GetValue<string>(data, "CustomTextDuelStarterLimit");
            CustomTextDuelStarterBGM = Utils.GetValue<string>(data, "CustomTextDuelStarterBGM");
            CustomTextDuelStarterBGM1 = Utils.GetValue<string>(data, "CustomTextDuelStarterBGM1");
            CustomTextDuelStarterBGM2 = Utils.GetValue<string>(data, "CustomTextDuelStarterBGM2");
            CustomTextDuelStarterBGM3 = Utils.GetValue<string>(data, "CustomTextDuelStarterBGM3");
            CustomTextDuelStarterP1 = Utils.GetValue<string>(data, "CustomTextDuelStarterP1");
            CustomTextDuelStarterPlayer = Utils.GetValue<string>(data, "CustomTextDuelStarterPlayer");
            CustomTextDuelStarterCPU = Utils.GetValue<string>(data, "CustomTextDuelStarterCPU");
            CustomTextDuelStarterLP1 = Utils.GetValue<string>(data, "CustomTextDuelStarterLP1");
            CustomTextDuelStarterLP2 = Utils.GetValue<string>(data, "CustomTextDuelStarterLP2");
            CustomTextDuelStarterHand1 = Utils.GetValue<string>(data, "CustomTextDuelStarterHand1");
            CustomTextDuelStarterHand2 = Utils.GetValue<string>(data, "CustomTextDuelStarterHand2");
            CustomTextDuelStarterSleeve1 = Utils.GetValue<string>(data, "CustomTextDuelStarterSleeve1");
            CustomTextDuelStarterSleeve2 = Utils.GetValue<string>(data, "CustomTextDuelStarterSleeve2");
            CustomTextDuelStarterField1 = Utils.GetValue<string>(data, "CustomTextDuelStarterField1");
            CustomTextDuelStarterField2 = Utils.GetValue<string>(data, "CustomTextDuelStarterField2");
            CustomTextDuelStarterFieldPart1 = Utils.GetValue<string>(data, "CustomTextDuelStarterFieldPart1");
            CustomTextDuelStarterFieldPart2 = Utils.GetValue<string>(data, "CustomTextDuelStarterFieldPart2");
            CustomTextDuelStarterMate1 = Utils.GetValue<string>(data, "CustomTextDuelStarterMate1");
            CustomTextDuelStarterMate2 = Utils.GetValue<string>(data, "CustomTextDuelStarterMate2");
            CustomTextDuelStarterMateBase1 = Utils.GetValue<string>(data, "CustomTextDuelStarterMateBase1");
            CustomTextDuelStarterMateBase2 = Utils.GetValue<string>(data, "CustomTextDuelStarterMateBase2");
            CustomTextDuelStarterIcon1 = Utils.GetValue<string>(data, "CustomTextDuelStarterIcon1");
            CustomTextDuelStarterIcon2 = Utils.GetValue<string>(data, "CustomTextDuelStarterIcon2");
            CustomTextDuelStarterIconFrame1 = Utils.GetValue<string>(data, "CustomTextDuelStarterIconFrame1");
            CustomTextDuelStarterIconFrame2 = Utils.GetValue<string>(data, "CustomTextDuelStarterIconFrame2");
            CustomTextDuelStarterCoin1 = Utils.GetValue<string>(data, "CustomTextDuelStarterCoin1");
            CustomTextDuelStarterCoin2 = Utils.GetValue<string>(data, "CustomTextDuelStarterCoin2");
            CustomTextDuelStarterLoadSave = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadSave");
            CustomTextDuelStarterLoadIncludingDecks = Utils.GetValue<string>(data, "CustomTextDuelStarterLoadIncludingDecks");
            CustomTextDuelStarterLoad = Utils.GetValue<string>(data, "CustomTextDuelStarterLoad");
            CustomTextDuelStarterSave = Utils.GetValue<string>(data, "CustomTextDuelStarterSave");
            CustomTextDuelStarterExta = Utils.GetValue<string>(data, "CustomTextDuelStarterExta");
            CustomTextDuelStarterOpenDeckEditor = Utils.GetValue<string>(data, "CustomTextDuelStarterOpenDeckEditor");
            CustomTextDuelStarterClearSelectedDecks = Utils.GetValue<string>(data, "CustomTextDuelStarterClearSelectedDecks");
            CustomTextDuelStarterP1DuelField = Utils.GetValue<string>(data, "CustomTextDuelStarterP1DuelField");
            CustomTextDuelStarterP2DuelField = Utils.GetValue<string>(data, "CustomTextDuelStarterP2DuelField");

            CustomTextHomeSubMenuLoadReplays = Utils.GetValue<string>(data, "CustomTextHomeSubMenuLoadReplays");
            CustomTextHomeSubMenuLoadReplaysAsOpponent = Utils.GetValue<string>(data, "CustomTextHomeSubMenuLoadReplaysAsOpponent");

            CustomTextDeckEditLoadFromClipboard = Utils.GetValue<string>(data, "CustomTextDeckEditLoadFromClipboard");
            CustomTextDeckEditToClipboard = Utils.GetValue<string>(data, "CustomTextDeckEditToClipboard");
            CustomTextDeckEditLoadFile = Utils.GetValue<string>(data, "CustomTextDeckEditLoadFile");
            CustomTextDeckEditSaveFile = Utils.GetValue<string>(data, "CustomTextDeckEditSaveFile");
            CustomTextDeckEditOpenDecksFolder = Utils.GetValue<string>(data, "CustomTextDeckEditOpenDecksFolder");
            CustomTextDeckEditClearDeck = Utils.GetValue<string>(data, "CustomTextDeckEditClearDeck");
            CustomTextDeckEditCardCollectionStats = Utils.GetValue<string>(data, "CustomTextDeckEditCardCollectionStats");
            CustomTextDeckEditBanner = Utils.GetValue<string>(data, "CustomTextDeckEditBanner");
            CustomTextDeckEditBannerTrading = Utils.GetValue<string>(data, "CustomTextDeckEditBannerTrading");
            CustomTextDeckEditTextParseBadRows = Utils.GetValue<string>(data, "CustomTextDeckEditTextParseBadRows");
            CustomTextDeckEditTextParseBadRow = Utils.GetValue<string>(data, "CustomTextDeckEditTextParseBadRow");
            CustomTextDeckEditTextParseBadColumns = Utils.GetValue<string>(data, "CustomTextDeckEditTextParseBadColumns");
            CustomTextDeckEditTextParseFailed = Utils.GetValue<string>(data, "CustomTextDeckEditTextParseFailed");
            CustomTextDeckEditLoadCardDataFailed = Utils.GetValue<string>(data, "CustomTextDeckEditLoadCardDataFailed");
            CustomTextDeckEditClearDeckConfirm = Utils.GetValue<string>(data, "CustomTextDeckEditClearDeckConfirm");
            CustomTextDeckEditFolderNotFound = Utils.GetValue<string>(data, "CustomTextDeckEditFolderNotFound");
            
            CustomTextCardStatsTitle = Utils.GetValue<string>(data, "CustomTextCardStatsTitle");
            CustomTextCardStatsOwnedTotal = Utils.GetValue<string>(data, "CustomTextCardStatsOwnedTotal");
            CustomTextCardStatsCardPool = Utils.GetValue<string>(data, "CustomTextCardStatsCardPool");
            CustomTextCardStatsTotal = Utils.GetValue<string>(data, "CustomTextCardStatsTotal");
            CustomTextCardStatsN = Utils.GetValue<string>(data, "CustomTextCardStatsN");
            CustomTextCardStatsR = Utils.GetValue<string>(data, "CustomTextCardStatsR");
            CustomTextCardStatsSR = Utils.GetValue<string>(data, "CustomTextCardStatsSR");
            CustomTextCardStatsUR = Utils.GetValue<string>(data, "CustomTextCardStatsUR");
            CustomTextCardStatsOwned1x = Utils.GetValue<string>(data, "CustomTextCardStatsOwned1x");
            CustomTextCardStatsOwned3x = Utils.GetValue<string>(data, "CustomTextCardStatsOwned3x");
            CustomTextCardStatsOwned3xEx = Utils.GetValue<string>(data, "CustomTextCardStatsOwned3xEx");
            CustomTextCardStatsNormal = Utils.GetValue<string>(data, "CustomTextCardStatsNormal");
            CustomTextCardStatsShine = Utils.GetValue<string>(data, "CustomTextCardStatsShine");
            CustomTextCardStatsRoyal = Utils.GetValue<string>(data, "CustomTextCardStatsRoyal");
            CustomTextCardStatsRarity1x = Utils.GetValue<string>(data, "CustomTextCardStatsRarity1x");
            CustomTextCardStatsRarity3x = Utils.GetValue<string>(data, "CustomTextCardStatsRarity3x");
            CustomTextCardStatsRarity3xEx = Utils.GetValue<string>(data, "CustomTextCardStatsRarity3xEx");
            CustomTextCardStatsIndex = Utils.GetValue<string>(data, "CustomTextCardStatsIndex");
            
            CustomTextProfileTradeButton = Utils.GetValue<string>(data, "CustomTextProfileTradeButton");
            CustomTextTradeBannerAlreadyTrading = Utils.GetValue<string>(data, "CustomTextTradeBannerAlreadyTrading");
            CustomTextTradeBannerEnterTrade = Utils.GetValue<string>(data, "CustomTextTradeBannerEnterTrade");
            CustomTextTradeBannerLeaveTrade = Utils.GetValue<string>(data, "CustomTextTradeBannerLeaveTrade");
            CustomtextTradeMyEntireCollection = Utils.GetValue<string>(data, "CustomtextTradeMyEntireCollection");
            CustomtextTradeMyTradableCollection = Utils.GetValue<string>(data, "CustomtextTradeMyTradableCollection");
            CustomtextTradeTheirEntireCollection = Utils.GetValue<string>(data, "CustomtextTradeTheirEntireCollection");
            CustomtextTradeTheirTradableCollection = Utils.GetValue<string>(data, "CustomtextTradeTheirTradableCollection");
            CustomTextTradeDialogTitle = Utils.GetValue<string>(data, "CustomTextTradeDialogTitle");
            CustomTextTradeComplete = Utils.GetValue<string>(data, "CustomTextTradeComplete");
            CustomTextTradeFailed = Utils.GetValue<string>(data, "CustomTextTradeFailed");
            CustomTextTradeWaiting = Utils.GetValue<string>(data, "CustomTextTradeWaiting");
            CustomTextTradeWait = Utils.GetValue<string>(data, "CustomTextTradeWait");
            CustomTextTradeCancel = Utils.GetValue<string>(data, "CustomTextTradeCancel");
            CustomTextTradeCanTrade = Utils.GetValue<string>(data, "CustomTextTradeCanTrade");
            CustomTextTradeCanTradePartnerReady = Utils.GetValue<string>(data, "CustomTextTradeCanTradePartnerReady");

            CustomTextDuelInvite = Utils.GetValue<string>(data, "CustomTextDuelInvite");

            CustomTextEmotesListHeader = Utils.GetValue<string>(data, "CustomTextEmotesListHeader");

            DuelSettings.DefaultNamePlayer = Utils.GetValue<string>(data, "CustomTextDuelSettingsDefaultNamePlayer");
            DuelSettings.DefaultNameCPU = Utils.GetValue<string>(data, "CustomTextDuelSettingsDefaultNameCPU");

            CustomTextFileLoadErrorEx = Utils.GetValue<string>(data, "CustomTextFileLoadErrorEx", "How to fix this error:\ngithub.com/pixeltris/YgoMaster/blob/master/Docs/FileLoadError.md\n\n");

            CustomTextVisualNovelNext = Utils.GetValue<string>(data, "CustomTextVisualNovelNext");
            CustomTextVisualNovelSkip = Utils.GetValue<string>(data, "CustomTextVisualNovelSkip");

            return true;
        }
    }
}
