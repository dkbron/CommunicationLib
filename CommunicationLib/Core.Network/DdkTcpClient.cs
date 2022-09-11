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
    public delegate void PushMsg(string msg);
    public class DdkTcpClient
    {
        Socket tcpSocketClient;
        public bool isOpened;

        /// <summary>
        /// 推送收到服务器信息事件
        /// </summary>
        public PushMsg PushServerMsgEvent;

        /// <summary>
        /// 推送异常信息事件
        /// </summary>
        public PushMsg PushExceptionMsgEvent;

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip">服务器ip</param>
        /// <param name="port">服务器端口</param>
        public void OpenDevice(string ip, int port)
        {
            if (!NetworkTools.IsIP(ip) || !NetworkTools.IsPort(port))
                return;

            isOpened = false;
            try
            {
                tcpSocketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                tcpSocketClient.Connect(ipEndPoint);  
                Thread thread = new Thread(RecieveThread);
                thread.Start();

                isOpened = true;
            }
            catch (Exception ex)
            {
                PushExceptionMsgEvent?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// 与服务器断开连接
        /// </summary>
        public void CloseDevice()
        {
            if (!isOpened)
                return;

            try
            {
                tcpSocketClient.Shutdown(SocketShutdown.Both);
                tcpSocketClient.Close();
                isOpened = false;
            }
            catch (Exception ex)
            {
                PushExceptionMsgEvent?.Invoke(ex.Message);
            }
        }

        public void SendMsg(string Msg)
        {
            if (!isOpened)
                return; 
            tcpSocketClient.Send(Encoding.UTF8.GetBytes(Msg));  
        }

        private void RecieveThread()
        {
            while (true)
            {
                if (!isOpened || !tcpSocketClient.Connected)
                {
                    Thread.Sleep(50);
                    continue;
                }

                try
                {
                    byte[] recieveBuffer = new byte[1024];

                    int recieveNum = tcpSocketClient.Receive(recieveBuffer);
                    if (recieveNum > 0)
                    {
                        string receivedText = Encoding.ASCII.GetString(recieveBuffer, 0, recieveNum);
                        PushServerMsgEvent?.Invoke(receivedText);
                    }
                    else
                        Thread.Sleep(20);
                }
                catch (Exception ex)
                {
                    PushExceptionMsgEvent?.Invoke(ex.Message);
                }
            }
        } 
    }
}
