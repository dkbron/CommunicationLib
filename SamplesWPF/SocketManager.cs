using CommunicationLib.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF
{
    public static class SocketManager
    {
        public enum ViewType
        {
            TcpServer = 0,
            TcpServerClient = 1,
            TcpClient = 2
        }

        public static List<DdkTcpServer> ddkTcpServers = new List<DdkTcpServer>();

        public static List<DdkTcpClient> ddkTcpClients = new List<DdkTcpClient>();

        public static List<DdkUdpClient> ddkUdpClients = new List<DdkUdpClient>();
    }
}


