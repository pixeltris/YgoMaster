using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;
#if YGO_MASTER_CLIENT
using YgoMasterClient;
using YgoMaster.Net.Messages;
#endif

namespace YgoMaster.Net
{
    class NetClient
    {
        public DateTime LastMessageTime;
#if !YGO_MASTER_CLIENT
        public string Token;
#endif

        private object sendLocker = new object();
        public object Owner { get; private set; }
        public Socket Socket { get; private set; }
        public string IP { get; private set; }
        public int Port { get; private set; }
        public bool IsConnected
        {
            get
            {
                try
                {
                    Socket socket = Socket;
                    return socket != null && socket.Connected;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool IsServer
        {
            get { return Owner != this; }
        }

        public EndPoint LocalEndPoint
        {
            get
            {
                try
                {
                    Socket socket = Socket;
                    return socket != null ? socket.LocalEndPoint : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public EndPoint RemoteEndPoint
        {
            get
            {
                try
                {
                    Socket socket = Socket;
                    return socket != null ? socket.RemoteEndPoint : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        private const int defaultBufferSize = 1024;
        private byte[] tempBuffer;
        private NetPacketBuffer buffer;

        public delegate void ConnectedEvent(NetClient client);
        public event ConnectedEvent Connected;

        public delegate void DisconnectedEvent(NetClient client);
        public event DisconnectedEvent Disconnected;

        public object Data { get; set; }

        public NetClient()
        {
            Owner = this;
            tempBuffer = new byte[defaultBufferSize];
            buffer = new NetPacketBuffer();
        }

        public NetClient(Socket socket, object owner)
            : this()
        {
            Owner = owner;
            IP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            Port = ((IPEndPoint)socket.RemoteEndPoint).Port;
            Socket = socket;
            LastMessageTime = DateTime.UtcNow;
            BeginReceive();
        }

        public void Connect(string ip, int port)
        {
            Connect(IPAddress.Parse(ip), port);
        }

        public void Connect(IPAddress ip, int port)
        {
            IP = ip.ToString();
            Port = port;

            Socket socket = Socket;
            if (socket != null)
                socket.Close();

            Socket = socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);
            LastMessageTime = DateTime.UtcNow;
            if (Connected != null)
            {
                Connected(this);
            }
            BeginReceive();
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
            try
            {
                byte[] output = new byte[data.Length + 4];
                Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, output, 0, 4);
                Buffer.BlockCopy(data, 0, output, 4, data.Length);
                lock (sendLocker)
                {
                    Socket.Send(output);
                }
            }
            catch
            {
                OnDisconnected();
            }
        }

        private void OnDisconnected()
        {
            try
            {
                try
                {
                    Close();
                }
                catch
                {
                }

                if (Disconnected != null)
                    Disconnected(this);
            }
            catch
            {
            }
        }

        public void Close()
        {
            Socket socket = Socket;
            if (socket.Connected)
                socket.Close();
        }

        void OnRecvData(IAsyncResult ar)
        {
            try
            {
                byte[] data = GetRecievedData(ar);

                if (data.Length > 0)
                {
                    buffer.Append(data);
                    while (buffer.IsComplete())
                    {
                        byte[] packet = buffer.GetPacket();
                        try
                        {
                            using (MemoryStream stream = new MemoryStream(packet))
                            using (BinaryReader reader = new BinaryReader(stream))
                            {
                                uint opcode = reader.ReadUInt32();
                                Type type = NetMessage.GetType(opcode);
                                if (type == null)
                                {
                                    Utils.LogWarning("Unknown opcode " + opcode);
                                    return;
                                }

                                LastMessageTime = DateTime.UtcNow;

#if YGO_MASTER_CLIENT
                                Dictionary<Type, MethodInfo> handlers = NetMessageHandlers<NetClient>.Handlers;
#else
                                Dictionary<Type, MethodInfo> handlers = IsServer ?
                                    NetMessageHandlers<NetServer>.Handlers : NetMessageHandlers<NetClient>.Handlers;
#endif

                                MethodInfo handler;
                                if (handlers.TryGetValue(type, out handler))
                                {
                                    NetMessage message = (NetMessage)Activator.CreateInstance(type);
                                    message.Read(reader);
                                    if (IsServer)
                                    {
                                        handler.Invoke(Owner, new object[] { this, message });
                                    }
                                    else
                                    {
                                        handler.Invoke(Owner, new object[] { message });
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utils.LogWarning(e.ToString());
                        }
                    }
                    BeginReceive();
                }
                else
                    OnDisconnected();
            }
            catch
            {
                OnDisconnected();
            }
        }

        byte[] GetRecievedData(IAsyncResult ar)
        {
            int nBytesRec = 0;
            try { nBytesRec = Socket.EndReceive(ar); }
            catch { }
            byte[] byReturn = new byte[nBytesRec];
            Array.Copy(tempBuffer, byReturn, nBytesRec);

            return byReturn;
        }

        void BeginReceive()
        {
            try
            {
                Socket.BeginReceive(tempBuffer, 0, defaultBufferSize,
                        SocketFlags.None, new AsyncCallback(OnRecvData), null);
            }
            catch
            {
                OnDisconnected();
            }
        }

#if YGO_MASTER_CLIENT
        void OnConnectionResponse(ConnectionResponseMessage message)
        {
            if (!message.Success)
            {
                Console.WriteLine("Session server failed to validate token '" + ClientSettings.MultiplayerToken + "'");
            }
        }

        void OnPing(PingMessage message)
        {
            if (message.DuelingState != DuelRoomTableState.Dueling)
            {
                // TODO: Check if actively dueling
            }

            Send(new PongMessage()
            {
                ServerToClientLatency = Utils.GetEpochTime() - Utils.GetEpochTime(message.RequestTime),
                ResponseTime = DateTime.UtcNow
            });
        }
#endif
    }
}