using CommunicationLib.Core.Network;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs; 
using SamplesWPF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.ViewModels.Dialogs
{
    public class AddTcpServerViewModel :BindableBase, IDialogHostAware
    { 
        public AddTcpServerViewModel()
        {
            CancelCommand = new DelegateCommand(Canel);
            SaveCommand = new DelegateCommand(Save); 
        }

        private int port;

        public int Port
        {
            get { return port; }
            set { port = value;  RaisePropertyChanged(); }
        }


        private void Save()
        {
            if (!NetworkTools.IsPort(Port))
                return;
            if (DialogHost.IsDialogOpen(DialogHostName))
            {
                DialogParameters param = new DialogParameters();
                param.Add("Port", Port);
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.OK, param));
            }
        }

        private void Canel()
        {
            if (DialogHost.IsDialogOpen(DialogHostName))
                DialogHost.Close(DialogHostName, new DialogResult(ButtonResult.Cancel));
        }

        public DelegateCommand SaveCommand {get;set;}
        public DelegateCommand CancelCommand {get;set;}
        public string DialogHostName { get; set; }= "添加服务器";
        public IEventAggregator Aggregator { get; }

        public void OnDialogOpened(IDialogParameters parameters)
        {
             
        }
    }
}
