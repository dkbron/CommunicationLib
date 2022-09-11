using SamplesWPF.Models;
using SamplesWPF.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.ViewModels
{
    public class SettingViewModel:BindableBase
    {
        public SettingViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            CreateMenuBar();
        }

        private IRegionManager regionManager;
        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.NameSpace))
                return;

            regionManager.Regions[PrismManager.SettingViewRegionName].RequestNavigate(obj.NameSpace);
        }

        private ObservableCollection<MenuBar> menubars;

        public ObservableCollection<MenuBar> Menubars
        {
            get => menubars;
            set
            {
                menubars = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<MenuBar> NavigateCommand { get; set; }

        private void CreateMenuBar()
        {
            Menubars = new ObservableCollection<MenuBar>();
            Menubars.Add(new MenuBar() { Icon= "Palette", NameSpace="SkinView", Title="个性化"});
            Menubars.Add(new MenuBar() { Icon= "Cog", NameSpace="SystemConfigView", Title="系统设置"});
            Menubars.Add(new MenuBar() { Icon= "AlertCircle", NameSpace="AboutMoreView", Title="关于更多"});
        }
    }


}
