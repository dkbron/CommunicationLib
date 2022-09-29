using CommunicationLib.Core.Network;
using DdkTestTool.Events;
using DdkTestTool.Extensions;
using DdkTestTool.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks; 

namespace DdkTestTool.ViewModels.SocketModels
{ 
    public class SocketViewModel:BindableBase, INavigationAware
    {

        #region 属性 

        private readonly IEventAggregator aggregator;
        private readonly IRegionManager regionManager;
        private DdkTcpServer tcpServer;
        private DdkTcpServer.Client tcpServerClient;
        private DdkTcpClient tcpClient;

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
                        CloseButtonText = "停止监听";
                        break;
                    case SocketManager.ViewType.TcpServerClient:
                        CloseButtonText = "断开";
                        break;
                    case SocketManager.ViewType.TcpClient:
                        CloseButtonText = "断开";
                        ConnectButtonText = "连接";
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

        private int clientPort;

        public int ClientPort
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
            set 
            { 
                isServerOpened = value;
                ServerStatus = isServerOpened ? "已启动" : "未启动";
                RaisePropertyChanged(); 
            }
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
        private string connectButtonText = "开始监听";

        public string ConnectButtonText
        {
            get { return connectButtonText; }
            set { connectButtonText = value; RaisePropertyChanged(); }
        }

        #endregion

        public SocketViewModel(IEventAggregator aggregator, IRegionManager regionManager)
        {
            ConnectCommand = new DelegateCommand(Connect);
            DisConnectCommand = new DelegateCommand(DisConnect);
            SendDataCommand = new DelegateCommand(SendMsg);
            this.aggregator = aggregator;
            this.regionManager = regionManager;
        }
        #region Command

        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand DisConnectCommand { get; private set; }
        public DelegateCommand SendDataCommand { get; private set; }
        #endregion

