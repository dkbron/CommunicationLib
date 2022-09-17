using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLib.Core.Network
{ 
    public class DdkTcpServer : IDisposable
    {
        public TcpListener server = null;

        public bool IsOpen = false;
         
        private IPAddress localIPAddress = IPAddress.Any;

        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
            set { localIPAddress = value; }
        }

        /// <summary>
        /// 客户端列表
        /// </summary>
        public List<Client> tcpClientList = new List<Client>();

        public delegate void ClientConnectedDelegate(Client client);
        public event ClientConnectedDelegate ClientConnectedEvent; 

        public delegate void ClientDisConnectedDelegate(Client client);
        public event ClientDisConnectedDelegate ClientDisconnectedEvent;

        /// <summary>
        /// 处理客户端信息事件
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="data">收到的信息</param>
        public delegate void RecieveClientMessageDelegate(Client client, byte[] data);
        public event RecieveClientMessageDelegate RecieveClientMessageEvent;

        /// <summary>
        /// 推送错误信息事件
        /// </summary>
        public event Action<string> PushExceptionMessageEvent;

        #region 构造
        public DdkTcpServer()
        {

        }

        public DdkTcpServer(IPEndPoint ipEndPoint)
        {
            CreateListener(ipEndPoint);
        }
        public DdkTcpServer(IPAddress ip, int port)
        {
            CreateListener(new IPEndPoint(ip, port));
        }

        public DdkTcpServer(string ip, int port)
        {
            CreateListener(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public DdkTcpServer(int port)
        {
            CreateListener(new IPEndPoint(LocalIPAddress, port));
        }
        #endregion

        #region 创建与连接
        public void CreateListenerAndConnect(int port)
        {
            CreateListenerAndConnect(LocalIPAddress, port);
        }

        public void CreateListenerAndConnect(string ip, int port)
        {
            CreateListenerAndConnect(IPAddress.Parse(ip), port);
        }

        public void CreateListenerAndConnect(IPAddress ip, int port)
        {
            CreateListenerAndConnect(new IPEndPoint(ip, port));
        }

        public void CreateListenerAndConnect(IPEndPoint ipEndPoint)
        {
            server = new TcpListener(ipEndPoint);
            server.Start();
            IsOpen = true;
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, server);
        }

        public void CreateListener(IPEndPoint ipEndPoint)
        {
            server = new TcpListener(ipEndPoint);
        }

        public void Connect()
        {
            if (IsOpen)
                return;
            if (server.LocalEndpoint == null)
                return;
            server.Start();
            IsOpen = true;
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, server);
        }

        #endregion


        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            if (!IsOpen)
                return;
            try
            {  
                TcpClient tcpClient = server.EndAcceptTcpClient(ar);
                Client client = new Client(tcpClient);

                var item = tcpClientList.FirstOrDefault(x => x.TcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
                if(item != null)
                    tcpClientList.RemoveAll(x => x.TcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);

                tcpClientList.Add(client); 
                ClientConnectedEvent?.Invoke(client);

                server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);

                NetworkStream networkStream = client.NetworkStream;
                networkStream.ReadTimeout = 4000;
                networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
            }
            catch(Exception ex)
            {
                PushExceptionMessageEvent?.Invoke(ex.Message);
            } 
        }

        private void ReadCallback(IAsyncResult ar)
        { 
            Client client = ar.AsyncState as Client;
            client.Ticks = DateTime.Now.Ticks; 
            if (client != null)
            {
                int read;
                NetworkStream networkStream = null;
                try
                {
                    networkStream = client.NetworkStream; 
                    read = networkStream.EndRead(ar); 

                    if (read == 0)
                    { 
                        OnClientDisconnected(client);
                        return;
                    }
                    byte[] data = new byte[read];
                    Buffer.BlockCopy(client.Buffer, 0, data, 0, read); 
                    RecieveClientMessageEvent?.Invoke(client,data);
                    networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);

                }
                catch (Exception ex)
                {
                    PushExceptionMessageEvent?.Invoke(ex.Message);
                }

            }
        }

        public void Send(Client client, string msg)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            if (client.TcpClient.Client.Connected)
                client.NetworkStream.Write(sendMsg, 0, sendMsg.Length);
        }

        public async Task SendAsync(Client client, string msg)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            if (client.TcpClient.Client.Connected)
                await client.NetworkStream.WriteAsync(sendMsg, 0, sendMsg.Length);
        }


        public void SendToAllClient(string msg)
        {
            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);

            foreach(var client in tcpClientList)
            { 
                if(client.TcpClient.Client.Connected)
                    client.NetworkStream.Write(sendMsg, 0, sendMsg.Length);
            }
        }

        public async Task SendToAllClientAsync(string msg)
        {
            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            foreach (var client in tcpClientList)
            {
                if (client.TcpClient.Client.Connected)
                    await client.NetworkStream.WriteAsync(sendMsg, 0, sendMsg.Length); 
            }
        }


        public void Disconnect()
        {  
            try
            {
                foreach (Client clientLoop in tcpClientList)
                {
                    if (clientLoop.TcpClient.Client.Connected)
                    {
                        ClientDisconnectedEvent?.Invoke(clientLoop);
                        clientLoop.TcpClient.Client.Disconnect(false);
                    }
                }
                tcpClientList.Clear();
            }
            catch (Exception ex)
            {
                PushExceptionMessageEvent?.Invoke(ex.Message);
            }
            finally
            { 
                server.Stop();
                IsOpen = false;
            }

        }

        public void OnClientDisconnected(Client client)
        { 
            if (client == null)
                return;
            try
            {
                tcpClientList.RemoveAll(c=>c==client);
                if (client.TcpClient.Client.Connected)
                { 
                    ClientDisconnectedEvent?.Invoke(client);
                    client.TcpClient.Client.Disconnect(false);
                }
            }
            catch(Exception ex)
            {
                PushExceptionMessageEvent?.Invoke(ex.Message);
            }
        }

        public void Dispose()
        {
            server = null;
        }

        public class Client
        {
            private readonly TcpClient tcpClient;
            private readonly byte[] buffer;

            public List<string> RecieveStrs = new List<string>();

            public long Ticks { get; set; }

            public Client(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
                int bufferSize = tcpClient.ReceiveBufferSize;
                buffer = new byte[bufferSize];
            }

            public TcpClient TcpClient
            {
                get { return tcpClient; }
            }

            public byte[] Buffer
            {
                get { return buffer; }
            }

            public NetworkStream NetworkStream
            {
                get
                { 
                    return tcpClient.GetStream(); 
                }
            }
        }
    } 
}
