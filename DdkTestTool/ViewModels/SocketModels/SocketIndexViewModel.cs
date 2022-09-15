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
    public class SocketIndexViewModel:BindableBase
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
                                                treeL2.Children.Add(model.TreeItem);
                                                break;
                                            case UpdateTreeViewModel.UpdateType.Remove:
                                                var treeItem = treeL2.Children.First(t => t.Tag.Equals(model.TreeItem.Tag));
                                                if (treeItem != null)
                                                    treeL2.Children.Remove(treeItem);
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
            if (string.IsNullOrEmpty(obj) || TreeViewSelectedRootNode == null)
                return;
            switch(obj)
            {
                case "Add":
                    if(TreeViewSelectedRootNode.Tag == "TcpServer")
                    {
                        dialogService.ShowDialog("AddServerDialog", null, dialogResult =>
                        {
                            if (dialogResult.Result != ButtonResult.OK && !dialogResult.Parameters.ContainsKey("Port"))
                                return;
                            var port = dialogResult.Parameters.GetValue<string>("Port");
                            if (!string.IsNullOrEmpty(port))
                            {
                                TreeItem child = new TreeItem($"{NetworkTools.GetLocalIp()} [{port}]", $"{NetworkTools.GetLocalIp()}:{port}", 1) { ImageSource = "/Images/circle-Red.png", Parent = TreeViewSelectedRootNode };
                                TreeViewSelectedRootNode.Children.Add(child);
                            }
                        });
                    } 
                    break;
                case "Delete":
                    //TreeItems[0].ImageSource = "/Images/circle-Red.png"; 
                    A = "/Images/circle-Green.png";
                    TreeItems[0].Name = "fffzzz"; 
                    break;
            }
        }

        //private BitmapImage a = new BitmapImage(new Uri("pack://application:,,,/Images/circle-Red.png"));

        //public BitmapImage A
        //{
        //    get { return a; }
        //    set { a = value; RaisePropertyChanged(); }
        //}
        private string a = "/Images/circle-Red.png";

        public string A
        {
            get { return a; }
            set { a = value; RaisePropertyChanged(); }
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

                    string ip = treeItem.Tag.Split(':')[0];
                    string port = treeItem.Tag.Split(':')[1];
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Port", port);
                    param.Add("Ip", ip);
                    if(treeItem.Parent.Tag == "TcpServer")
                        param.Add("Type", "TcpServer");
                    else if(treeItem.Parent.Tag == "TcpClient")
                        param.Add("Type", "TcpClient");
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("SocketView", param);
                }
            }
            else if (treeItem.Layer == 2)
            {
                if (treeItem.Parent != null && treeItem.Parent.Parent != null)
                {
                    TreeViewSelectedRootNode = treeItem.Parent.Parent; 

                    string ip = treeItem.Tag.Split(':')[0];
                    string port = treeItem.Tag.Split(':')[1];
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Parent", treeItem.Parent.Tag);
                    param.Add("Port", port);
                    param.Add("Ip", ip);
                    param.Add("Type", "TcpServerClient");
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("SocketView", param);
                }
            }

        } 


        #endregion
    }
}
