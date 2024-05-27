using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace YgoMasterClient
{
    static class SimpleProxy
    {
        class Connection
        {
            public Socket ClientSocket;
            public Socket ServerSocket;
            public byte[] ClientSocketRecvBuffer = new byte[1024];
            public byte[] ServerSocketRecvBuffer = new byte[1024];

            public Connection(Socket serverSocket)
            {
                ServerSocket = serverSocket;
            }

            public void Start()
            {
                try
                {
                    ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ClientSocket.BeginConnect(targetIP, ClientSettings.BasePort, new AsyncCallback(OnConnect), ClientSocket);
                }
                catch
                {
                    Close();
                }
            }

            void OnConnect(IAsyncResult ar)
            {
                try
                {
                    ClientSocket.EndConnect(ar);
                }
                catch
                {
                    Close();
                    return;
                }
                BeginRecv(ClientSocket, ClientSocketRecvBuffer);
                BeginRecv(ServerSocket, ServerSocketRecvBuffer);
            }

            void BeginRecv(Socket socket, byte[] buffer)
            {
                try
                {
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnRecvData), socket);
                }
                catch
                {
                    Close();
                }
            }

            void OnRecvData(IAsyncResult ar)
            {
                try
                {
                    Socket socket = (Socket)ar.AsyncState;
                    byte[] buffer = socket == ServerSocket ? ServerSocketRecvBuffer : ClientSocketRecvBuffer;

                    byte[] data = GetRecievedData(ar);
                    if (data != null)
                    {
                        Socket otherSocket = socket == ServerSocket ? ClientSocket : ServerSocket;
                        int offset = 0;
                        while (offset < data.Length)
                        {
                            try
                            {
                                int sent = otherSocket.Send(data, offset, data.Length - offset, SocketFlags.None);
                                if (sent == 0)
                                {
                                    Close();
                                    return;
                                }
                                offset += sent;
                                VerboseLog("Sent " + sent + " bytes to " + (socket == ServerSocket ? "YgoMaster" : "client"));
                            }
                            catch
                            {
                                Close();
                                return;
                            }
                        }
                    }
                    BeginRecv(socket, buffer);
                }
                catch
                {
                    Close();
                }
            }

            byte[] GetRecievedData(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                byte[] buffer = socket == ServerSocket ? ServerSocketRecvBuffer : ClientSocketRecvBuffer;

                int nBytesRec = 0;
                try { nBytesRec = socket.EndReceive(ar); }
                catch { }
                byte[] byReturn = new byte[nBytesRec];
                Array.Copy(buffer, byReturn, nBytesRec);

                return byReturn;
            }

            void Close()
            {
                if (ServerSocket != null)
                {
                    RemoveConnection(this);
                }
                CloseSocket(ClientSocket);
                CloseSocket(ServerSocket);
                ClientSocket = null;
                ServerSocket = null;
            }

            void CloseSocket(Socket socket)
            {
                try
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                }
                catch
                {
                }
            }
        }

        static List<Connection> connections = new List<Connection>();
        static string targetIP;

        public static void LogConnections()
        {
            Console.WriteLine("Proxy connections: " + connections.Count);
        }

        static void RemoveConnection(Connection connection)
        {
            lock (connections)
            {
                connections.Remove(connection);
                VerboseLog("Remove proxy connection. Connections: " + connections.Count);
            }
        }

        public static void Run()
        {
            if (ClientSettings.ProxyPort == 0)
            {
                Console.WriteLine("Can't create proxy server on port " + ClientSettings.ProxyPort);
                return;
            }

            new Thread(delegate ()
            {
                targetIP = ClientSettings.ResolveIP(ClientSettings.BaseIP);

                try
                {
                    TcpListener listener = new TcpListener(IPAddress.Loopback, ClientSettings.ProxyPort);
                    listener.Start();
                    Console.WriteLine("Proxy listening on port " + ClientSettings.ProxyPort);
                    while (true)
                    {
                        try
                        {
                            Socket socket = listener.AcceptSocket();
                            Connection connection = new Connection(socket);
                            lock (connections)
                            {
                                connections.Add(connection);
                                VerboseLog("Create proxy connection. Connections: " + connections.Count);
                            }
                            connection.Start();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR when creating proxy connection " + e);
                        }
                        Thread.Sleep(100);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to create proxy server");
                }
            }).Start();
        }

        static void VerboseLog(string str)
        {
            //Console.WriteLine(str);
        }
    }
}
