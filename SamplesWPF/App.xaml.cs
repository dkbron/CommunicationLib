using Prism.DryIoc;
using Prism.Ioc;
using SamplesWPF.Extensions;
using SamplesWPF.ViewModels;
using SamplesWPF.ViewModels.Dialogs;
using SamplesWPF.Views;
using SamplesWPF.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SamplesWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        { 
            return Container.Resolve<MainView>();
        }

        protected override void OnInitialized()
        {
            var configureService = Current.MainWindow.DataContext as IConfigureService;
            if (configureService != null)
                configureService.Configure(); 
            base.OnInitialized();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<WelcomeView, WelComeViewModel>();
            containerRegistry.RegisterForNavigation<AddTcpServerView, AddTcpServerViewModel>(); 
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>(); 
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<SettingView, SettingViewModel>();
            containerRegistry.RegisterForNavigation<SkinView, SkinViewModel>();
            containerRegistry.RegisterForNavigation<TcpView, TcpViewModel>();
            containerRegistry.RegisterForNavigation<TcpServerView, TcpServerViewModel>(); 
        }
    }
}
