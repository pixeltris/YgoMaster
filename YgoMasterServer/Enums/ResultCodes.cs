using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    /// <summary>
    /// YgomSystem.Network
    /// </summary>
    class ResultCodes
    {
        public enum ErrorCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            MAINTE = 4,
            MAINTE_FOR_BACK = 5,
            GENERAL = 6,
            GOTO_STORE = 7,
            SECTION_MAINTE = 8,
            ADDITIONAL_DL = 9,
            ACCOUNT_BAN = 10,
        }

        public enum AccountCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            NO_PLATFORM = 1100,
            NO_TOKEN = 1101,
            INVALID_TOKEN = 1102,
            AGREE_MISMATCH = 1104,
            PLATFORM_EXISTING = 1105,
            PFM_INHERIT_SUCCESS = 1140,
            PFM_INHERIT_NOT_REGISTER = 1141,
            PFM_INHERIT_ALREADY_REGISTERED = 1142,
            PFM_INHERIT_ERR_INVALID_PLATFORM = 1143,
            PFM_INHERIT_ERR_SHA_NON_REG = 1144,
            PFM_INHERIT_ERR_INHERIT_FAILED = 1145,
            KID_INHERIT_SUCCESS = 1150,
            KID_INHERIT_NOT_LINKED = 1151,
            KID_INHERIT_LINKED = 1152,
            KID_INHERIT_INHERIT_WAIT = 1153,
            KID_INHERIT_API_NEED_AGREE = 1154,
            KID_INHERIT_API_UNAVAILABLE = 1155,
            KID_INHERIT_NO_DATA = 1156,
            KID_INHERIT_API_FAILED = 1157,
            KID_INHERIT_NONCE_ERR = 1158,
            KID_INHERIT_FAILED = 1159,
            KID_INHERIT_PF_RELATION_FAILED_PS = 1160,
            KID_INHERIT_PF_RELATION_FAILED_NINTENDO = 1161,
            KID_INHERIT_PF_RELATION_FAILED_XBOX = 1162,
            KID_INHERIT_PF_RELATION_FAILED_STEAM = 1163,
            PLATFORM_ERROR = 1170,
            PLATFORM_REAUTH = 1171,
            PLATFORM_REBOOT = 1172,
            ERR_PLATFORM_AUTH_EXPIRED = 1173,
            ERR_PLATFORM_SERVICE_AUTH_EXPIRED = 1174,
            ERR_EXCESSIVE_REPORT = 1180,
        }

        public enum UserCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            NO_PLATFORM = 1200,
            ERR_INVALID_PARAM = 1201,
            ERR_ACCOUNT_NOT_EXIST = 1202,
            ERR_INVALID_VALUE = 1203,
            ERR_DIFF_FLAG = 1204,
            CROSS_PLAY = 1205,
            XBOX_CROSS_PLAY = 1206,
            ERR_NG_WORD = 1207,
            ERR_NAME_LENGTH = 1208,
        }

        public enum DuelCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            TIMEOUT = 1400,
            NOT_FIND_FRIEND = 1401,
            REFUSED_FRIEND = 1402,
            INVALID_DECK = 1403,
        }

        public enum TournamentCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 1500,
            ERR_DECK_CONFIG = 1501,
            ERR_DECK_SAME_CARD = 1502,
            ERR_DECK_REGULATION = 1503,
            ERR_OUT_OF_TERM = 1504,
            ERROR_DECK_LIMIT = 1505,
            ERROR_FIXED_ACCESSORY = 1506,
        }

        public enum ItemCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            LIMIT_NUM = 1600,
            SUB = 1601,
        }

        public enum DeckCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERROR_DECK_CONFIG = 1400,
            ERROR_DECK_LIMIT = 1401,
            ERROR_DECK_REGULATION = 1402,
            ERROR_PARAMS_CONFIG = 1403,
            ERROR_TOURNAMENT_STATUS = 1404,
            ERROR_EXHIBITION_STATUS = 1405,
            ERROR_DECK_NAME_LEN = 1406,
            ERROR_DECK_SAME_CARDS = 1407,
            ERROR_DECK_NO,//v1.3.1
            ERROR_CARD_ID,//v1.3.1
            INVALID_DECK_COUNT = 1440,//v1.3.1
            INVALID_DECK_NAME,//v1.3.1
            INVALID_DECK_BIKO,//v1.3.1
            CGN_ID_NOT_LINKED,//v1.3.1
            OVER_DECK_LIMIT,//v1.3.1
            INVALID_ACCESS,//v1.3.1
            CDB_SERVER_ERROR,//v1.3.1
            KONAMIID_SERVER_ERROR,//v1.3.1
            NEURON_MAINTENANCE,//v1.3.1
        }

        public enum CraftCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERROR_CRAFT_GEN = 1800,
            ERROR_CRAFT_LIMIT = 1801,
            ERROR_UPDATE_FAILED = 1802,
        }

        public enum GachaCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERR_INVALID_PARAM = 1900,
            ERR_INVALID_DRAW_NUM = 1901,
            ERR_OUT_OF_TERM = 1902,
            ERR_MISSING_REQUIRED_ITEMS = 1903,
        }

        public enum FriendCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 2000,
            ACCOUNT_NOT_EXIST = 2001,
            ACCOUNT_OWN = 2002,
            ALREADY_FOLLOWED = 2003,
            NO_FOLLOW_ACCOUNT = 2004,
            FOLLOW_MAX = 2005,
            SAME_VALUE = 2006,
            ALREADY_BLOCKED = 2007,
            NO_BLOCK_ACCOUNT = 2008,
            BLOCK_MAX = 2009,
        }

        public enum MissionCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERROR_INVALID_POOL = 2601,
            ERROR_INVALID_MISSION = 2602,
            ERROR_OUT_OF_TERM_POOL = 2603,
            ERROR_OUT_OF_TERM_MISSION = 2604,
            ERROR_GET_REWARD_MASTER = 2611,
            ERROR_GET_PROGRESS = 2612,
            ERROR_ALREADY_RECEIVE = 2613,
            ERROR_ILLEGAL_RECEIVE = 2614,
            ERROR_ILLEGAL_PROGRESS = 2615,
            ERROR_NON_APPLY_REWARD = 2616,
        }

        public enum PvPCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 2100,
            ALREADY_SAVE_REPLAY = 2101,
            NO_SAVE_REPLAY = 2102,
            ERR_DISABLE_WATCH = 2103,
            ERR_BLOCK_USER_REPLAY = 2106,
            ERR_BLOCK_USER_WATCH = 2107,
            SAVE_REPLAY_MAX = 2108,
            NO_REPLAY_DATA = 2109,
            TIMEOUT = 2110,
            NOT_FIND_FRIEND = 2111,
            REFUSED_FRIEND = 2112,
            INVALID_DECK = 2113,
            INVALID_PERIOD = 2114,
            NOT_FIND_OPPONENT = 2115,
            NOT_EXIST_ROOM = 2116,
            NOT_EXIST_ROOM_OPP = 2117,
            AUDIENCE_LIMIT_MAX = 2118,
            NOT_EXIST_TEAM,//v1.3.1
            INVALID_TEAM_INFO,//v1.3.1
            VS_TEAM_DECIDED,//v1.3.1
            TEAM_MATCHING_CANCELED,//v1.3.1
            VS_TEAM_WAITING,//v1.3.1
        }

        public enum StructureCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 2200,
            ALREADY_FIRST_SET = 2201,
        }

        public enum TeamCode//v1.3.1
        {
            NONE,
            ERROR,
            FATAL,
            CRITICAL,
            ERR_INVALID_PARAM = 3900,
            ERR_INVALID_TEAM,
            ERR_ENTRY_FAILED,
            ERR_INVALID_NEW_TEAM_ID,
            ERR_NO_VACANT_TABLES,
            ERR_INVALID_DECKSET,
            ERR_TEAM_CREATE_FAILED,
            ERR_USER_BLOCK,
            ERR_INVALID_DATA,
            ERR_PLATFORM_AUTH_EXPIRED,
            ERR_ALREADY_ENTRY_TEAM,
            ERR_TEAM_MEMBER_MAX,
            ERR_RIVAL_LEAVE_TABLE,
            ERR_ENTRY_TEAM_CROSS_INVALID,
            ERR_ENTRY_TEAM_CROSS_PF_INVALID,
            ERR_ENTRY_TEAM_CROSS_XB_BOTH_INVALID,
            ERR_ENTRY_TEAM_CROSS_XB_DEVICE_INVALID,
            ERR_ENTRY_TEAM_CROSS_XB_APP_INVALID,
            ERR_ENTRY_TEAM_CROSS_PF_APP_INVALID,
            ERR_ENTRY_TEAM_VALID_CROSS_XB_BOTH_INVALID,
            ERR_ENTRY_TEAM_VALID_CROSS_XB_DEVICE_INVALID,
            ERR_ENTRY_TEAM_VALID_CROSS_XB_APP_INVALID,
            ERR_ENTRY_TEAM_VALID_CROSS_PF_APP_INVALID,
            ERR_DECK_EMPTY,
            ERR_DECK_REG,
            ERR_LIMIT_REGULATION_PERIOD,
            ERR_HOST_CLIENT_VERSION,
            WAITING_OTHER_MEMBER,
            SEARCHING_TEAM,
            MATCHING_TIME_OUT,
            ERR_ALREADY_MEMBER_COMPLETE,
            ERR_KICK_TIMEOUT,
            ERR_KICK_NOT_SEATED,
            ERR_CANNOT_SIT_TABLE,
            ERR_CANNOT_REASON_TEAM_STATUS,
            ERR_TEAM_REMOVED,
            ERR_RIVAL_MATCHING_FAILED,
        }

        public enum ShopCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 2700,
            OUT_OF_TERM = 2701,
            ITEMS_SHORTAGE = 2702,
            LIMIT_MAX = 2703,
            PROCESSING_FAILED = 2704,
        }

        public enum ChallengeCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 2300,
        }

        public enum SoloCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_CHAPTER = 2400,
            LOCKED_CHAPTER = 2401,
            NPC_NOT_EXIST = 2402,
            STORY_DECK_NOT_EXIST = 2403,
            INVALID_GATE = 2405,
        }

        public enum PresentBoxCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERR_INVALID_PARAM = 2500,
        }

        public enum DuelMenuCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
        }

        public enum NotificationCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
        }

        public enum EventNotifyCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
        }

        public enum BillingCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            PURCHASE_FAILED = 2901,
            PURCHASE_FATAL = 2902,
            PURCHASE_CRITICAL = 2903,
            INVALID_RECEIPT = 2904,
            PROCESSED = 2905,
            BILLING_LIMIT = 2906,
            REQUIRE_AGE = 2907,
            REGISTER_AGE_FAILED = 2908,
            UN_COMPLETE_PURCHASE_ITEM = 2909,
            PURCHASE_ITEM_ADD_FAILED = 2910,
            INVALID_RECEIPT_REACCESS = 2911,
            HISTORY_FAILED = 2912,
            PLATFORM_AUTH_EXPIRED = 2913,
            NO_ITEM_PURCHASED = 2914,
            MIIM_MAINTENANCE = 2915,
            DIVISION_MAINTENANCE = 2916,
            DATE_LIMIT_OVER = 2917,
            LIMIT_EXCEEDED = 2918,
            NX_BAASERROR_SERVICE_MAINTENANCE = 2950,
            NX_SUGARERROR_SERVICE_MAINTENANCE = 2951,
        }

        public enum CgdbDeckSearchCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            MAINTE = 4,
            INVALID_USER_TOKEN = 5,//v1.3.1
            ID_NOT_REGISTERD = 6,//v1.3.1
            DECK_NOT_FOUND = 7,//v1.3.1
        }

        public enum RoomCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERR_INVALID_ROOM = 3101,
            ERR_ENTRY_FAILED = 3102,
            ERR_INVALID_NEW_ROOM_ID = 3103,
            ERR_NO_VACANT_TABLES = 3104,
            ERR_INVALID_DECKSET = 3105,
            ERR_ROOM_CREATE_FAILED = 3106,
            ERR_USER_BLOCK = 3107,
            ERR_INVALID_DATA = 3108,
            ERR_PLATFORM_AUTH_EXPIRED = 3109,
            ERR_ALREADY_ENTRY_ROOM = 3110,
            ERR_ROOM_MEMBER_MAX = 3111,
            ERR_RIVAL_LEAVE_TABLE = 3112,
            ERR_ENTRY_ROOM_CROSS_INVALID = 3113,
            ERR_ENTRY_ROOM_CROSS_PF_INVALID = 3114,
            ERR_ENTRY_ROOM_CROSS_XB_BOTH_INVALID = 3115,
            ERR_ENTRY_ROOM_CROSS_XB_DEVICE_INVALID = 3116,
            ERR_ENTRY_ROOM_CROSS_XB_APP_INVALID = 3117,
            ERR_ENTRY_ROOM_CROSS_PF_APP_INVALID = 3118,
            ERR_ENTRY_ROOM_VALID_CROSS_XB_BOTH_INVALID = 3119,
            ERR_ENTRY_ROOM_VALID_CROSS_XB_DEVICE_INVALID = 3120,
            ERR_ENTRY_ROOM_VALID_CROSS_XB_APP_INVALID = 3121,
            ERR_ENTRY_ROOM_VALID_CROSS_PF_APP_INVALID = 3122,
            ERR_DECK_EMPTY = 3123,
            ERR_DECK_REG = 3124,
        }

        public enum ExhibitionCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            INVALID_PARAM = 3000,
            ERR_DECK_CONFIG = 3001,
            ERR_DECK_SAME_CARD = 3002,
            ERR_DECK_REGULATION = 3003,
            ERR_OUT_OF_TERM = 3004,
            ERR_DECK_LIMIT = 3005,
            ERROR_FIXED_ACCESSORY = 3006,
        }

        public enum DuelpassCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERROR_INVALID_MASTER = 3201,
            ERROR_INVALID_TERM = 3202,
            ERROR_INVALID_PARAM = 3203,
        }

        public enum DuelLiveCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            PLAYBACK_FAIL = 3301,
        }

        public enum EnqueteCode
        {
            NONE = 0,
            ERROR = 1,
            FATAL = 2,
            CRITICAL = 3,
            ERROR_INVALID_TERM = 3401,
            ERROR_INVALID_PARAM = 3402,
        }
    }
}
