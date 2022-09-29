using CommunicationLib.Core.Network;
using DdkTestTool.Events;
using DdkTestTool.Extensions;
using DdkTestTool.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DdkTestTool.ViewModels.SocketModels
{
    public class SocketIndexViewModel:BindableBase, INavigationAware
    {
        #region Property
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;
        private readonly IEventAggregator aggregator;
        private TreeItem TreeViewSelectedNode;
        private TreeItem TreeViewSelectedRootNode;
        /// <summary>
        /// 树集合
        /// </summary>
        private ObservableCollection<TreeItem> treeItems;
        public ObservableCollection<TreeItem> TreeItems
        {
            get { return treeItems; }
            set
            {
                treeItems = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Construction
        public SocketIndexViewModel(IRegionManager regionManager, IDialogService dialogService, IEventAggregator aggregator)
        {
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.aggregator = aggregator;
            InitTreeItems();
            ExecuteTreeViewCommand = new DelegateCommand<string>(ExecuteTreeView);
            NavigateToCommand = new DelegateCommand<TreeItem>(NavigateTo);

            aggregator.GetEvent<UpdateTreeViewEvent>().Subscribe(model =>
            {
                if (TreeItems == null)
                    return;
                try
                {
                    if (model.TreeItem.Layer == 1)
                    {
                        var treeL1 = TreeItems.First(l1 => l1.Tag.Equals(model.TreeItem.ParentTag));
                        if (treeL1 != null)
                        {
                            var treeL2 = treeL1.Children.First(l2 => l2.Tag == model.TreeItem.Tag);
                            if(treeL2 != null)
                            {
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    SynchronizationContext.SetSynchronizationContext(new
                                        System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                    SynchronizationContext.Current.Post(pl =>
                                    {
                                        switch (model.Type)
                                        {
                                            case UpdateTreeViewModel.UpdateType.Update:
                                                treeL2.Name =model.TreeItem.Name;
                                                treeL2.Tag = model.TreeItem.Tag;
                                                treeL2.ImageSource = model.TreeItem.ImageSource; 
                                                break;
                                        }
                                    }, null);
                                });
                            }

                        }
                    }
                    else if (model.TreeItem.Layer == 2)
                    {
                        var treeL1 = TreeItems.First(l1 => l1.Children.Contains(
                        l1.Children.First(l2 => l2.Tag.Equals(model.TreeItem.ParentTag))));

                        if (treeL1 != null)
                        {
                            var treeL2 = treeL1.Children.First(l2 => l2.Tag.Equals(model.TreeItem.ParentTag));
                            if (treeL2 != null)
                            {
                                model.TreeItem.Parent = treeL2;
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    System.Threading.SynchronizationContext.SetSynchronizationContext(new
                                        System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                    System.Threading.SynchronizationContext.Current.Post(pl =>
                                    {
                                        switch (model.Type)
                                        {
                                            case UpdateTreeViewModel.UpdateType.Add:
                                                var treeItem = treeL2.Children.FirstOrDefault(t => t.Tag.Equals(model.TreeItem.Tag));
                                                if(treeItem == null)
                                                    treeL2.Children.Add(model.TreeItem);
                                                break;
                                            case UpdateTreeViewModel.UpdateType.Remove:
                                                var treeItem1 = treeL2.Children.FirstOrDefault(t => t.Tag.Equals(model.TreeItem.Tag));
                                                if (treeItem1 != null)
                                                    treeL2.Children.Remove(treeItem1);
                                                break;
                                        }
                                    }, null);
                                });
                            }
                        }
                    }
                }
                catch
                {

                }
            });
        }
        #endregion

        #region Command
        public DelegateCommand<string> ExecuteTreeViewCommand{get;private  set;}
        public DelegateCommand<TreeItem> NavigateToCommand { get;private set;}
        #endregion

        #region Method
        /// <summary>
        /// 初始化树
        /// </summary>
        private void InitTreeItems()
        {
            TreeItems = new ObservableCollection<TreeItem>()
            {
                new TreeItem("TcpServer","TcpServer",0){ImageSource="/Images/server.png"},
                new TreeItem("TcpClient","TcpClient",0){ImageSource="/Images/client.png"},
                new TreeItem("UdpServer","UdpServer",0){ImageSource="/Images/server.png"},
                new TreeItem("UdpClient","UdpClient",0){ImageSource="/Images/client.png"},
            };
        }

        /// <summary>
        /// 对树节点进行增删
        /// </summary>
        /// <param name="obj">节点操作类型</param>
        private void ExecuteTreeView(string obj)
        {
            if (string.IsNullOrEmpty(obj) || TreeViewSelectedRootNode == null || TreeViewSelectedNode == null)
                return;
            switch(obj)
            {
                case "Add":
                    if((string)TreeViewSelectedRootNode.Tag == "TcpServer")
                    {
                        dialogService.ShowDialog("AddServerDialog", null, dialogResult =>
                        {
                            if (dialogResult.Result != ButtonResult.OK && !dialogResult.Parameters.ContainsKey("Port"))
                                return;
                            var port = dialogResult.Parameters.GetValue<int>("Port");
                            if (NetworkTools.IsPort(port))
                            { 
                                DdkTcpServer tcpServer = new DdkTcpServer(NetworkTools.GetLocalIp(),port);
                                TreeItem child = new TreeItem($"{NetworkTools.GetLocalIp()} [{port}]", tcpServer, 1) { ImageSource = "/Images/circle-Red.png", Parent = TreeViewSelectedRootNode };
                                TreeViewSelectedRootNode.Children.Add(child);
                            }
                        });
                    }
                    else if((string)TreeViewSelectedRootNode.Tag == "TcpClient")
                    {
                        dialogService.ShowDialog("AddClientDialog", null, dialogResult =>
                        {
                            if (dialogResult.Result != ButtonResult.OK && !dialogResult.Parameters.ContainsKey("Port") && !dialogResult.Parameters.ContainsKey("Ip"))
                                return;
                            int port = dialogResult.Parameters.GetValue<int>("Port");
                            var ip = dialogResult.Parameters.GetValue<string>("Ip"); 

                            if (!string.IsNullOrEmpty(ip))
                            {
                                try
                                { 
                                    DdkTcpClient tcpClient = new DdkTcpClient(ip, port);
                                    tcpClient.DisConnectEvent += TcpClient_DisConnectEvent;
                                    tcpClient.Connect();
                                    TreeItem child = new TreeItem($"{ip} [{port}]", tcpClient, 1) { ImageSource = "/Images/circle-Green.png", Parent = TreeViewSelectedRootNode };
                                    SocketManager.ddkTcpClients.Add(tcpClient);
                                    TreeViewSelectedRootNode.Children.Add(child);
                                }
                                catch
                                {

                                }
                            }
                        });
                    }
                    break;
                case "Delete": 
                    if (TreeViewSelectedNode.Layer == 1 && TreeViewSelectedNode.Parent != null)
                    {
                        switch (TreeViewSelectedNode.Parent.Tag)
                        {
                            case "TcpServer":
                                var _server = SocketManager.ddkTcpServers.First(s => s.server.LocalEndpoint.ToString().Equals(TreeViewSelectedNode.Tag));
                                if (_server != null)
                                {
                                    _server.Disconnect();
                                    SocketManager.ddkTcpServers.Remove(_server);
                                }
                                TreeViewSelectedNode.Parent.Children.Remove(TreeViewSelectedNode);
                                regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("WelcomeView");
                                break;
                            case "TcpClient":
                                var _client = SocketManager.ddkTcpClients.First(c => c.Equals(TreeViewSelectedNode.Tag));
                                if (_client != null)
                                {
                                    _client.DisConnect();
                                    SocketManager.ddkTcpClients.Remove(_client);
                                } 
                                break;
                            case "UdpClient":
                                break;
                        }
                    }
                    else if (TreeViewSelectedNode.Layer == 2 && TreeViewSelectedNode.Parent != null && TreeViewSelectedNode.Parent.Parent != null)
                    {
                        switch (TreeViewSelectedNode.Parent.Parent.Tag)
                        {
                            case "TcpServer":
                                var _server = SocketManager.ddkTcpServers.First(s => s.server.LocalEndpoint.ToString().Equals(TreeViewSelectedNode.Parent.Tag));
                                if (_server != null)
                                {
                                    var _client = _server.tcpClientList.First(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals(TreeViewSelectedNode.Tag));
                                    if (_client != null)
                                    {
                                        _server.OnClientDisconnected(_client);
                                    }
                                } 
                                break;
                        }
                    }
                    break;
            }
        }

        private void TcpClient_DisConnectEvent(DdkTcpClient obj)
        {
            var l1 = TreeItems.FirstOrDefault(l1 => (string)l1.Tag == "TcpClient");
            if(l1 != null)
            {
                var l2 = l1.Children.FirstOrDefault(l2 => l2.Tag == obj);
                if(l2 != null)
                {
                    l1.Children.Remove(l2);
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("WelcomeView");
                }
            }
        }

        private void NavigateTo(TreeItem treeItem)
        {
            if (treeItem == null)
                return;

            TreeViewSelectedNode = treeItem;
            if(treeItem.Layer == 0)
            {
                TreeViewSelectedRootNode = treeItem;
            }
            if (treeItem.Layer == 1)
            { 
                if (treeItem.Parent != null)
                {
                    TreeViewSelectedRootNode = treeItem.Parent;
                     
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Tag", treeItem.Tag); 
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("SocketView", param);
                }
            }
            else if (treeItem.Layer == 2)
            {
                if (treeItem.Parent != null && treeItem.Parent.Parent != null)
                {
                    TreeViewSelectedRootNode = treeItem.Parent.Parent; 
                     
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Tag", treeItem.Tag); 
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("SocketView", param);
                }
            }

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        { 
            regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("WelcomeView");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { 
        }


        #endregion
    }
}
