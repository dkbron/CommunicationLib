using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLib.Core.Network
{ 
    public class DdkUdpClient:IDisposable
    {
        private Socket client = null;

        public bool IsOpen = false; 
        public AddressFamily m_Family { get; private set; }

        public event EventHandler<PushUdpClientMsgArgs> PushUdpClientMsg;

        public DdkUdpClient(IPEndPoint localEP)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }

            CreateClientAndConnect(localEP); 
        }

        public DdkUdpClient(IPAddress iPAddress, int port)
        {
            if (iPAddress == null)
            {
                throw new ArgumentNullException("iPAddress");
            }

            if (!NetworkTools.IsPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }

            CreateClientAndConnect(iPAddress, port);    
        }

        public void CreateClientAndConnect(IPAddress iPAddress, int port)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);
            CreateClientAndConnect(iPEndPoint);
        }

        public void CreateClientAndConnect(IPEndPoint iPEndPoint)
        { 
            m_Family = iPEndPoint.AddressFamily; 
            client = new Socket(m_Family, SocketType.Dgram, ProtocolType.Udp); 
            client.Bind(iPEndPoint);
            new Task(RecieveThread).Start(); 
            IsOpen = true;
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                IsOpen = false;
            }
            catch (Exception ex)
            { 
            }
        }

        public void RecieveThread()
        {
            while(true)
            {
                if (!IsOpen)
                    return;
                try
                {
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint remoteEP = (EndPoint)(sender);
                    byte[] reciByte = new byte[1024]; 
                    client.ReceiveFrom(reciByte, ref remoteEP);
                    var reciMsg = Encoding.UTF8.GetString(reciByte).ToString();
                    PushUdpClientMsg?.Invoke(null, new PushUdpClientMsgArgs() { RemoteEP=remoteEP, ReciMsg = reciMsg});

                }
                catch(Exception ex)
                {

                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class PushUdpClientMsgArgs:EventArgs
    {
        public EndPoint RemoteEP { get; set; }
        public string ReciMsg { get; set; }
    }
}
