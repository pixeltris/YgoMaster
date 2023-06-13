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
            }
        }

        public void CloseConnection(NetClient client)
        {
            lock (connections)
            {
                connections.Remove(client);
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
            Player player = client.Player;
            if (player != null && player.NetClient == client)
            {
                player.NetClient = null;

                lock (tradeLocker)
                {
                    if (player.ActiveTrade != null)
                    {
                        Player otherPlayer = player.ActiveTrade.GetOtherPlayer(player);
                        player.ActiveTrade.Remove(player);
                        if (otherPlayer != null && otherPlayer != player)
                        {
                            NetClient otherClient = otherPlayer.NetClient;
                            if (otherClient != null)
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

                DuelRoom duelRoom = player.DuelRoom;
                if (duelRoom != null)
                {
                    DuelRoomTable table = duelRoom.GetTable(player);
                    if (table != null && table.State == DuelRoomTableState.Dueling)
                    {
                        SendDuelErrorMessageToTable(table, player);
                    }
                }
            }

            lock (connections)
            {
                connections.Remove(client);
            }
        }

        void SendDuelErrorMessageToTable(Player player, Player excludePlayer = null)
        {
            DuelRoom duelRoom = player.DuelRoom;
            DuelRoomTable table = duelRoom == null ? null : duelRoom.GetTable(player);
            if (table != null)
            {
                SendDuelErrorMessageToTable(table, excludePlayer);
            }
            else if (player != excludePlayer)
            {
                NetClient client = player.NetClient;
                if (client != null)
                {
                    client.Send(new DuelErrorMessage());
                }
            }
        }

        void SendDuelErrorMessageToTable(DuelRoomTable table, Player excludePlayer = null)
        {
            Player[] players = { table.Player1, table.Player2 };
            foreach (Player player in players)
            {
                if (player != null && player != excludePlayer)
                {
                    NetClient client = player.NetClient;
                    if (client != null)
                    {
                        client.Send(new DuelErrorMessage());
                    }
                }
            }

            lock (table.Spectators)
            {
                foreach (Player spectator in new HashSet<Player>(table.Spectators))
                {
                    if (spectator.SpectatingPlayerCode > 0)
                    {
                        NetClient spectatorClient = spectator.NetClient;
                        if (spectatorClient != null)
                        {
                            spectatorClient.Send(new DuelErrorMessage());
                        }
                    }
                }
                table.ClearSpectators();
            }
        }

        void OnMessage(NetClient client, NetMessage message)
        {
            switch (message.Type)
            {
                case NetMessageType.ConnectionRequest: OnConnectionRequest(client, (ConnectionRequestMessage)message); break;
                case NetMessageType.Pong: OnPong(client, (PongMessage)message); break;

                case NetMessageType.DuelSpectatorEnter: OnDuelSpectatorEnter(client, (DuelSpectatorEnterMessage)message); break;
                //case NetMessageType.DuelSpectatorLeave: OnDuelSpectatorLeave(client, (DuelSpectatorLeaveMessage)message); break;
                case NetMessageType.DuelSpectatorData: OnDuelSpectatorData(client, (DuelSpectatorDataMessage)message); break;
                case NetMessageType.DuelSpectatorFieldGuide: OnDuelSpectatorFieldGuide(client, (DuelSpectatorFieldGuideMessage)message); break;

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
            if (client.Player != null)
            {
                return;
            }
            Player player = GameServer.GetPlayerFromToken(message.Token);
            try
            {
                GameServer.CheckTokenLimitForIP(null, message.Token, client.IP);
            }
            catch (Exception e)
            {
                Utils.LogWarning(e.ToString());
                player = null;
            }
            if (player != null)
            {
                lock (connections)
                {
                    client.Player = player;
                    player.NetClient = client;
                }
            }
            client.Send(new ConnectionResponseMessage()
            {
                Success = player != null
            });
        }

        void OnPong(NetClient client, PongMessage message)
        {
            // TODO: Use the latency values
            Player player = client.Player;
            if (player == null)
            {
                return;
            }

            // TODO: Check if the client thinks it's ina duel before re-enabling the below code
            /*DuelRoom duelRoom = player.DuelRoom;
            DuelRoomTable table = duelRoom.GetTable(player);
            if (table == null && (player.SpectatingPlayerCode == 0 || duelRoom.GetTableAsSpectator(player) == null))
            {
                client.Send(new DuelErrorMessage());
            }*/
        }

        void OnDuelSpectatorEnter(NetClient client, DuelSpectatorEnterMessage message)
        {
            Player player = client.Player;
            if (player == null)
            {
                return;
            }

            Player duelingPlayer = GameServer.GetPlayerFromId(player.SpectatingPlayerCode);
            if (duelingPlayer == null)
            {
                Utils.LogWarning("TODO: Send error");
                return;
            }

            DuelRoom duelRoom = duelingPlayer.DuelRoom;
            if (duelRoom == null || duelRoom != player.DuelRoom)
            {
                Utils.LogWarning("TODO: Send error");
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(duelingPlayer);
            if (table == null || table.State != DuelRoomTableState.Dueling)
            {
                Utils.LogWarning("TODO: Send error");
                return;
            }

            lock (table.Spectators)
            {
                table.Spectators.Add(player);
                client.Send(new DuelSpectatorDataMessage()
                {
                    Buffer = table.SpectatorData.ToArray()
                });
                client.Send(new DuelSpectatorFieldGuideMessage()
                {
                    Near = table.SpectatorData.Count == 0 ? table.FirstPlayer == 0 : table.SpectatorFieldGuideNear
                });

                DuelSpectatorEnterMessage enterMessage = new DuelSpectatorEnterMessage()
                {
                    PlayerCode = player.Code,
                    SpectatorCount = table.Spectators.Count
                };
                foreach (Player spectator in new HashSet<Player>(table.Spectators))
                {
                    NetClient spectatorClient = spectator.NetClient;
                    if (spectatorClient != null)
                    {
                        spectatorClient.Send(enterMessage);
                    }
                }
            }
        }

        void OnDuelSpectatorData(NetClient client, DuelSpectatorDataMessage message)
        {
            BroadcastDuelSpectatorMessage(client, message, (DuelRoomTable table) =>
            {
                if (message.Buffer != null && message.Buffer.Length > 0)
                {
                    table.SpectatorData.AddRange(message.Buffer);
                }
            });
        }

        void OnDuelSpectatorFieldGuide(NetClient client, DuelSpectatorFieldGuideMessage message)
        {
            BroadcastDuelSpectatorMessage(client, message, (DuelRoomTable table) =>
            {
                table.SpectatorFieldGuideNear = message.Near;
            });
        }

        void BroadcastDuelSpectatorMessage(NetClient client, NetMessage message, Action<DuelRoomTable> actionBeforeBroadcasting)
        {
            Player player = client.Player;
            if (player == null)
            {
                return;
            }

            DuelRoom duelRoom = player.DuelRoom;
            if (duelRoom == null)
            {
                return;
            }

            DuelRoomTable table = duelRoom.GetTable(player);
            if (table == null)
            {
                return;
            }

            lock (table.Spectators)
            {
                if (table.Player1 != player || table.State != DuelRoomTableState.Dueling)
                {
                    return;
                }

                if (actionBeforeBroadcasting != null)
                {
                    actionBeforeBroadcasting(table);
                }

                foreach (Player spectatorPlayer in new HashSet<Player>(table.Spectators))
                {
                    NetClient spectatorClient = spectatorPlayer.NetClient;
                    if (spectatorClient != null && spectatorPlayer.SpectatingPlayerCode == player.Code)
                    {
                        spectatorClient.Send(message);
                    }
                    else
                    {
                        spectatorPlayer.ClearSpectatingDuel();
                    }
                }
            }
        }

        void OnDuelCom(NetClient client, NetMessage message)
        {
            Player opponent = GameServer.GetDuelingOpponent(client.Player);
            NetClient opponentClient = opponent == null ? null : opponent.NetClient;
            if (opponentClient != null)
            {
                opponentClient.Send(message);
            }
            else
            {
                SendDuelErrorMessageToTable(client.Player);
            }
        }

        public void Ping(NetClient client)
        {
            DuelRoomTableState tableState;
            Player opponent = GameServer.GetDuelingOpponent(client.Player, out tableState);
            if (opponent != null && opponent.NetClient == null)
            {
                SendDuelErrorMessageToTable(client.Player);
            }

            client.Send(new PingMessage()
            {
                RequestTime = DateTime.UtcNow,
                DuelingState = tableState
            });
        }

        void OnTradeEnterRoom(NetClient client, TradeEnterRoomMessage message)
        {
            Player player = client.Player;
            if (player == null)
            {
                return;
            }

            lock (tradeLocker)
            {
                if (player.LastEnterTradeRoomRequest > DateTime.UtcNow - TimeSpan.FromSeconds(GameServer.TradeEnterRoomRequestDelayInSeconds))
                {
                    // This is to avoid spamming of the trade button causing a potential state desync between the client/server
                    return;
                }
                player.LastEnterTradeRoomRequest = DateTime.UtcNow;

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
                NetClient otherClient = otherPlayer.NetClient;
                if (otherPlayer.ActiveTrade != null)
                {
                    if (!otherPlayer.ActiveTrade.Add(player) && otherClient != null)
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
            Player player = client.Player;
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
                    NetClient otherClient = otherPlayer.NetClient;
                    if (otherClient != null)
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
            Player player = client.Player;
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
                        NetClient otherClient = otherPlayer.NetClient;
                        if (otherClient != null)
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
            Player player = client.Player;
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

                NetClient otherClient = otherPlayer.NetClient;
                if (otherClient == null)
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