using DdkTestTool.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DdkTestTool.ViewModels
{
    public class WindowViewModel:BindableBase, IConfigureService
    {
        private readonly IRegionManager regionManager;

        public WindowViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void Configure()
        {
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("IndexView");
        }
    }
}
