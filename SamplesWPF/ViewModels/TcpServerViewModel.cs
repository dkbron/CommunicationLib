using CommunicationLib.Core.Network;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SamplesWPF.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.ViewModels
{
    public class TcpServerViewModel : BindableBase, INavigationAware
    {
        private DdkTcpServer server;
        private DdkTcpServer.Client tcpClient;

        public TcpServerViewModel(IEventAggregator aggregator)
        { 
            OpenServerCommand = new DelegateCommand(OpenServer);
            CloseServerCommand = new DelegateCommand(CloseServer);
            SendDataCommand = new DelegateCommand(SendMsg);
            this.aggregator = aggregator;
        }

        #region 属性



        private SocketManager.ViewType currentViewType;
        public SocketManager.ViewType CurrentViewType
        {
            get { return currentViewType; }
            set 
            { 
                currentViewType = value;
                switch (currentViewType)
                {
                    case SocketManager.ViewType.TcpServer:
                        ClientIp = string.Empty;
                        ClientPort = string.Empty;
                        CloseButtonText = "停止监听";
                        break;
                    case SocketManager.ViewType.TcpServerClient:
                        CloseButtonText = "断开";
                        break;
                    case SocketManager.ViewType.TcpClient:
                        break;
                }

                RaisePropertyChanged();
            }
        }
          
        private int port;

        public int Port
        {
            get { return port; }
            set { port = value; RaisePropertyChanged(); }
        }

        private string ip;

        public string Ip
        {
            get { return ip; }
            set { ip = value; RaisePropertyChanged(); }
        }

        private string clientPort;

        public string ClientPort
        {
            get { return clientPort; }
            set { clientPort = value; RaisePropertyChanged(); }
        }

        private string clientIp;

        public string ClientIp
        {
            get { return clientIp; }
            set { clientIp = value; RaisePropertyChanged(); }
        }

        private bool isServerOpened = false;

        public bool IsServerOpened
        {
            get { return isServerOpened; }
            set { isServerOpened = value; RaisePropertyChanged(); }
        }

        private string serverStatus = "未启动";

        public string ServerStatus
        {
            get { return serverStatus; }
            set { serverStatus = value; RaisePropertyChanged(); }
        }

        private string dataRecieveStr;

        public string DataRecieveStr
        {
            get { return dataRecieveStr; }
            set { dataRecieveStr = value; RaisePropertyChanged(); }
        }

        private string dataSendStr;

        public string DataSendStr
        {
            get { return dataSendStr; }
            set { dataSendStr = value; RaisePropertyChanged(); }
        }

        private int sendTimesIndex = 0;

        public int SendTimesIndex
        {
            get { return sendTimesIndex; }
            set { sendTimesIndex = value; RaisePropertyChanged(); }
        }

        private string closeButtonText = "停止监听";

        public string CloseButtonText
        {
            get { return closeButtonText; }
            set { closeButtonText = value; RaisePropertyChanged(); }
        } 



        public DelegateCommand OpenServerCommand { get; private set; }
        public DelegateCommand CloseServerCommand { get; private set; } 
        public DelegateCommand SendDataCommand { get; private set; }
        public IEventAggregator aggregator { get; }

        #endregion


        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["Port"] == null 
                || navigationContext.Parameters["Type"] == null 
                || navigationContext.Parameters["Ip"] == null)
                return;
            string _port = (string)navigationContext.Parameters["Port"];
            string _ip = (string)navigationContext.Parameters["Ip"];
            string _type = (string)navigationContext.Parameters["Type"]; 
            try
            { 

                if(_type == "TcpServer")
                {
                    Ip = _ip;
                    Port = int.Parse(_port);
                    CurrentViewType = SocketManager.ViewType.TcpServer;
                    var _server = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals($"{Ip}:{Port}"));
                    if (_server == null)
                    {
                        IsServerOpened = false;
                        server = new DdkTcpServer(Ip,Port);
                        server.ClientConnectedEvent += Server_ClientConnectedEvent;
                        server.RecieveClientMessageEvent += Server_RecieveClientMessageEvent;
                        server.ClientDisconnectedEvent += Server_ClientDisconnectedEvent;
                        SocketManager.ddkTcpServers.Add(server);
                    }
                    else
                    {
                        server = _server;
                        IsServerOpened = server.IsOpen;
                    }
                }
                if(_type == "TcpServerClient")
                { 
                    if (navigationContext.Parameters["Parent"] == null)
                        return; 
                    string _parent = (string)navigationContext.Parameters["Parent"];
                    CurrentViewType = SocketManager.ViewType.TcpServerClient; 
                    ClientIp = _ip;
                    ClientPort = _port;
                    server = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(_parent));
                    tcpClient = server.tcpClientList.FirstOrDefault(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals($"{clientIp}:{clientPort}"));
                    DataRecieveStr = string.Empty;
                    foreach (var str in tcpClient.Log.ToArray())
                    { 
                        DataRecieveStr += str;
                    }
                }  

            }
            catch
            {

            }
        }
         
        private void CloseServer()
        {
            if (currentViewType == SocketManager.ViewType.TcpServer &&  IsServerOpened)
            {
                server.Disconnect();
                IsServerOpened = false;
                ServerStatus = "未启动";
            } 
            else if(currentViewType == SocketManager.ViewType.TcpServerClient)
            { 
                if(tcpClient != null)
                    server.OnClientDisconnected(tcpClient);
            }
        }

        private void OpenServer()
        { 
            try
            {
                //server.CreateListenerAndConnect(Port);
                server.Connect();
                IsServerOpened = true;
                ServerStatus = "已启动";
            }
            catch
            {

            }
        }
        private void Server_ClientDisconnectedEvent(DdkTcpServer.Client client)
        {
            switch (currentViewType)
            {
                case SocketManager.ViewType.TcpServer:
                    DataRecieveStr += $"客户端:{client.TcpClient.Client.RemoteEndPoint} 已断开连接\r\n";
                    aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel()
                    {
                        NodeName = client.TcpClient.Client.RemoteEndPoint.ToString(),
                        ParentName = server.server.LocalEndpoint.ToString(),
                        Type = UpdateTreeViewModel.UpdateType.Remove
                    });
                    break;
            }
        }

        private void Server_RecieveClientMessageEvent(DdkTcpServer.Client _client, byte[] data)
        { 
            string msg = $"收到客户端[{_client.TcpClient.Client.RemoteEndPoint}]信息:{Encoding.UTF8.GetString(data)}\r\n";
            var client = server.tcpClientList.FirstOrDefault(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals(_client.TcpClient.Client.RemoteEndPoint.ToString()));
            client.Log.Add(msg);

            if(currentViewType == SocketManager.ViewType.TcpServer)
                DataRecieveStr += msg;
            else if(currentViewType == SocketManager.ViewType.TcpServerClient)
            {
                if (_client.TcpClient.Client.RemoteEndPoint.ToString().Equals($"{clientIp}:{clientPort}"))
                {
                    DataRecieveStr += msg;
                }
            }
        }

        private void Server_ClientConnectedEvent(DdkTcpServer.Client client)
        {
            if (currentViewType != SocketManager.ViewType.TcpServer 
                || client.TcpClient.Client.RemoteEndPoint == null)
                return; 
            string? endPoint = client.TcpClient.Client.RemoteEndPoint.ToString();

            if (endPoint == null)
                return;

            DataRecieveStr += $"客户端:{endPoint} 已连接\r\n"; 
            aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel() { NodeName = endPoint, ParentName = server.server.LocalEndpoint.ToString(), Type = UpdateTreeViewModel.UpdateType.Add});
        }

        private async void SendMsg()
        {
            if (string.IsNullOrEmpty(DataSendStr))
                return;

            double sendTimes = Math.Pow(10, SendTimesIndex);
            switch (currentViewType)
            {
                case SocketManager.ViewType.TcpServer:
                    for (int i = 0; i < sendTimes; i++) 
                        await server.SendToAllClientAsync(DataSendStr); 
                    break;
                case SocketManager.ViewType.TcpServerClient:
                    for (int i = 0; i < sendTimes; i++)
                        await server.SendAsync(tcpClient, DataSendStr);
                    break;
            } 
            DataSendStr = string.Empty;
        } 
    }
}
