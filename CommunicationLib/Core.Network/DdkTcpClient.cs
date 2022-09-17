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
        public bool IsOpen { get; set; } 
        private IPAddress LocalIPAddress = IPAddress.Any;


        public NetworkStream NetworkStream { get; private set; }
        private byte[] ReceiveBuffer;
        /// <summary>
        /// 推送收到服务器信息事件
        /// </summary>
        public Action<byte[]> RecieveServerMessageEvent;

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
            //Client.Connect(remoteEP);
            NetworkStream = Client.GetStream();
            NetworkStream.ReadTimeout = 4000;
            ReceiveBuffer = new byte[Client.ReceiveBufferSize];
            NetworkStream.BeginRead(ReceiveBuffer, 0, Client.ReceiveBufferSize, ReadCallBack, null);
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
                string a = Encoding.UTF8.GetString(data);
                //RecieveClientMessageEvent?.Invoke(client, data);
                NetworkStream.BeginRead(ReceiveBuffer, 0, ReceiveBuffer.Length, ReadCallBack, null);
            }
            catch
            {

            }
        }

        public void Dispose()
        {  
            NetworkStream?.Dispose();
            Client.Close();
        }
    }
}
