using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YgoMaster
{
    partial class GameServer
    {
        void Act_AccountAuth(GameServerWebRequest request)
        {
            request.Player.LoginTime = DateTime.UtcNow;

            WriteToken(request);
            request.Response["Server"] = new Dictionary<string, object>()
            {
                { "urls_deck_ext", new Dictionary<string, object>() {
                    { "debug", false },
                    { "com_prefix", "data.ycdb" },
                    { "Cgdb.deck_search", new Dictionary<string, object>() {
                        { "cn", "searchDeckForMd" },
                        { "url", deckSearchUrl }
                    }},
                    { "Cgdb.deck_search_detail", new Dictionary<string, object>() {
                        { "cn", "getDeckDetailForMd" },
                        { "url", deckSearchDetailUrl }
                    }},
                    { "Cgdb.deck_search_init", new Dictionary<string, object>() {
                        { "cn", "getDeckAttributesForMd" },
                        { "url", deckSearchAttributesUrl }
                    }},
                }}
            };
            request.Remove(
                "Persistence.System.token",
                "Server.Agreement",
                "Persistence.System.optout",
                "Master",
                "User",
                "Deck",
                "Tournament",
                "Response");
        }

        void Act_SystemInfo(GameServerWebRequest request)
        {
            Version clientVersion;
            if (!string.IsNullOrEmpty(request.ClientVersion) && Version.TryParse(request.ClientVersion, out clientVersion) &&
                clientVersion > highestSupportedClientVersion)
            {
                request.Response["Operation"] = new Dictionary<string, object>()
                {
                    { "Dialog", new Dictionary<string, object>() {
                        { "title", "Unsupported client version" },
                        { "buttonLabelTid", "IDS_SYS_OK" },
                        { "lowerMessage", "Client version: " + request.ClientVersion + "\nHighest supported version: " + highestSupportedClientVersion },
                        { "upperMessage", "Unsupported client version " + request.ClientVersion }
                    }}
                };
                request.ResultCode = (int)ResultCodes.ErrorCode.MAINTE;
                request.Remove("Server", "Response", "Download", "Operation.Dialog");
                return;
            }

            request.Player.Lang = Utils.GetValue<string>(request.ActParams, "lang");
            if (!string.IsNullOrEmpty(request.Player.Lang))
            {
                // Only using this for topic text which uses underscores
                request.Player.Lang = request.Player.Lang.Replace("-", "_");
            }

            WriteToken(request);

            request.Response["Server"] = new Dictionary<string, object>()
            {
                { "status", (int)ServerStatus.NORMAL },
                { "pvp", true },
                { "inherit", true },
                { "debug_tool", false },
                { "langs", new List<object>() {
                    new string[] { "en-US", "English" },
                    new string[] { "fr-FR", "Français" },
                    new string[] { "it-IT", "Italiano" },
                    new string[] { "de-DE", "Deutsch" },
                    new string[] { "es-ES", "Español" },
                    new string[] { "pt-BR", "Português" },
                    new string[] { "ja-JP", "日本語" },
                    new string[] { "ko-KR", "한국어" },
                    new string[] { "zh-CN", "简体中文" },
                }},
                { "TitleLoop", new Dictionary<string, object>()
                    {
                        { "logos", new List<object>(){
                            new Dictionary<string, object>(){
                                { "p", "Images/SplashLogo/OCG25thLogo" },
                                { "bg", "000000" }
                            }
                        }},
                        { "scenarios", new List<string>(){
                            "00020004", "00030009", "00040015", "00050019", "00060023", "00070035", "00080041", "00090047", "00100053", "00110059", "00120065", "00130071", "00140077", "00150083", "00160089", "00170095", "00190107", "00240192", "00270215", "00280223", "00320255", "00350280", "00360287", "00370294", "00400319", "00410326", "00430340", "00470378"
                        }},
                        { "wp_allow_list", new List<int>(){
                            1130001, 1130002, 1130003, 1130004, 1130005, 1130006, 1130007, 1130008, 1130009, 1130010, 1130011, 1130012, 1130013, 1130014, 1130015, 1130016, 1130017, 1130018, 1130019, 1130020, 1130023, 1130024
                        }}
                    }
                }
            };

            request.Remove("Server");
            request.Remove("Response");
            request.Remove("Download");
        }

        void Act_SystemSetLanguage(GameServerWebRequest request)
        {
            string lang;
            if (Utils.TryGetValue(request.ActParams, "lang", out lang))
            {
                request.Response["Persistence"] = new Dictionary<string, object>()
                {
                    { "System", new Dictionary<string, object>() {
                        { "lang", lang }
                    }}
                };
            }
        }
    }
}
