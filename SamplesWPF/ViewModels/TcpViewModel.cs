using CommunicationLib.Core.Network;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SamplesWPF.Events;
using SamplesWPF.Extensions;
using SamplesWPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SamplesWPF.ViewModels
{
    public class TcpViewModel:BindableBase, INavigationAware
    {
        private readonly IRegionManager regionManager;
        private readonly IDialogHostService dialogHostService;
        private readonly IEventAggregator aggregator;

        public TcpViewModel(IRegionManager regionManager, IDialogHostService dialogHostService, IEventAggregator aggregator)
        {
            this.regionManager = regionManager;
            this.dialogHostService = dialogHostService;
            this.aggregator = aggregator; 

            NavigateCommand = new DelegateCommand<object>(Navigate);
            CreateTree();
            AddTreeCommand = new DelegateCommand(AddTreeChild);
            DeleteTreeCommand = new DelegateCommand(DeleteTreeChild); 
            aggregator.GetEvent<UpdateTreeViewEvent>().Subscribe(args =>
            {
                if (TreeL1s == null)
                    return;
                try
                {
                    string parentName = args.ParentName;
                    string nodeName = args.NodeName;
                    var treeL1 = TreeL1s.FirstOrDefault(l1 => l1.treeL2s.Contains(
                        l1.treeL2s.FirstOrDefault(l2 => l2.Name.Equals(parentName))));

                    if (treeL1 != null)
                    {
                        var treeL2 = treeL1.treeL2s.FirstOrDefault(l2 => l2.Name.Equals(parentName));
                        if (treeL2 != null)
                        {
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                System.Threading.SynchronizationContext.SetSynchronizationContext(new
                                    System.Windows.Threading.DispatcherSynchronizationContext(System.Windows.Application.Current.Dispatcher));
                                System.Threading.SynchronizationContext.Current.Post(pl =>
                                {
                                    switch (args.Type)
                                    {
                                        case UpdateTreeViewModel.UpdateType.Add:
                                            treeL2.treeL3s.Add(new TreeL3($"{nodeName}") { Parent = treeL2 });
                                            break;
                                        case UpdateTreeViewModel.UpdateType.Remove:
                                            var treeL3 = treeL2.treeL3s.FirstOrDefault(l3 => l3.Name.Equals(nodeName));
                                            treeL2.treeL3s.Remove(treeL3);
                                            break;
                                    }
                                }, null);
                            });
                        }
                    }
                }
                catch{

                } 
            });
        }

        

        private void Navigate(object obj)
        {
            if (obj is TreeL1 treeL1)
            {
                TreeViewSelectedRootName = treeL1.Name;
                TreeViewSelectedNode = treeL1;
            }
            else if (obj is TreeL2 treeL2)
            {
                var parent = TreeL1s.FirstOrDefault(l => l.treeL2s.Contains(obj));
                if (parent != null)
                {
                    TreeViewSelectedRootName = parent.Name;
                    TreeViewSelectedNode = treeL2;

                    string ip = treeL2.Name.Split(':')[0];
                    string port = treeL2.Name.Split(':')[1];
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Port", port);
                    param.Add("Ip", ip);
                    param.Add("Type", "TcpServer");
                    regionManager.Regions[PrismManager.TcpViewRegionName].RequestNavigate("TcpServerView", param);
                }
            }
            else if(obj is TreeL3 treeL3)
            {
                if (treeL3.Parent != null && treeL3.Parent.Parent != null && treeL3.Parent.Parent.Name == "TcpServer")
                {
                    TreeViewSelectedRootName = treeL3.Parent.Parent.Name;
                    TreeViewSelectedNode = treeL3;

                    string ip = treeL3.Name.Split(':')[0];
                    string port = treeL3.Name.Split(':')[1];
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Parent", treeL3.Parent.Name);
                    param.Add("Port", port); 
                    param.Add("Ip", ip);
                    param.Add("Type", "TcpServerClient");
                    regionManager.Regions[PrismManager.TcpViewRegionName].RequestNavigate("TcpServerView", param);
                }
            }

        }

        public void CreateTree()
        { 

            //TreeParents = new ObservableCollection<ItemViewModel>()
            //{
            //    new ItemViewModel("TcpServer")
            //    {
            //        Children = new ObservableCollection<ItemViewModel>()
            //        {
            //            new ItemViewModel("server1")
            //            {
            //                Children = new ObservableCollection<ItemViewModel>()
            //                {
            //                    new ItemViewModel("client1")
            //                }
            //            }
            //        }
            //    },
            //    new ItemViewModel("TcpClient"),
            //    new ItemViewModel("UdpServer"),
            //    new ItemViewModel("UdpClient"),
            //};
             

            TreeL1s = new ObservableCollection<TreeL1>()
            {
                new TreeL1("TcpServer"),
                new TreeL1("TcpClient"),
                new TreeL1("UdpClient"), 
            };

            //var l1 = TreeL1s.FirstOrDefault(l1 => l1.Name.Equals("TcpServer"));
            //l1.treeL2s.Add(new TreeL2("l2-1"));
            //l1.treeL2s[0].treeL3s.Add(new TreeL3("l3-1")); 
        }

        private async void AddTreeChild()
        {
            if (string.IsNullOrEmpty(TreeViewSelectedRootName))
            {
                return;
            }

            if (TreeViewSelectedRootName == "TcpServer")
            {
                var dialog = await dialogHostService.ShowDialog("AddTcpServerView", null);

                if (dialog.Result == Prism.Services.Dialogs.ButtonResult.Cancel)
                    return;
                string Port = dialog.Parameters.GetValue<string>("Port");
                if (!string.IsNullOrEmpty(Port))
                {
                    var treeL1 = TreeL1s.FirstOrDefault(l => l.Name == "TcpServer");
                    if (treeL1 != null)
                        treeL1.treeL2s.Add(new TreeL2($"{NetworkTools.GetLocalIp()}:{Port}") { Parent = treeL1});
                }
            }
        }

        private void DeleteTreeChild()
        {
            if (TreeViewSelectedNode == null)
            {
                return;
            }
            if (TreeViewSelectedNode is TreeL2 treeL2 && treeL2.Parent != null)
            {
                switch (treeL2.Parent.Name)
                {
                    case "TcpServer":
                        var _server = SocketManager.ddkTcpServers.First(s => s.server.LocalEndpoint.ToString().Equals(treeL2.Name));
                        if (_server != null)
                        {
                            _server.Disconnect();
                            SocketManager.ddkTcpServers.Remove(_server);
                        }
                        treeL2.Parent.treeL2s.Remove(treeL2);
                        regionManager.Regions[PrismManager.TcpViewRegionName].RequestNavigate("WelcomeView");
                        break;
                    case "TcpClient":
                        break;
                    case "UdpClient":
                        break;
                }
            }
            else if (TreeViewSelectedNode is TreeL3 treeL3 && treeL3.Parent != null && treeL3.Parent.Parent != null)
            {
                switch(treeL3.Parent.Parent.Name)
                {
                    case "TcpServer":
                        var _server = SocketManager.ddkTcpServers.First(s => s.server.LocalEndpoint.ToString().Equals(treeL3.Parent.Name));
                        if(_server != null)
                        {
                            var _client = _server.tcpClientList.First(c => c.TcpClient.Client.RemoteEndPoint.ToString().Equals(treeL3.Name));
                            if(_client != null)
                            { 
                                _server.OnClientDisconnected(_client);
                            } 
                        }
                        treeL3.Parent.treeL3s.Remove(treeL3);
                        break;
                }
            }
        }
         

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            regionManager.Regions[PrismManager.TcpViewRegionName].RequestNavigate("WelcomeView");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        { 
        }



        #region 属性
        public DelegateCommand<object> NavigateCommand { get; private set; }

        private ObservableCollection<MenuBar>? menuBars;

        public ObservableCollection<MenuBar>? MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }


        public ObservableCollection<ItemViewModel> TreeParents { get; private set; }

        public ObservableCollection<TreeL1> TreeL1s { get; private set; }

        public DelegateCommand AddTreeCommand { get; set; }

        public DelegateCommand DeleteTreeCommand { get; private set; }

        /// <summary>
        /// 当前TreeView被选中的节点的根节点名称，添加节点时需要用到
        /// </summary>
        private string TreeViewSelectedRootName { get; set; } = string.Empty;
        /// <summary>
        /// 当前TreeView被选中的节点
        /// </summary>
        private object TreeViewSelectedNode { get; set;  } = string.Empty;

        #endregion
    }
}
