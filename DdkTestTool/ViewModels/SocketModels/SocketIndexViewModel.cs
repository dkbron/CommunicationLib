using CommunicationLib.Core.Network;
using DdkTestTool.Extensions;
using DdkTestTool.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.ViewModels.SocketModels
{
    public class SocketIndexViewModel:BindableBase
    {
        #region Property
        private readonly IRegionManager regionManager;
        private readonly IDialogService dialogService;

        private TreeItem TreeViewSelectedNode;
        private TreeItem TreeViewSelectedRootNode;
        /// <summary>
        /// 树集合
        /// </summary>
        public ObservableCollection<TreeItem> TreeItems { get; private set; }

        #endregion
        public SocketIndexViewModel(IRegionManager regionManager, IDialogService dialogService)
        {
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            InitTreeItems();
            ExecuteTreeViewCommand = new DelegateCommand<string>(ExecuteTreeView);
            NavigateToCommand = new DelegateCommand<TreeItem>(NavigateTo);
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
                    dialogService.ShowDialog("AddServerDialog", null, dialogResult =>
                    {
                        if (dialogResult.Result != ButtonResult.OK && !dialogResult.Parameters.ContainsKey("Port"))
                            return;
                        var port = dialogResult.Parameters.GetValue<string>("Port");
                        if(!string.IsNullOrEmpty(port))
                        {
                            TreeItem child = new TreeItem($"{NetworkTools.GetLocalIp()} [{port}]", $"{NetworkTools.GetLocalIp()}:{port}", 1) { ImageSource="/Images/server.png", Parent= TreeViewSelectedRootNode };
                            TreeViewSelectedRootNode.Children.Add(child);
                        }
                    });
                    break;
                case "Delete":
                    break;
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

                    string ip = treeItem.Tag.Split(':')[0];
                    string port = treeItem.Tag.Split(':')[1];
                    NavigationParameters param = new NavigationParameters();
                    param.Add("Port", port);
                    param.Add("Ip", ip);
                    if(treeItem.Parent.Tag == "TcpServer")
                        param.Add("Type", "TcpServer");
                    else if(treeItem.Parent.Tag == "TcpClient")
                        param.Add("Type", "TcpClient");
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("TcpServerView", param);
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
                    param.Add("Parent", treeItem.Tag);
                    param.Add("Port", port);
                    param.Add("Ip", ip);
                    param.Add("Type", "TcpServerClient");
                    regionManager.Regions[PrismManager.SocketIndexViewRegionName].RequestNavigate("TcpServerView", param);
                }
            }

        } 


        #endregion
    }
}
