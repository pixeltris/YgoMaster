using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace YgoMaster
{
    partial class GameServer
    {
        Thread thread;
        HttpListener listener;

        public void Start()
        {
            try
            {
                LoadSettings();
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Loading data threw an exception" + Environment.NewLine + e.ToString());
                return;
            }

            thread = new Thread(delegate()
                {
                    listener = new HttpListener();
                    try
                    {
                        listener.Prefixes.Add(bindIP);
                        listener.Start();
                    }
                    catch
                    {
                        Console.WriteLine("[ERROR] Failed to bind to " + bindIP + " (ensure the program isn't already running, or try running as admin)");
                        return;
                    }
                    Console.WriteLine("Initialized");
                    while (listener != null)
                    {
                        try
                        {
                            HttpListenerContext context = listener.GetContext();
                            Process(context);
                        }
                        catch
                        {
                        }
                    }
                });
            thread.CurrentCulture = CultureInfo.InvariantCulture;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        void Process(HttpListenerContext context)
        {
            byte[] requestBuffer = null;
            string actsHeader = null;

            try
            {
                string url = context.Request.Url.OriginalString;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // For encoding / decoding packets see:
                // - YgomSystem.Network.FormatYgom - Serialize()/Deserialize()
                // These classes are similar but unused:
                // - YgomSystem.Network.FormatMsgPack - Serialize()/Deserialize()
                // - YgomSystem.Network.FormatJson - Serialize()/Deserialize()
                //
                // For sending see:
                // - YgomSystem.Network.Request.Entry()
                // - YgomSystem.Network.NetworkMain.Entry()
                // - YgomSystem.Network.ProtocolHttp.Exec()
                //
                // For acts see:
                // - YgomSystem.Network.API
                //
                // For ResultCode values see YgomSystem.Network.XXXXXCode (different enum type for each act)

                long maxContentLength = ushort.MaxValue;

                actsHeader = context.Request.Headers["x_acts"];
                Utils.LogInfo("Req " + actsHeader);
                if (context.Request.ContentLength64 > maxContentLength)
                {
                    Utils.LogWarning("Buffer too long. MaxLen:" + maxContentLength + " len:" + context.Request.ContentLength64);
                }
                if (!string.IsNullOrEmpty(actsHeader) && context.Request.ContentLength64 <= maxContentLength)
                {
                    requestBuffer = new byte[context.Request.ContentLength64];
                    int readBytes = context.Request.InputStream.Read(requestBuffer, 0, requestBuffer.Length);
                    if (readBytes == requestBuffer.Length)
                    {
                        string sessionToken;
                        Dictionary<string, object> vals = Deserialize(requestBuffer, out sessionToken);

                        List<object> actsList = null;
                        Dictionary<string, object> actInfo;
                        int actId;
                        string actName;
                        if (vals != null && Utils.TryGetValue(vals, "acts", out actsList) && actsList.Count > 0 &&
                            (actInfo = actsList[0] as Dictionary<string, object>) != null && Utils.TryGetValue(actInfo, "act", out actName) &&
                            Utils.TryGetValue(actInfo, "id", out actId))
                        {
                            GameServerWebRequest gameServerWebRequest = new GameServerWebRequest();
                            gameServerWebRequest.ActName = actName;
                            Utils.TryGetValue(actInfo, "params", out gameServerWebRequest.ActParams);
                            Utils.TryGetValue(vals, "v", out gameServerWebRequest.ClientVersion);
                            gameServerWebRequest.Response = new Dictionary<string, object>();

                            if (MultiplayerEnabled)
                            {
                                lock (playersByToken)
                                {
                                    if (string.IsNullOrEmpty(sessionToken) || !playersByToken.TryGetValue(sessionToken, out gameServerWebRequest.Player))
                                    {
                                        if (string.IsNullOrEmpty(sessionToken))
                                        {
                                            while (true)
                                            {
                                                sessionToken = Guid.NewGuid().ToString();
                                                if (!playersByToken.ContainsKey(sessionToken))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        playersByToken[sessionToken] = gameServerWebRequest.Player = new Player(nextPlayerCode++);
                                        LoadPlayer(gameServerWebRequest.Player);
                                        WriteToken(gameServerWebRequest);
                                    }
                                }
                            }
                            else
                            {
                                if (localPlayer == null)
                                {
                                    localPlayer = new Player(1111111111);
                                    LoadPlayer(localPlayer);
                                }
                                gameServerWebRequest.Player = localPlayer;
                            }

                            if (gameServerWebRequest.Player != null)
                            {
                                switch (actName)
                                {
                                    case "System.info":
                                        Act_SystemInfo(gameServerWebRequest);
                                        break;
                                    case "System.set_language":
                                        Act_SystemSetLanguage(gameServerWebRequest);
                                        break;
                                    case "Account.auth":
                                        Act_AccountAuth(gameServerWebRequest);
                                        break;
                                    case "User.entry":
                                        Act_UserEntry(gameServerWebRequest);
                                        break;
                                    case "User.home":
                                        Act_UserHome(gameServerWebRequest);
                                        break;
                                    case "User.name_entry":
                                        Act_UserNameEntry(gameServerWebRequest);
                                        break;
                                    case "User.set_profile":
                                        Act_UserSetProfile(gameServerWebRequest);
                                        break;
                                    case "EventNotify.get_list":
                                        Act_EventNotifyGetList(gameServerWebRequest);
                                        break;
                                    case "Deck.SetFavoriteCards":
                                        Act_DeckSetFavoriteCards(gameServerWebRequest);
                                        break;
                                    case "Deck.update_deck":// Unused sinc v1.2.0?
                                        Act_DeckUpdate(gameServerWebRequest);
                                        break;
                                    case "Deck.update_deck_reg":// Added v1.2.0
                                        Act_DeckUpdate(gameServerWebRequest);
                                        break;
                                    case "Deck.get_deck_list":
                                        Act_DeckGetDeckList(gameServerWebRequest);
                                        break;
                                    case "Deck.set_deck_accessory":
                                        Act_DeckSetDeckAccessory(gameServerWebRequest);
                                        break;
                                    case "Deck.delete_deck":
                                        Act_DeckDeleteDeck(gameServerWebRequest);
                                        break;
                                    case "Deck.delete_deck_multi":
                                        Act_DeckDeleteDeckMulti(gameServerWebRequest);
                                        break;
                                    case "Deck.set_select_deck":
                                        Act_SetSelectDeck(gameServerWebRequest);
                                        break;
                                    case "Shop.get_list":
                                        Act_ShopGetList(gameServerWebRequest);
                                        break;
                                    case "Shop.purchase":
                                        Act_ShopPurchase(gameServerWebRequest);
                                        break;
                                    case "Gacha.get_card_list":
                                        Act_GachaGetCardList(gameServerWebRequest);
                                        break;
                                    case "Gacha.get_probability":
                                        Act_GachaGetProbability(gameServerWebRequest);
                                        break;
                                    case "Craft.exchange_multi":
                                        Act_CraftExchangeMulti(gameServerWebRequest);
                                        break;
                                    case "Craft.generate_multi":
                                        Act_CraftGenerateMulti(gameServerWebRequest);
                                        break;
                                    case "Craft.get_card_route":
                                        Act_CraftGetCardRoute(gameServerWebRequest);
                                        break;
                                    case "Solo.info":
                                        Act_SoloInfo(gameServerWebRequest);
                                        break;
                                    case "Solo.detail":
                                        Act_SoloDetail(gameServerWebRequest);
                                        break;
                                    case "Solo.set_use_deck_type":
                                        Act_SoloSetUseDeckType(gameServerWebRequest);
                                        break;
                                    case "Solo.deck_check":
                                        Act_SoloDeckCheck(gameServerWebRequest);
                                        break;
                                    case "Solo.skip":
                                        Act_SoloSkip(gameServerWebRequest);
                                        break;
                                    case "Solo.start":
                                        Act_SoloStart(gameServerWebRequest);
                                        break;
                                    case "Duel.begin":
                                        Act_DuelBegin(gameServerWebRequest);
                                        break;
                                    case "Duel.end":
                                        Act_DuelEnd(gameServerWebRequest);
                                        break;
                                    default:
                                        Utils.LogInfo("Unhandled act " + actsHeader);
                                        Debug.WriteLine("Unhandled act " + actsHeader + " " + MiniJSON.Json.Serialize(vals));
                                        break;
                                }

                                if (gameServerWebRequest.Player.RequiresSaving)
                                {
                                    SavePlayerNow(gameServerWebRequest.Player);
                                }

                                string jsonResponse = MiniJSON.Json.Serialize(gameServerWebRequest.Response);
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append(@"{""code"":" + gameServerWebRequest.ErrorCode + @",""res"":[[" + actId + "," +
                                    jsonResponse + "," + gameServerWebRequest.ResultCode + ",0]]");
                                if (gameServerWebRequest.RemoveList != null && gameServerWebRequest.RemoveList.Count > 0)
                                {
                                    stringBuilder.Append(",\"remove\":" + MiniJSON.Json.Serialize(gameServerWebRequest.RemoveList.ToArray()));
                                }
                                if (gameServerWebRequest.Keep != 0)
                                {
                                    switch (gameServerWebRequest.Keep)
                                    {
                                        case 0:
                                            stringBuilder.Append(@",""keep"":""all""");
                                            break;
                                        case 1:
                                            stringBuilder.Append(@",""keep"":""update""");
                                            break;
                                        case 2:
                                            stringBuilder.Append(@",""keep"":""remove""");
                                            break;
                                    }
                                    if (gameServerWebRequest.AddKeep)
                                    {
                                        stringBuilder.Append(@",""addkeep"":true");
                                    }
                                }
                                if (gameServerWebRequest.Commit)
                                {
                                    stringBuilder.Append(@",""commit"":true");
                                }
                                stringBuilder.Append("}");

                                if (!string.IsNullOrEmpty(gameServerWebRequest.StringResponse))
                                {
                                    stringBuilder.Length = 0;
                                    stringBuilder.Append(gameServerWebRequest.StringResponse);
                                }
                                //Debug.WriteLine(stringBuilder.ToString());

                                byte[] responseBuffer = Serialize(stringBuilder.ToString());
                                context.Response.Headers[HttpResponseHeader.ContentType] = "application/octet-stream";
                                context.Response.ContentLength64 = responseBuffer.Length;
                                context.Response.StatusCode = (int)HttpStatusCode.OK;
                                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                            }
                        }

                        if (actsList != null && actsList.Count > 1)
                        {
                            throw new Exception("TODO: Hande multiple acts in the same message");
                        }
                    }
                    else
                    {
                        if (context.Request.InputStream.Read(requestBuffer, 0, 1) != 0)
                        {
                            throw new Exception("TODO: A proper chunked reader");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                string requestToken = null;
                if (string.IsNullOrEmpty(actsHeader))
                {
                    actsHeader = "(null)";
                }
                string requestString = "(null)";
                if (requestBuffer != null)
                {
                    try
                    {
                        requestString = GetRequestString(requestBuffer, out requestToken);
                    }
                    catch
                    {
                    }
                }
                if (string.IsNullOrEmpty(requestToken))
                {
                    requestToken = "(null)";
                }
                if (string.IsNullOrEmpty(requestString))
                {
                    requestString = "(null)";
                }
                string errorMsg = "Exception when processing message. Exception: " + e + Environment.NewLine +
                    " Token: " + requestToken + Environment.NewLine + "Request: " + requestString;
                Utils.LogWarning(errorMsg);
                Debug.WriteLine(errorMsg);
            }
            finally
            {
                context.Response.Close();
            }
        }

        static string GetRequestString(byte[] buffer, out string token)
        {
            ushort tokenLength = BitConverter.ToUInt16(buffer, 0);
            ushort dataLength = BitConverter.ToUInt16(buffer, 2);
            byte[] tokenBuffer = new byte[tokenLength];
            Buffer.BlockCopy(buffer, 4, tokenBuffer, 0, tokenBuffer.Length);
            token = Convert.ToBase64String(tokenBuffer);
            //token = Encoding.UTF8.GetString(buffer, 4, tokenLength);
            return Encoding.UTF8.GetString(buffer, tokenLength + 4, dataLength);
        }

        static Dictionary<string, object> Deserialize(byte[] buffer, out string token)
        {
            string json = GetRequestString(buffer, out token);
            return MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
        }

        static byte[] Serialize(string value)
        {
            object o = MiniJSON.Json.Deserialize(value) as Dictionary<string, object>;
            byte[] packed = LZ4.Compress(MessagePack.Pack(o));
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write((byte)1);
                bw.Write((byte)2);
                bw.Write((byte)0);
                bw.Write((int)packed.Length);
                bw.Write(packed);
                bw.Write((int)0);
                bw.Flush();
                return ms.ToArray();
            }
            //return Encoding.UTF8.GetBytes("@" + value);
        }
    }

    class GameServerWebRequest
    {
#pragma warning disable 0169// We currently don't use some members of this class. No need for these warnings
#pragma warning disable 0649
        public Player Player;
        public string ClientVersion;
        public string ActName;
        public Dictionary<string, object> ActParams;
        public Dictionary<string, object> Response;
        public HashSet<string> RemoveList;
        public bool Commit;
        public int Keep;
        public bool AddKeep;

        /// <summary>
        /// Used as an alternative response
        /// </summary>
        public string StringResponse;

        // On the main screen these are the mapped error codes:
        // 5 = IDS_SYS.MAINTENANCE
        // 6 = IDS_SYS.FATAL_SERVER_ERROR
        // -1 = IDS_SYS.CLIENT_FATAL_ERROR
        // 20001 = IDS_SYS.DUEL_RESULT_ERROR
        // >=500 && <600 = IDS_SYS.UNEXPECTED_ERROR
        // misc = IDS_SYS.FATAL_SERVER_ERROR (this just displays the error code number)
        public int ErrorCode;// The main error code "code"
        public int ResultCode;// Result / sub error code, 3rd value of "res"

#pragma warning restore 0169
#pragma warning restore 0649

        public void Remove(params string[] strs)
        {
            if (RemoveList == null)
            {
                RemoveList = new HashSet<string>();
            }
            foreach (string str in strs)
            {
                RemoveList.Add(str);
            }
        }

        public Dictionary<string, object> GetOrCreateDictionary(string name)
        {
            object obj;
            if (Response.TryGetValue(name, out obj))
            {
                return obj as Dictionary<string, object>;
            }
            Dictionary<string, object> result = new Dictionary<string, object>();
            Response[name] = result;
            return result;
        }
    }
}
