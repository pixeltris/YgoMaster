using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;
using YgoMaster.Net.Messages;

namespace YgoMaster.Net
{
    class NetServer
    {
        public GameServer GameServer;

        private Socket socket;
        public string IP { get; private set; }
        public int Port { get; private set; }

        private List<NetClient> connections;

        public bool Listening { get; private set; }

        public NetServer(int port)
            : this(IPAddress.Any.ToString(), port)
        {
        }

        public NetServer(string ip, int port)
        {
            IP = ip;
            Port = port;
            connections = new List<NetClient>();
        }

        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                NetClient client = new NetClient(socket.EndAccept(ar), this);
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
                writer.Write(NetMessage.GetOpcode(message.GetType()));
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
                connections.Remove(client);
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
                client.Token = message.Token;
            }
            client.Send(new ConnectionResponseMessage()
            {
                Success = playerId != 0
            });
        }
    }
}