        #region method

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["Tag"] == null)
                return;
            DataRecieveStr = string.Empty;
            var _tag = navigationContext.Parameters["Tag"]; 
            if(_tag is DdkTcpServer _server)
            {
                try
                {
                    CurrentViewType = SocketManager.ViewType.TcpServer;
                    Ip = NetworkTools.EndPointToIPEndPoint(_server.server.LocalEndpoint).Address.ToString();
                    Port = NetworkTools.EndPointToIPEndPoint(_server.server.LocalEndpoint).Port;
                    tcpServer = _server;
                    IsServerOpened = tcpServer.IsOpen;  
                    tcpServer.ClientConnectedEvent = Server_ClientConnectedEvent;
                    tcpServer.RecieveClientMessageEvent = Server_RecieveClientMessageEvent;
                    tcpServer.ClientDisconnectedEvent = Server_ClientDisconnectedEvent;
                    SocketManager.ddkTcpServers.Add(tcpServer); 
                }
                catch
                {
                    throw;
                }
            }
            if(_tag is DdkTcpServer.Client _serverClient)
            {
                CurrentViewType = SocketManager.ViewType.TcpServerClient;
                Ip = NetworkTools.EndPointToIPEndPoint(_serverClient.TcpClient.Client.LocalEndPoint).Address.ToString();  
                Port = NetworkTools.EndPointToIPEndPoint(_serverClient.TcpClient.Client.LocalEndPoint).Port;
                ClientIp = NetworkTools.EndPointToIPEndPoint(_serverClient.TcpClient.Client.RemoteEndPoint).Address.ToString();
                ClientPort = NetworkTools.EndPointToIPEndPoint(_serverClient.TcpClient.Client.RemoteEndPoint).Port;
                tcpServerClient = _serverClient;
                foreach (var str in tcpServerClient.Log.ToArray()) 
                    DataRecieveStr += str; 
            }
            if(_tag is DdkTcpClient _client)
            {
                CurrentViewType = SocketManager.ViewType.TcpClient;
                Ip = NetworkTools.EndPointToIPEndPoint(_client.Client.Client.LocalEndPoint).Address.ToString();
                Port = NetworkTools.EndPointToIPEndPoint(_client.Client.Client.LocalEndPoint).Port;
                ClientIp = NetworkTools.EndPointToIPEndPoint(_client.Client.Client.RemoteEndPoint).Address.ToString();
                ClientPort = NetworkTools.EndPointToIPEndPoint(_client.Client.Client.RemoteEndPoint).Port; 
                tcpClient = _client;
                tcpClient.RecieveServerMessageEvent = Client_RecieveServerMessageEvent; 
                foreach (var str in tcpClient.Log.ToArray())
                    DataRecieveStr += str;
                IsServerOpened = tcpClient.Client.Connected;
            }
        } 
        private void Client_RecieveServerMessageEvent(DdkTcpClient _tcpClient, byte[] bytes)
        {
            string msg = $"收到服务端信息:{Encoding.UTF8.GetString(bytes)}\r\n";
            if(CurrentViewType == SocketManager.ViewType.TcpClient &&tcpClient == _tcpClient) 
                DataRecieveStr += msg; 
        }

        private void DisConnect()
        {
            if (currentViewType == SocketManager.ViewType.TcpServer && IsServerOpened)
            {
                tcpServer.Disconnect();
                IsServerOpened = false; 
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                 new TreeItem(NetworkTools.EndPointToUIString(tcpServer.server.LocalEndpoint), tcpServer, 1) { ImageSource = "/Images/circle-Red.png", ParentTag = "TcpServer" },
                UpdateTreeViewModel.UpdateType.Update));
            }
            else if (currentViewType == SocketManager.ViewType.TcpServerClient)
            {
                if (tcpServerClient != null)
                    tcpServer.OnClientDisconnected(tcpServerClient);
            }
            else if (currentViewType == SocketManager.ViewType.TcpClient)
            {
                tcpClient.DisConnect(); 
            }
        }

        private void Connect()
        {
            try
            {
                switch (currentViewType)
                {
                    case SocketManager.ViewType.TcpServer:
                        tcpServer.Connect();
                        IsServerOpened = true;
                        aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                        new TreeItem(NetworkTools.EndPointToUIString(tcpServer.server.LocalEndpoint), tcpServer, 1) { ImageSource = "/Images/circle-Green.png", ParentTag = "TcpServer" },
                        UpdateTreeViewModel.UpdateType.Update));
                        break;  
                }
            }
            catch
            {

            }
        }

        private void Server_ClientConnectedEvent(DdkTcpServer.Client client)
        {  
            var tcpServer = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(client.TcpClient.Client.LocalEndPoint.ToString()));
            if (tcpServer != null && tcpServer.IsOpen)
            { 
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                        new TreeItem(NetworkTools.EndPointToUIString(client.TcpClient.Client.RemoteEndPoint), client, 2) { ImageSource = "/Images/circle-Green.png", ParentTag = tcpServer }));
            }
        }
        private void Server_ClientDisconnectedEvent(DdkTcpServer.Client client)
        {
            var tcpServer = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(client.TcpClient.Client.LocalEndPoint.ToString()));
            if (tcpServer != null && tcpServer.IsOpen)
            { 
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                    new TreeItem(NetworkTools.EndPointToUIString(client.TcpClient.Client.RemoteEndPoint), client, 2) { ImageSource = "/Images/circle-Red.png", ParentTag = tcpServer },
                    UpdateTreeViewModel.UpdateType.Remove));
            }
        }


        private void Server_RecieveClientMessageEvent(DdkTcpServer.Client _client, byte[] data)
        {
            try
            {
                var endPoint = _client.TcpClient.Client.RemoteEndPoint;
                if (endPoint == null)
                    return;
                string msg = $"收到客户端[{endPoint}]信息:{Encoding.UTF8.GetString(data)}\r\n";

                var tcpServer = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(_client.TcpClient.Client.LocalEndPoint.ToString()));
                if (tcpServer == null || !tcpServer.IsOpen)
                    return;

                var client = tcpServer.tcpClientList.FirstOrDefault(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals(endPoint.ToString()));
                if (client == null)
                    return; 
                 
                if (currentViewType == SocketManager.ViewType.TcpServerClient && (endPoint.ToString().Equals($"{clientIp}:{clientPort}"))) 
                    DataRecieveStr += msg;  
            }
            catch(Exception ex)
            {
                throw ex;
            }

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
                        await tcpServer.SendToAllClientAsync(DataSendStr);
                    break;
                case SocketManager.ViewType.TcpServerClient:
                    for (int i = 0; i < sendTimes; i++)
                        await tcpServer.SendAsync(tcpServerClient, DataSendStr);
                    string sendData = $"[{DateTime.Now.ToShortTimeString()}]发送: {DataSendStr}   ({sendTimes}次)\r\n";
                    DataRecieveStr += sendData;
                    tcpServerClient.Log.Add(sendData);
                    break;
                case SocketManager.ViewType.TcpClient:
                    for (int i = 0; i < sendTimes; i++)
                        await tcpClient.SendAsync(DataSendStr);
                    string sendData1 = $"[{DateTime.Now.ToShortTimeString()}]发送: {DataSendStr}   ({sendTimes}次)\r\n";
                    DataRecieveStr += sendData1;
                    tcpClient.Log.Add(sendData1); 
                    break;
            }
            DataSendStr = string.Empty;
        }
        #endregion
    }
}
