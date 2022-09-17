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
using System.Text;
using System.Threading.Tasks; 

namespace DdkTestTool.ViewModels.SocketModels
{ 
    public class SocketViewModel:BindableBase, INavigationAware
    {

        #region 属性 

        private readonly IEventAggregator aggregator;
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
                        ClientIp = string.Empty;
                        ClientPort = string.Empty;
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
        private string connectButtonText = "开始监听";

        public string ConnectButtonText
        {
            get { return connectButtonText; }
            set { connectButtonText = value; RaisePropertyChanged(); }
        }

        #endregion

        public SocketViewModel(IEventAggregator aggregator)
        {
            ConnectCommand = new DelegateCommand(Connect);
            DisConnectCommand = new DelegateCommand(DisConnect);
            SendDataCommand = new DelegateCommand(SendMsg);
            this.aggregator = aggregator;
            this.aggregator = aggregator;
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
            if (navigationContext.Parameters["Port"] == null
                || navigationContext.Parameters["Type"] == null
                || navigationContext.Parameters["Ip"] == null)
                return;
            string _port = (string)navigationContext.Parameters["Port"];
            string _ip = (string)navigationContext.Parameters["Ip"];
            string _type = (string)navigationContext.Parameters["Type"];
            try
            {
                switch (_type)
                {
                    case "TcpServer":
                        Ip = _ip;
                        Port = int.Parse(_port);
                        CurrentViewType = SocketManager.ViewType.TcpServer;
                        var _server = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals($"{Ip}:{Port}"));
                        if (_server == null)
                        {
                            IsServerOpened = false;
                            tcpServer = new DdkTcpServer(Ip, Port);
                            tcpServer.ClientConnectedEvent += Server_ClientConnectedEvent;
                            tcpServer.RecieveClientMessageEvent += Server_RecieveClientMessageEvent;
                            tcpServer.ClientDisconnectedEvent += Server_ClientDisconnectedEvent;
                            SocketManager.ddkTcpServers.Add(tcpServer);
                        }
                        else
                        {
                            tcpServer = _server;
                            IsServerOpened = tcpServer.IsOpen;
                        }
                        break;
                    case "TcpServerClient":
                        if (navigationContext.Parameters["Parent"] == null)
                            return;
                        string _parent = (string)navigationContext.Parameters["Parent"];
                        CurrentViewType = SocketManager.ViewType.TcpServerClient;
                        ClientIp = _ip;
                        ClientPort = _port;
                        tcpServer = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(_parent));
                        tcpServerClient = tcpServer.tcpClientList.FirstOrDefault(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals($"{clientIp}:{clientPort}"));
                        DataRecieveStr = string.Empty;
                        foreach (var str in tcpServerClient.RecieveStrs.ToArray())
                        {
                            DataRecieveStr += str;
                        }
                        break;
                    case "TcpClient":
                        if (navigationContext.Parameters["LocalPort"] == null)
                            return;
                        string _localPort = (string)navigationContext.Parameters["LocalPort"]; 
                        CurrentViewType = SocketManager.ViewType.TcpClient;
                        Ip = NetworkTools.GetLocalIp(); 
                        ClientIp = _ip;
                        ClientPort = _port;

                        var _tcpClient = SocketManager.ddkTcpClients.FirstOrDefault(c => c.Client.Client.RemoteEndPoint.ToString().Equals($"{Ip}:{Port}"));
                        if (_tcpClient == null)
                        {
                            IsServerOpened = false;
                            tcpClient = new DdkTcpClient(Ip, Port);
                        }
                        else
                        {
                            tcpClient = _tcpClient;
                            IsServerOpened = tcpClient.IsOpen;
                        }
                        break; 
                }   
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private void DisConnect()
        {
            if (currentViewType == SocketManager.ViewType.TcpServer && IsServerOpened)
            {
                tcpServer.Disconnect();
                IsServerOpened = false;
                ServerStatus = "未启动";
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                 new TreeItem(NetworkTools.EndPointToUIString(tcpServer.server.LocalEndpoint), tcpServer.server.LocalEndpoint.ToString(), 1) { ImageSource = "/Images/circle-Red.png", ParentTag = "TcpServer" },
                UpdateTreeViewModel.UpdateType.Update));
            }
            else if (currentViewType == SocketManager.ViewType.TcpServerClient)
            {
                if (tcpServerClient != null)
                    tcpServer.OnClientDisconnected(tcpServerClient);
            }
        }

        private void Connect()
        {
            try
            { 
                tcpServer.Connect();
                IsServerOpened = true;
                ServerStatus = "已启动"; 
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                    new TreeItem(NetworkTools.EndPointToUIString(tcpServer.server.LocalEndpoint), tcpServer.server.LocalEndpoint.ToString(), 1) { ImageSource = "/Images/circle-Green.png", ParentTag = "TcpServer" },
                    UpdateTreeViewModel.UpdateType.Update));
            }
            catch
            {

            }
        }
        private void Server_ClientDisconnectedEvent(DdkTcpServer.Client client)
        {
            var tcpServer = SocketManager.ddkTcpServers.FirstOrDefault(s => s.server.LocalEndpoint.ToString().Equals(client.TcpClient.Client.LocalEndPoint.ToString()));
            if (tcpServer != null && tcpServer.IsOpen)
            { 
                aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                    new TreeItem(NetworkTools.EndPointToUIString(client.TcpClient.Client.RemoteEndPoint), client.TcpClient.Client.RemoteEndPoint.ToString(), 2) { ImageSource = "/Images/circle-Red.png", ParentTag = client.TcpClient.Client.LocalEndPoint.ToString() },
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
                client.RecieveStrs.Add(msg);
                 
                if (currentViewType == SocketManager.ViewType.TcpServerClient && (endPoint.ToString().Equals($"{clientIp}:{clientPort}"))) 
                    DataRecieveStr += msg;  
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        private void Server_ClientConnectedEvent(DdkTcpServer.Client client)
        {
            if (client.TcpClient.Client.RemoteEndPoint == null)
                return;
            string? endPoint = client.TcpClient.Client.RemoteEndPoint.ToString();

            if (endPoint == null)
                return;
             
            aggregator.GetEvent<UpdateTreeViewEvent>().Publish(new UpdateTreeViewModel(
                        new TreeItem(NetworkTools.EndPointToUIString(client.TcpClient.Client.RemoteEndPoint), endPoint, 2) { ImageSource = "/Images/circle-Green.png", ParentTag = client.TcpClient.Client.LocalEndPoint.ToString() }));
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
                    break;
            }
            DataSendStr = string.Empty;
        }
        #endregion
    }
}
