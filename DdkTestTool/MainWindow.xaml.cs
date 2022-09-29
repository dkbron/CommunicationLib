using DdkTestTool.ViewModels;
using Prism.Services.Dialogs;
using System.Windows; 

namespace DdkTestTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDialogService dialogService;

        public MainWindow(IDialogService dialogService)
        {
            InitializeComponent(); 
            InitWindowChorme();
            this.dialogService = dialogService;
        }

        private void InitWindowChorme()
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

            btnClose.Click += (sender, e) =>
            {
                //var result = await dialogService.Question("温馨提示", "确认关闭程序?");
                //if (result.Result == Prism.Services.Dialogs.ButtonResult.No)
                //    return;
                this.Close();
            }; 
        } 
    }
}
