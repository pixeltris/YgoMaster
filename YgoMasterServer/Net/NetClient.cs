using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Reflection;

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

        public delegate void MessageHandlerEvent(NetClient client, NetMessage message);
        public event MessageHandlerEvent HandleMessage;

        public delegate void ConnectedEvent(NetClient client);
        public event ConnectedEvent Connected;

        public delegate void DisconnectedEvent(NetClient client);
        public event DisconnectedEvent Disconnected;

        public object Data { get; set; }

        static Dictionary<NetMessageType, Type> messageTypes = new Dictionary<NetMessageType, Type>();

        static NetClient()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(NetMessage)) && !type.IsAbstract)
                {
                    NetMessage message = (NetMessage)Activator.CreateInstance(type);
                    messageTypes[message.Type] = type;
                }
            }
        }

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
            //Socket.NoDelay = true;
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
                writer.Write((ushort)message.Type);
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
                                NetMessageType messageType = (NetMessageType)reader.ReadUInt16();
                                Type type;
                                if (messageTypes.TryGetValue(messageType, out type))
                                {
                                    LastMessageTime = DateTime.UtcNow;
                                    if (HandleMessage != null)
                                    {
                                        NetMessage message = (NetMessage)Activator.CreateInstance(type);
                                        message.Read(reader);
                                        HandleMessage(this, message);
                                    }
                                }
                                else
                                {
                                    Utils.LogWarning("Unknown net message type " + messageType);
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
    }
}