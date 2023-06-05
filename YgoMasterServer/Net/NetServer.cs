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

        private Socket socket;
        public string IP { get; private set; }
        public int Port { get; private set; }

        private List<NetClient> connections = new List<NetClient>();
        private Dictionary<string, NetClient> connectionByToken = new Dictionary<string, NetClient>();

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
                NetClient client = new NetClient(socket.EndAccept(ar), this);
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
                //socket.NoDelay = true;
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
                connectionByToken.Clear();
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
                    connectionByToken.Remove(token);
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
            lock (connections)
            {
                connections.Remove(client);
                string token = client.Token;
                if (!string.IsNullOrEmpty(token))
                {
                    connectionByToken.Remove(token);
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
                    OnDuelCom(client, message);
                    break;
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
                    connectionByToken[client.Token] = client;
                }
            }
            client.Send(new ConnectionResponseMessage()
            {
                Success = playerId != 0
            });
        }

        void OnPong(NetClient client, PongMessage message)
        {
        }

        void OnDuelCom(NetClient client, NetMessage message)
        {
            NetClient opponentClient = GetOpponentClient(client);
            if (opponentClient != null)
            {
                opponentClient.Send(message);
            }
        }

        private NetClient GetOpponentClient(NetClient client)
        {
            string opponentToken = GameServer.GetDuelingOpponentToken(client.Token);
            NetClient opponentClient;
            lock (connections)
            {
                connectionByToken.TryGetValue(opponentToken, out opponentClient);
            }
            if (opponentClient == null)
            {
                client.Send(new DuelErrorMessage());
            }
            return opponentClient;
        }
    }
}