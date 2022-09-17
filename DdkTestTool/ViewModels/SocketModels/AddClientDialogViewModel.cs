using CommunicationLib.Core.Network;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdkTestTool.ViewModels.SocketModels
{
    public class AddClientDialogViewModel:BindableBase, IDialogAware
    {
        public AddClientDialogViewModel()
        {

            CancelCommand = new DelegateCommand(Canel);
            SaveCommand = new DelegateCommand(Save);
        }
        public string Title => "添加客户端";

        public event Action<IDialogResult> RequestClose;

        private string ip = NetworkTools.GetLocalIp();

        public string Ip
        {
            get { return ip; }
            set { ip = value; RaisePropertyChanged(); }
        }


        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; RaisePropertyChanged(); }
        }

        private int localPort;

        public int LocalPort
        {   
            get { return localPort; }
            set { localPort = value;  RaisePropertyChanged(); }
        }
         
        private void Save()
        {
            if (!NetworkTools.IsPort(Port))
                return;
            DialogParameters param = new DialogParameters();
            param.Add("Port", Port);
            param.Add("Ip", Ip); 
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, param));
        }

        private void Canel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }


        public void OnDialogOpened(IDialogParameters parameters)
        {

        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
    }
}
