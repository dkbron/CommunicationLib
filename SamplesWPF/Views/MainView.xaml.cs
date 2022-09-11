using Prism.Events;
using Prism.Ioc;
using SamplesWPF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SamplesWPF.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IContainerProvider container;
        IDialogHostService dialogService;
        public MainView(IEventAggregator aggregator, IContainerProvider container)
        {
            InitializeComponent();


            aggregator.Register(args =>
            {
                DialogHost.IsOpen = args.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

            aggregator.RegisterMessage(args =>
            {
                if (snackbar.MessageQueue != null)
                    snackbar.MessageQueue.Enqueue(args.Message);
            });

            InitColorZoneNavigateBtn();

            ListBoxMenuBar.SelectionChanged += (s, e) =>
            {
                drawerHost.IsLeftDrawerOpen = false;
            };

            this.container = container;
            dialogService = container.Resolve<IDialogHostService>();
        }


        private void InitColorZoneNavigateBtn()
        {

            btnMax.Click += (sender, e) =>
            {
                if (this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                    btnMax.Content = "❐";
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    btnMax.Content = "☐";
                }
            };

            btnMin.Click += (sender, e) =>
            {
                this.WindowState = WindowState.Minimized;
            };

            btnClose.Click += async (sender, e) =>
            {
                var result = await dialogService.Question("温馨提示", "确认关闭程序?");
                if (result.Result == Prism.Services.Dialogs.ButtonResult.No)
                    return;
                Close();
            };

            colorZoneNavigate.MouseDown += (sender, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };

            colorZoneNavigate.MouseDoubleClick += (sender, e) =>
            {
                if (this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                    btnMax.Content = "❐";
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                    btnMax.Content = "☐";
                }
            };
        }
    }
}
