using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationLib.Core.Network
{ 
    public class DdkTcpClient : IDisposable
    {
        public TcpClient Client { get; private set; } 
        private IPAddress LocalIPAddress = IPAddress.Any;
        public List<string> Log = new List<string>();  
        public NetworkStream NetworkStream { get; private set; }
        private byte[] ReceiveBuffer;

        public EndPoint LocalEndPoint { get; private set; }
        public EndPoint RemoteEndPoint { get; private set; }
        /// <summary>
        /// 推送收到服务器信息事件
        /// </summary>
        public Action<DdkTcpClient, byte[]> RecieveServerMessageEvent;

        public event Action<DdkTcpClient> DisConnectEvent;

        /// <summary>
        /// 推送异常信息事件
        /// </summary>
        public Action<string> PushExceptionMessageEvent;

        #region 构造
        public DdkTcpClient()
        {

        } 
        public DdkTcpClient(IPEndPoint localEP)
        {
            Client = new TcpClient(localEP); 
        }
        public DdkTcpClient(string hostname, int port)
        {
            Client = new TcpClient(hostname,port);
        }
        public DdkTcpClient(IPAddress ip, int port)
        {
            Client = new TcpClient(new IPEndPoint(ip,port));
        }
        public DdkTcpClient(int port)
        {
            Client = new TcpClient(new IPEndPoint(LocalIPAddress, port));
        }
        #endregion
        
        public void Connect()
        { 
            NetworkStream = Client.GetStream();
            NetworkStream.ReadTimeout = 4000;
            ReceiveBuffer = new byte[Client.ReceiveBufferSize];
            NetworkStream.BeginRead(ReceiveBuffer, 0, Client.ReceiveBufferSize, ReadCallBack, null); 
        }

        public void DisConnect()
        {
            Client.Client.Disconnect(false);
            DisConnectEvent?.Invoke(this);
        }

        public void Send(string msg)
        { 
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            if (Client.Connected)
                NetworkStream.Write(sendMsg, 0, sendMsg.Length);
        }

        public async Task SendAsync(string msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            byte[] sendMsg = Encoding.UTF8.GetBytes(msg);
            if (Client.Connected)
                await NetworkStream.WriteAsync(sendMsg, 0, sendMsg.Length);
        }

        private void ReadCallBack(IAsyncResult ir)
        {
            try
            {
                int read = NetworkStream.EndRead(ir);
                if (read == 0)
                {
                    //OnClientDisconnected(client);
                    return;
                }
                byte[] data = new byte[read];
                Buffer.BlockCopy(ReceiveBuffer, 0, data, 0, read);
                Log.Add($"[{DateTime.Now.ToShortTimeString()}]收到: {Encoding.UTF8.GetString(data)}\r\n");
                RecieveServerMessageEvent?.Invoke(this, data);
                NetworkStream.BeginRead(ReceiveBuffer, 0, ReceiveBuffer.Length, ReadCallBack, null);
            }
            catch
            {

            }
        }

        public void Close()
        { 
            ((IDisposable)this).Dispose(); 
        }

        protected virtual void Dispose(bool disposing)
        {   
            if (disposing)
            {
                IDisposable dataStream = NetworkStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                Log = null;
                Client.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        ~DdkTcpClient()
        {
            Dispose(disposing: false);
        }
    }
}
