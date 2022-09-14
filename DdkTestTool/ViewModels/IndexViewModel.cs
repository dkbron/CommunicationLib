using DdkTestTool.Extensions;
using DdkTestTool.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.ViewModels
{
    public class IndexViewModel : BindableBase
    {
        public IndexViewModel(IRegionManager regionManager)
        {
            NevigateToCommand = new DelegateCommand<Tool>(NavigateTo);
            Tools = new ObservableCollection<Tool>()
            {
                new Tool("Socket Tools","Sockets"){Description = "用于建立Socket通讯的工具，包括:\r\nTcpServer\r\nTcpClient\r\nUdpClient\r\n", ImageSource="/Images/ToolImages/s.png"},  
                new Tool("Socket Tools","Sockets"){Description = "用于建立Socket通讯的工具，包括:\r\nTcpServer\r\nTcpClient\r\nUdpClient\r\n", ImageSource="/Images/ToolImages/H.png"},  
                new Tool("Socket Tools","Sockets"){Description = "用于建立Socket通讯的工具，包括:\r\nTcpServer\r\nTcpClient\r\nUdpClient\r\n", ImageSource="/Images/ToolImages/s.png"},  
                new Tool("Socket Tools","Sockets"){Description = "用于建立Socket通讯的工具，包括:\r\nTcpServer\r\nTcpClient\r\nUdpClient\r\n", ImageSource="/Images/ToolImages/s.png"},  
                new Tool("Socket Tools","Sockets"){Description = "用于建立Socket通讯的工具，包括:\r\nTcpServer\r\nTcpClient\r\nUdpClient\r\n", ImageSource="/Images/ToolImages/s.png"},  
            };
            this.regionManager = regionManager;
        }

        #region Property
        private readonly IRegionManager regionManager;

        /// <summary>
        /// 工具类集合
        /// </summary> 
        public ObservableCollection<Tool> Tools { get; private set; }
        #endregion

        #region Command
        /// <summary>
        /// 导航到相应工具的命令
        /// </summary>
        public DelegateCommand<Tool> NevigateToCommand { get; private set; }
        #endregion

        #region Method
        private void NavigateTo(Tool tool)
        {
            if (tool == null)
                return;
            switch (tool.Tag)
            {
                case "Sockets":
                    regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("SocketIndexView");
                    break;
            }
        }
        #endregion
    }
}
