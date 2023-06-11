using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using YgoMaster.Net.Message;

namespace YgoMaster.Net
{
    class NetServer
    {
        public GameServer GameServer;
        object tradeLocker = new object();

        private Socket socket;
        public string IP { get; private set; }
        public int Port { get; private set; }

        private List<NetClient> connections = new List<NetClient>();
        private Dictionary<string, NetClient> connectionsByToken = new Dictionary<string, NetClient>();

        public bool Listening { get; private set; }

        public NetServer(int port)
            : this(IPAddress.Any.ToString(), port)
        {
        }

        public NetServer(string ip, int port)
        {
            IP = ip;
            Port = port;
        }

        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = socket.EndAccept(ar);
                clientSocket.NoDelay = GameServer.MultiplayerNoDelay;
                NetClient client = new NetClient(clientSocket, this);
                client.HandleMessage += OnMessage;
                client.Disconnected += new NetClient.DisconnectedEvent(client_Disconnected);
                lock (connections)
                {
                    connections.Add(client);
                }

                socket.BeginAccept(new AsyncCallback(ConnectCallback), null);
            }
            catch
            {
            }
        }

        public virtual void Listen()
        {
            lock (connections)
            {
                Close();
                Listening = true;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.NoDelay = GameServer.MultiplayerNoDelay;
                socket.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
                socket.Listen(100);
                socket.BeginAccept(new AsyncCallback(ConnectCallback), null);
            }
        }

        public virtual void Close()
        {
            lock (connections)
            {
                Listening = false;
                try
                {
                    if (socket != null)
                        socket.Close();
                }
                catch
                {
                }

                foreach (NetClient client in new List<NetClient>(connections))
                    client.Close();
                connections.Clear();
                connectionsByToken.Clear();
            }
        }

        public void CloseConnection(NetClient client)
        {
            lock (connections)
            {
                connections.Remove(client);
                string token = client.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    connectionsByToken.Remove(token);
                }
                client.Close();
            }
        }

        public List<NetClient> GetConnections()
        {
            lock (connections)
            {
                return new List<NetClient>(connections);
            }
        }

        public NetClient GetConnectionByToken(string token)
        {
            lock (connections)
            {
                NetClient client;
                connectionsByToken.TryGetValue(token, out client);
                return client;
            }
        }

        public void Send(NetMessage message)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)message.Type);
                message.Write(writer);
                writer.Flush();
                Send(stream.ToArray());
            }
        }

        public void Send(byte[] data)
        {
            lock (connections)
            {
                foreach (NetClient client in new List<NetClient>(connections))
                {
                    try
                    {
                        if (client.IsConnected)
                            client.Send(data);
                        else
                            CloseConnection(client);
                    }
                    catch
                    {
                    }
                }
            }
        }

        void client_Disconnected(NetClient client)
        {
            Player player = GameServer.GetPlayerFromToken(client.Token);
            if (player != null)
            {
                lock (tradeLocker)
                {
                    if (player.ActiveTrade != null)
                    {
                        Player otherPlayer = player.ActiveTrade.GetOtherPlayer(player);
                        player.ActiveTrade.Remove(player);
                        if (otherPlayer != null && otherPlayer != player)
                        {
                            NetClient otherClient = GetConnectionByToken(otherPlayer.Token);
                            if (otherClient != null && otherClient != client)
                            {
                                otherClient.Send(new TradeLeaveRoomMessage()
                                {
                                    PlayerCode = player.Code,
                                    Name = player.Name
                                });
                            }
                        }
                    }
                }
            }

            lock (connections)
            {
                connections.Remove(client);
                string token = client.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    connectionsByToken.Remove(token);
                }
            }
        }

        public bool HasClientWithToken(string token)
        {
            return FindClient(token) != null;
        }

        public NetClient FindClient(string token)
        {
            lock (connections)
            {
                return connections.OrderByDescending(x => x.LastMessageTime).FirstOrDefault(x => x.Token == token);
            }
        }

        void OnMessage(NetClient client, NetMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.ConnectionRequest: OnConnectionRequest(client, (ConnectionRequestMessage)message); break;
                case NetMessageType.Pong: OnPong(client, (PongMessage)message); break;

                case NetMessageType.DuelComMovePhase:
                case NetMessageType.DuelComDoCommand:
                case NetMessageType.DuelComCancelCommand:
                case NetMessageType.DuelComCancelCommand2:
                case NetMessageType.DuelDlgSetResult:
                case NetMessageType.DuelListSetCardExData:
                case NetMessageType.DuelListSetIndex:
                case NetMessageType.DuelListInitString:
                case NetMessageType.UpdateIsBusyEffect:// Special case
                case NetMessageType.DuelError:// Special case
                    OnDuelCom(client, message);
                    break;

                case NetMessageType.TradeEnterRoom: OnTradeEnterRoom(client, (TradeEnterRoomMessage)message);  break;
                case NetMessageType.TradeLeaveRoom: OnTradeLeaveRoom(client, (TradeLeaveRoomMessage)message); break;
                case NetMessageType.TradeMoveCard: OnTradeMoveCard(client, (TradeMoveCardMessage)message); break;
                case NetMessageType.TradeStateChange: OnTradeStateChange(client, (TradeStateChangeMessage)message); break;
            }
        }

        void OnConnectionRequest(NetClient client, ConnectionRequestMessage message)
        {
            uint playerId = GameServer.GetPlayerIdFromToken(message.Token);
            try
            {
                GameServer.CheckTokenLimitForIP(null, message.Token, client.IP);
            }
            catch (Exception e)
            {
                Utils.LogWarning(e.ToString());
                playerId = 0;
            }
            if (playerId != 0)
            {
                lock (connections)
                {
                    client.Token = message.Token;
                    connectionsByToken[client.Token] = client;
                }
            }
            client.Send(new ConnectionResponseMessage()
            {
                Success = playerId != 0
            });
        }

        void OnPong(NetClient client, PongMessage message)
        {
            // TODO: Use the latency values
        }

        void OnDuelCom(NetClient client, NetMessage message)
        {
            NetClient opponentClient = GetDuelingOpponentClient(client);
            if (opponentClient != null)
            {
                opponentClient.Send(message);
            }
        }

        NetClient GetDuelingOpponentClient(NetClient client)
        {
            Player opponent = GameServer.GetDuelingOpponent(client.Token);
            NetClient opponentClient = null;
            if (opponent != null)
            {
                lock (connections)
                {
                    connectionsByToken.TryGetValue(opponent.Token, out opponentClient);
                }
            }
            if (opponentClient == null)
            {
                client.Send(new DuelErrorMessage());
            }
            return opponentClient;
        }

        public void Ping(NetClient client)
        {
            DuelRoomTableState tableState;
            Player opponent = GameServer.GetDuelingOpponent(client.Token, out tableState);
            if (opponent != null)
            {
                NetClient opponentClient;
                lock (connections)
                {
                    connectionsByToken.TryGetValue(opponent.Token, out opponentClient);
                }
                if (opponentClient == null)
                {
                    client.Send(new DuelErrorMessage());
                }
            }

            client.Send(new PingMessage()
            {
                RequestTime = DateTime.UtcNow,
                DuelingState = tableState
            });
        }

        void OnTradeEnterRoom(NetClient client, TradeEnterRoomMessage message)
        {
            Player player = GameServer.GetPlayerFromToken(client.Token);
            if (player == null)
            {
                return;
            }

            lock (tradeLocker)
            {
                if (player.ActiveTrade != null)
                {
                    player.ActiveTrade.Remove(player);
                    player.ActiveTrade = null;
                }
                Player otherPlayer = GameServer.GetPlayerFromId(message.PlayerCode);
                if (otherPlayer == null || otherPlayer == player)
                {
                    return;
                }
                NetClient otherClient = GetConnectionByToken(otherPlayer.Token);
                if (otherClient == client)
                {
                    return;
                }

                if (otherPlayer.ActiveTrade != null)
                {
                    if (!otherPlayer.ActiveTrade.Add(player))
                    {
                        otherClient.Send(new TradeEnterRoomMessage()
                        {
                            IsRemotePlayerAlreadyTrading = true,
                            PlayerCode = otherPlayer.Code,
                            Name = otherPlayer.Name
                        });
                        return;
                    }
                }
                else
                {
                    player.ActiveTrade = new TradeInfo();
                    if (!player.ActiveTrade.Add(player))
                    {
                        player.ActiveTrade = null;
                        return;
                    }
                }

                string myEntireCollectionJson = player.Cards.CreateCardHaveJson(false);
                string myTradableCollectionJson = player.Cards.CreateCardHaveJson(true);
                string theirEntireCollectionJson = otherPlayer.Cards.CreateCardHaveJson(false);
                string theirTradableCollectionJson = otherPlayer.Cards.CreateCardHaveJson(true);

                client.Send(new TradeEnterRoomMessage()
                {
                    PlayerCode = message.PlayerCode,
                    OwnsMainDeck = player == player.ActiveTrade.Player1,
                    IsRemotePlayerAlreadyHere = player.ActiveTrade.GetOtherPlayer(player) != null,
                    DeckJson = MiniJSON.Json.Serialize(player.ActiveTrade.State.ToDictionaryEx()),
                    MyEntireCollectionJson = myEntireCollectionJson,
                    MyTradableCollectionJson = myTradableCollectionJson,
                    TheirEntireCollectionJson = theirEntireCollectionJson,
                    TheirTradableCollectionJson = theirTradableCollectionJson,
                });
                if (otherClient != null)
                {
                    otherClient.Send(new TradeEnterRoomMessage()
                    {
                        PlayerCode = player.Code,
                        Name = player.Name,
                        MyEntireCollectionJson = theirEntireCollectionJson,
                        MyTradableCollectionJson = theirTradableCollectionJson,
                        TheirEntireCollectionJson = myEntireCollectionJson,
                        TheirTradableCollectionJson = myTradableCollectionJson,
                    });
                }
            }
        }

        void OnTradeLeaveRoom(NetClient client, TradeLeaveRoomMessage message)
        {
            Player player = GameServer.GetPlayerFromToken(client.Token);
            if (player == null)
            {
                return;
            }

            lock (tradeLocker)
            {
                if (player.ActiveTrade == null)
                {
                    return;
                }

                if (!player.ActiveTrade.Remove(player))
                {
                    player.ActiveTrade = null;
                    return;
                }

                Player otherPlayer = GameServer.GetPlayerFromId(message.PlayerCode);
                if (otherPlayer != null && otherPlayer != player)
                {
                    NetClient otherClient = GetConnectionByToken(otherPlayer.Token);
                    if (otherClient != null && otherClient != client)
                    {
                        otherClient.Send(new TradeLeaveRoomMessage()
                        {
                            PlayerCode = player.Code,
                            Name = player.Name
                        });
                    }
                }
            }
        }

        void OnTradeMoveCard(NetClient client, TradeMoveCardMessage message)
        {
            Player player = GameServer.GetPlayerFromToken(client.Token);
            if (player == null)
            {
                return;
            }

            lock (tradeLocker)
            {
                TradeInfo trade = player.ActiveTrade;

                if (trade == null)
                {
                    return;
                }

                Player targetPlayer = player;
                bool targetIsOtherPlayer = false;

                if (message.OtherPlayerCode != 0)
                {
                    targetIsOtherPlayer = true;

                    targetPlayer = GameServer.GetPlayerFromId(message.OtherPlayerCode);
                    if (targetPlayer == null || targetPlayer == player)
                    {
                        return;
                    }

                    if (trade.Player1 != null && trade.Player2 != null &&
                        trade.Player1 != targetPlayer && trade.Player2 != targetPlayer)
                    {
                        return;
                    }

                    if (message.RemoveCard && !GameServer.TradeAllowOtherPlayerToRemoveYourCards)
                    {
                        return;
                    }

                    if (!message.RemoveCard && !GameServer.TradeAllowOtherPlayerToAddYourCards)
                    {
                        return;
                    }
                }

                CardCollection targetCollection;
                if ((player == trade.Player1 && !targetIsOtherPlayer) ||
                    (player == trade.Player2 && targetIsOtherPlayer))
                {
                    targetCollection = trade.State.MainDeckCards;
                }
                else if ((player == trade.Player2 && !targetIsOtherPlayer) ||
                         (player == trade.Player1 && targetIsOtherPlayer))
                {
                    targetCollection = trade.State.ExtraDeckCards;
                }
                else
                {
                    return;
                }

                bool success = false;

                if (message.RemoveCard)
                {
                    success = targetCollection.Remove(message.CardId, message.StyleRarity);
                }
                else
                {
                    int count = targetPlayer.Cards.GetCount(message.CardId, PlayerCardKind.Dismantle, message.StyleRarity);
                    count -= targetCollection.GetCount(message.CardId, message.StyleRarity);
                    if (count > 0)
                    {
                        targetCollection.Add(message.CardId, message.StyleRarity);
                        success = true;
                    }
                }

                if (success)
                {
                    client.Send(new TradeMoveCardMessage()
                    {
                        RemoveCard = message.RemoveCard,
                        CardId = message.CardId,
                        StyleRarity = message.StyleRarity,
                        OtherPlayer = targetIsOtherPlayer
                    });

                    Player otherPlayer = trade.GetOtherPlayer(player);
                    if (otherPlayer != null && otherPlayer != player)
                    {
                        NetClient otherClient = GetConnectionByToken(otherPlayer.Token);
                        if (otherClient != null && otherClient != client)
                        {
                            otherClient.Send(new TradeMoveCardMessage()
                            {
                                RemoveCard = message.RemoveCard,
                                CardId = message.CardId,
                                StyleRarity = message.StyleRarity,
                                OtherPlayer = !targetIsOtherPlayer
                            });
                        }
                    }
                }
            }
        }

        void OnTradeStateChange(NetClient client, TradeStateChangeMessage message)
        {
            Player player = GameServer.GetPlayerFromToken(client.Token);
            if (player == null)
            {
                client.Send(new TradeStateChangeMessage(TradeStateChange.Error));
                return;
            }

            lock (tradeLocker)
            {
                TradeInfo trade = player.ActiveTrade;

                if (trade == null)
                {
                    client.Send(new TradeStateChangeMessage(TradeStateChange.Error));
                    return;
                }

                if (message.State == TradeStateChange.PressedCancel)
                {
                    trade.SetHasPressedTrade(player, false);
                }

                Player otherPlayer = trade.GetOtherPlayer(player);
                if (otherPlayer == null || otherPlayer == player)
                {
                    client.Send(new TradeStateChangeMessage(TradeStateChange.Wait));
                    return;
                }

                NetClient otherClient = GetConnectionByToken(otherPlayer.Token);
                if (otherClient == null || otherClient == client)
                {
                    client.Send(new TradeStateChangeMessage(TradeStateChange.Wait));
                    return;
                }

                if (message.State != TradeStateChange.PressedTrade && message.State != TradeStateChange.PressedCancel)
                {
                    client.Send(new TradeStateChangeMessage(TradeStateChange.Error));
                    return;
                }

                if (message.State == TradeStateChange.PressedCancel)
                {
                    otherClient.Send(new TradeStateChangeMessage(TradeStateChange.PressedCancel));
                }
                else if (message.State == TradeStateChange.PressedTrade)
                {
                    trade.SetHasPressedTrade(player, true);
                    if (!trade.HaveBothPressedTrade)
                    {
                        otherClient.Send(new TradeStateChangeMessage(TradeStateChange.PressedTrade));
                    }
                    else
                    {
                        // NOTE: Player card collections could potentially be modified by the game server handler mid-loop

                        bool valid = true;

                        for (int i = 0; i < 2; i++)
                        {
                            Player targetPlayer = i == 0 ? trade.Player1 : trade.Player2;
                            CardCollection targetCollection = i == 0 ? trade.State.MainDeckCards : trade.State.ExtraDeckCards;
                            foreach (KeyValuePair<int, Dictionary<CardStyleRarity, int>> card in targetCollection.ToDictionaryCount())
                            {
                                foreach (KeyValuePair<CardStyleRarity, int> rarityCount in card.Value)
                                {
                                    if (targetPlayer.Cards.GetCount(card.Key, PlayerCardKind.Dismantle, rarityCount.Key) < rarityCount.Value)
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                                if (!valid)
                                {
                                    break;
                                }
                            }
                            if (!valid)
                            {
                                break;
                            }
                        }

                        if (valid)
                        {
                            foreach (KeyValuePair<int, CardStyleRarity> card in trade.State.MainDeckCards.GetCollection())
                            {
                                trade.Player1.Cards.Subtract(card.Key, 1, PlayerCardKind.Dismantle, card.Value);
                                trade.Player2.Cards.Add(card.Key, 1, PlayerCardKind.Dismantle, card.Value);
                            }
                            foreach (KeyValuePair<int, CardStyleRarity> card in trade.State.ExtraDeckCards.GetCollection())
                            {
                                trade.Player1.Cards.Add(card.Key, 1, PlayerCardKind.Dismantle, card.Value);
                                trade.Player2.Cards.Subtract(card.Key, 1, PlayerCardKind.Dismantle, card.Value);
                            }

                            GameServer.SavePlayerNow(trade.Player1);
                            GameServer.SavePlayerNow(trade.Player2);

                            client.Send(new TradeStateChangeMessage(TradeStateChange.Complete, player.Cards.CreateCardHaveJson(false)));
                            otherClient.Send(new TradeStateChangeMessage(TradeStateChange.Complete, otherPlayer.Cards.CreateCardHaveJson(false)));
                        }
                        else
                        {
                            client.Send(new TradeStateChangeMessage(TradeStateChange.Error));
                            otherClient.Send(new TradeStateChangeMessage(TradeStateChange.Error));
                        }

                        trade.Remove(player);
                        trade.Remove(otherPlayer);
                    }
                }
            }
        }
    }
}