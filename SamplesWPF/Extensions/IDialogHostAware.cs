using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplesWPF.Extensions
{
    public interface IDialogHostAware
    {
        /// <summary>
        /// DialogHost 名称
        /// </summary>
        string DialogHostName { get; set; }

        /// <summary>
        /// 打来Dialog时执行
        /// </summary>
        /// <param name="parameters"></param>
        void OnDialogOpened(IDialogParameters parameters);

        /// <summary>
        /// 保存命令
        /// </summary>
        DelegateCommand SaveCommand { get; set; }
        /// <summary>
        /// 取消命令
        /// </summary>
        DelegateCommand CancelCommand { get; set; }
    }
}
