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
                    new string[] { "zh-TW", "繁體中文" },
                }},
                { "TitleLoop", TitleLoop }
            };

            //request.Remove("Server");// Removed v1.9.0 as this will wipe urls which are set client side
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

        void Act_SystemToggleDuelBgm(GameServerWebRequest request)
        {
            request.Player.DuelBgmMode = request.Player.DuelBgmMode == DuelBgmMode.Myself ? DuelBgmMode.Rival : DuelBgmMode.Myself;
            SavePlayer(request.Player);
            request.Response["Persistence"] = new Dictionary<string, object>()
            {
                { "App", new Dictionary<string, object>() {
                    { "Settings", new Dictionary<string, object>() {
                        { "Duelbgm", (int)request.Player.DuelBgmMode }
                    }}
                }}
            };
        }
    }
}
