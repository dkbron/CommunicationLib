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
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.ViewModels
{
    public class MainViewModel:BindableBase, IConfigureService
    {
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;

        public MainViewModel(IRegionManager regionManager)
        { 
            this.regionManager = regionManager;

            CreateMenuBar();

            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);

            GoForwardCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoForward)
                    journal.GoForward();
            });

            GoBackCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoBack)
                    journal.GoBack();
            }); 
        }


        #region 属性
        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        public DelegateCommand GoForwardCommand { get; private set; }
        public DelegateCommand GoBackCommand { get; private set; }

        private ObservableCollection<MenuBar>? menuBars;

        public ObservableCollection<MenuBar>? MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        #endregion

        private void CreateMenuBar()
        {
            MenuBars = new ObservableCollection<MenuBar>();
            MenuBars.Add(new MenuBar() { Icon = "AlphaA", Title = "Tcp", NameSpace = "TcpView" });
            MenuBars.Add(new MenuBar() { Icon = "AlphaB", Title = "Udp", NameSpace = "UdpView" });
            MenuBars.Add(new MenuBar() { Icon = "AlphaC", Title = "ModbusRTU", NameSpace = "ModbusRTUView" });
            MenuBars.Add(new MenuBar() { Icon = "CogOutline", Title = "设置", NameSpace = "SettingView" });
        }

        private void Navigate(MenuBar menuBar)
        {
            if (menuBar == null)
                return;
              
            if (string.IsNullOrEmpty(menuBar.NameSpace))
                return;
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(menuBar.NameSpace, back =>
            {
                journal = back.Context.NavigationService.Journal;
            });
        }

        public void Configure()
        {
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("WelcomeView");
        }

    }
}
