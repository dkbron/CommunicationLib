using DdkTestTool.Extensions;
using DdkTestTool.ViewModels;
using DdkTestTool.ViewModels.SocketModels;
using DdkTestTool.Views;
using DdkTestTool.Views.SocketViews;
using DdkTestTool.Views.SocketViews.Dialogs;
using Prism.DryIoc;
using Prism.Ioc; 
using System.Windows;

namespace DdkTestTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnInitialized()
        {
            var config = Current.MainWindow.DataContext as IConfigureService;
            if (config != null)
                config.Configure();
            base.OnInitialized();
        }

        protected override Window CreateShell()
        { 
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        { 
            containerRegistry.RegisterDialog<AddServerDialog, AddServerDialogViewModel>();
            containerRegistry.RegisterDialog<AddClientDialog, AddClientDialogViewModel>();
            containerRegistry.RegisterForNavigation<MainWindow, WindowViewModel>();
            containerRegistry.RegisterForNavigation<WelcomeView, WelcomeViewModel>();
            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
            containerRegistry.RegisterForNavigation<SocketIndexView, SocketIndexViewModel>();
            containerRegistry.RegisterForNavigation<SocketView, SocketViewModel>();
        }
    }
}
