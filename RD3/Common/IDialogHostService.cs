using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Common
{
    public interface IDialogHostService : IDialogService
    {
        Task<IDialogResult> ShowDialog(string name, IDialogParameters parameters, string dialogHostName = "Root");


        /// <summary>
        /// 每个窗口只存在一个实例
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="callback"></param>
        /// <param name="windowName"></param>
        void ShowOnce(string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName = "");

              /// <summary>
              /// 每个窗口只存在一个实例
              /// </summary>
              /// <param name="name"></param>
              /// <param name="parameters"></param>
              /// <param name="callback"></param>
              /// <param name="windowName"></param>
        void ShowOnce(string name, Action<IDialogResult> callback, string windowName = "");
    }
}
