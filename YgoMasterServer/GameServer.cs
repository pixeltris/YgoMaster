using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using YgoMaster.Net.Messages;

namespace YgoMaster
{
    partial class GameServer
    {
        Thread thread;
        Thread updateThread;
        HttpListener listener;
        Net.NetServer sessionServer;

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

            thread = new Thread(delegate ()
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

                try
                {
                    sessionServer = new Net.NetServer(sessionServerIP, sessionServerPort);
                    sessionServer.GameServer = this;
                    sessionServer.Listen();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[ERROR] Failed to bind to " + sessionServerIP + ":" + sessionServerPort + " (ensure the program isn't already running, or try running as admin)");
                    Console.WriteLine(e);
                    return;
                }

                HttpListener tempListener = listener;

                updateThread = new Thread(delegate ()
                {
                    DateTime lastReleaseIP = DateTime.UtcNow;
                    TimeSpan releaseIPDelay = TimeSpan.FromMinutes(30);

                    Stopwatch generalUpdateStopwatch = new Stopwatch();
                    generalUpdateStopwatch.Start();

                    Stopwatch sessionServerPingStopwatch = new Stopwatch();
                    sessionServerPingStopwatch.Start();

                    while (listener != null && tempListener == listener)
                    {
                        if (generalUpdateStopwatch.ElapsedMilliseconds >= 10000)
                        {
                            lock (duelRoomsLocker)
                            {
                                foreach (DuelRoom duelRoom in GetDuelRoomsByRoomId().Values)
                                {
                                    if (duelRoom.TimeExpire < DateTime.UtcNow)
                                    {
                                        DisbandRoom(duelRoom);
                                    }
                                }
                            }

                            if (MultiplayerReleaseTokenIPInHours > 0 && lastReleaseIP < DateTime.UtcNow - releaseIPDelay)
                            {
                                lock (playersLock)
                                {
                                    foreach (KeyValuePair<string, HashSet<string>> tokensOnIP in tokensByIP)
                                    {
                                        foreach (string token in new HashSet<string>(tokensOnIP.Value))
                                        {
                                            Player player;
                                            if (!playersByToken.TryGetValue(token, out player) || player.LastRequestTime < DateTime.UtcNow - TimeSpan.FromHours(MultiplayerReleaseTokenIPInHours))
                                            {
                                                tokensOnIP.Value.Remove(token);
                                            }
                                        }
                                    }
                                }
                            }
                            generalUpdateStopwatch.Restart();
                        }

                        if (sessionServerPingStopwatch.Elapsed.TotalSeconds >= SessionServerPingInSeconds)
                        {
                            foreach (Net.NetClient client in sessionServer.GetConnections())
                            {
                                if (client.LastMessageTime < DateTime.UtcNow - TimeSpan.FromSeconds(SessionServerPingTimeoutInSeconds))
                                {
                                    Utils.LogInfo("Ping timeout from " + client.IP);
                                    client.Close();
                                }
                            }
                            sessionServer.Send(new PingMessage()
                            {
                                RequestTime = DateTime.UtcNow
                            });

                            sessionServerPingStopwatch.Restart();
                        }

                        Thread.Sleep(1000);
                    }
                });
                updateThread.CurrentCulture = CultureInfo.InvariantCulture;
                updateThread.SetApartmentState(ApartmentState.STA);
                updateThread.Start();

                Console.WriteLine("Initialized");

                while (listener != null && tempListener == listener)
                {
                    try
                    {
                        HttpListenerContext context = listener.GetContext();
                        if (MultiplayerEnabled)
                        {
                            ThreadPool.QueueUserWorkItem((x) =>
                            {
                                Process(context);
                            });
                        }
                        else
                        {
                            Process(context);
                        }
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
                                if (string.IsNullOrEmpty(sessionToken))
                                {
                                    throw new Exception("Session token required");
                                }

                                sessionToken = Encoding.UTF8.GetString(Convert.FromBase64String(sessionToken));

                                uint playerId = GetPlayerIdFromToken(sessionToken);
                                if (playerId == 0)
                                {
                                    throw new Exception("Invalid player id " + playerId + " from token '" + sessionToken + "'");
                                }

                                lock (playersLock)
                                {
                                    if (!playersByToken.TryGetValue(sessionToken, out gameServerWebRequest.Player))
                                    {
                                        Player possibleExistingPlayer;
                                        if (playersById.TryGetValue(playerId, out possibleExistingPlayer))
                                        {
                                            throw new Exception("Duplicate player code " + playerId + ". Requested token: '" + sessionToken + "'. Existing token: '" + possibleExistingPlayer.Token + "'");
                                        }
                                        playersByToken[sessionToken] = playersById[playerId] = gameServerWebRequest.Player = new Player(playerId);
                                        gameServerWebRequest.Player.LastRequestTime = DateTime.UtcNow;
                                        CheckTokenLimitForIP(gameServerWebRequest.Player, sessionToken, context.Request.RemoteEndPoint.Address.ToString());
                                        LoadPlayer(gameServerWebRequest.Player);
                                        gameServerWebRequest.Player.Token = sessionToken;
                                        SavePlayerNow(gameServerWebRequest.Player);
                                    }
                                    CheckTokenLimitForIP(gameServerWebRequest.Player, sessionToken, context.Request.RemoteEndPoint.Address.ToString());
                                }

                                if (!gameServerWebRequest.Player.HasWrittenToken)
                                {
                                    WriteToken(gameServerWebRequest);
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
                                gameServerWebRequest.Player.LastRequestTime = DateTime.UtcNow;
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
                                    case "User.profile":
                                        Act_UserProfile(gameServerWebRequest);
                                        break;
                                    case "User.record":
                                        Act_UserRecord(gameServerWebRequest);
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
                                    case "Friend.get_list":
                                        Act_FriendGetList(gameServerWebRequest);
                                        break;
                                    case "Friend.refresh_info":
                                        Act_FriendRefreshInfo(gameServerWebRequest);
                                        break;
                                    case "Friend.follow":
                                        Act_FriendFollow(gameServerWebRequest);
                                        break;
                                    case "Friend.id_search":
                                        Act_FriendIdSearch(gameServerWebRequest);
                                        break;
                                    case "Friend.set_pin":
                                        Act_FriendSetPin(gameServerWebRequest);
                                        break;
                                    case "Room.get_room_list":
                                        Act_RoomGetList(gameServerWebRequest);
                                        break;
                                    case "Room.room_entry":
                                        Act_RoomEntry(gameServerWebRequest);
                                        break;
                                    case "Room.room_exit":
                                        Act_RoomExit(gameServerWebRequest);
                                        break;
                                    case "Room.room_create":
                                        Act_RoomCreate(gameServerWebRequest);
                                        break;
                                    case "Room.room_table_polling":
                                        Act_RoomTablePolling(gameServerWebRequest);
                                        break;
                                    case "Room.table_arrive":
                                        Act_RoomTableArrive(gameServerWebRequest);
                                        break;
                                    case "Room.table_leave":
                                        Act_RoomTableLeave(gameServerWebRequest);
                                        break;
                                    case "Room.set_user_comment":
                                        Act_RoomSetUserComment(gameServerWebRequest);
                                        break;
                                    case "Room.room_friend_invite":
                                        Act_RoomFriendInvite(gameServerWebRequest);
                                        break;
                                    case "Room.is_room_battle_ready":
                                        Act_RoomBattleReady(gameServerWebRequest);
                                        break;
                                    case "DuelMenu.deck_check":
                                        Act_DuelMenuDeckCheck(gameServerWebRequest);
                                        break;
                                    case "Duel.matching":
                                        Act_DuelMatching(gameServerWebRequest);
                                        break;
                                    case "Duel.matching_cancel":
                                        Act_DuelMatchingCancel(gameServerWebRequest);
                                        break;
                                    case "Duel.start_waiting":
                                        Act_DuelStartWating(gameServerWebRequest);
                                        break;
                                    case "Duel.start_selecting":
                                        Act_DuelStartSelecting(gameServerWebRequest);
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

        public uint GetPlayerIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return 0;
            }

            if (!string.IsNullOrEmpty(MultiplayerTokenPrefixSecret))
            {
                if (!token.StartsWith(MultiplayerTokenPrefixSecret))
                {
                    return 0;
                }
                token = token.Substring(0, MultiplayerTokenPrefixSecret.Length);
                if (string.IsNullOrEmpty(token))
                {
                    return 0;
                }
            }

            if (MultiplayerAllowUserSpecifiedPlayerCode)
            {
                // First check for manually entered player code e.g. "000-000-001"
                string[] splitted = token.Trim().Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length == 3)
                {
                    uint[] numbers = new uint[3];
                    bool allNumbers = false;
                    for (int i = 0; i < numbers.Length; i++)
                    {
                        uint number;
                        if (!uint.TryParse(splitted[i], out number) || splitted[i].Length != 3)
                        {
                            allNumbers = false;
                        }
                        numbers[i] = number;
                    }
                    if (allNumbers)
                    {
                        return (numbers[0] * 1000000) + (numbers[1] * 1000) + numbers[2];
                    }
                }
            }

            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToUInt32(md5.ComputeHash(Encoding.UTF8.GetBytes(token)), 0) % 999999999;
            }
        }

        public void CheckTokenLimitForIP(Player player, string token, string remoteIP)
        {
            if (player == null || string.IsNullOrEmpty(player.IP))
            {
                if (player != null)
                {
                    player.IP = remoteIP;
                }

                int tokenLimit = GetMaxTokensPerIP(remoteIP); ;

                if (MultiplayerReleaseTokenIPInHours > 0 && tokenLimit > 0)
                {
                    HashSet<string> tokensOnThisIP;
                    if (!tokensByIP.TryGetValue(remoteIP, out tokensOnThisIP))
                    {
                        tokensOnThisIP = new HashSet<string>();
                    }
                    if (!tokensOnThisIP.Contains(token))
                    {
                        if (tokensOnThisIP.Count >= MultiplayerMaxTokensPerIP)
                        {
                            throw new Exception("Ignore request from " + remoteIP + " due to reaching token IP limit of " + tokenLimit);
                        }
                        tokensOnThisIP.Add(token);
                    }
                }
            }
        }

        private int GetMaxTokensPerIP(string remoteIP)
        {
            int tokenLimit;
            if (MultiplayerMaxTokensPerIPEx.TryGetValue(remoteIP, out tokenLimit))
            {
                if (tokenLimit == 0)
                {
                    throw new Exception("Request from banned IP " + remoteIP);
                }
            }
            else
            {
                tokenLimit = MultiplayerMaxTokensPerIP;
            }
            return tokenLimit;
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
