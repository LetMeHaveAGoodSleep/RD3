using RD3.Common;
using RD3.Common.Models;
using RD3.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RD3.Views;
using DryIoc;
using System.Windows.Controls;
using HandyControl.Data;
using HandyControl.Controls;
using ImTools;
using Prism.Services.Dialogs;
using System.Windows.Threading;
using Prism.Events;
using RD3.Shared;
using HandyControl.Tools;
using System.Resources;
using static MaterialDesignThemes.Wpf.Theme;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Fpi.Communication.Manager;
using static RD3.Views.NewMainView;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using OpenTK.Compute.OpenCL;
using System.Data;
using System.IO;
using XZ.SQLite;
using System.Globalization;
using System.Windows.Data;

namespace RD3.ViewModels
{
    public class ParameterViewModel : BaseViewModel, IDialogAware
    {
        private ObservableCollection<Parameter> _parameters = [];
        public ObservableCollection<Parameter> Parameters 
        { 
            get { return _parameters; } 
            set { SetProperty(ref _parameters, value); } 
        }
        private string _languageName;
        public string LanguageName
        {
            get { return _languageName; }
            set
            {
                
                if (value != _languageName)
                {
                    if (value == Const.CHNLanguage)
                    {
                        Language.LoadResourceKey("zh_CN");
                    }
                    else
                    {
                        Language.LoadResourceKey("en_US");
                    }
                }
                SetProperty(ref _languageName, value);
                AppSession.LanguageName = value;
            }
        }
        public string Title => "";
        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// 保存pid参数
        /// </summary>
        public DelegateCommand SaveCommand => new(() => 
        {
            string result = JsonConvert.SerializeObject(Parameters.ToList());
            File.WriteAllText(FileConst.ParameterPath, result);
        });

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public ParameterViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();
            List<Parameter> parameters = new List<Parameter>();
            if (File.Exists(FileConst.ParameterPath))
            {
                string result = File.ReadAllText(FileConst.ParameterPath);
                parameters = CustomApp.JsonHelper.StringToObject<List<Parameter>>(result);
            }
            else
            {
                DateTime dateTime = DateTime.Now;
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "SP", unit = "℃", minValue = 0, maxValue = 100,  parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime ,createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Temp_SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "DO_SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "PH_SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed1_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed1_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed1_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed2_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed2_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Feed2_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Wigth1", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Wigth2", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G01", parameterName = "Wigth3", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });

                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Temp_SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "DO_SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "PH_SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed1_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed1_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed1_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed2_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed2_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Feed2_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Wigth1", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Wigth2", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G02", parameterName = "Wigth3", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });


                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Temp_SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "DO_SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "PH_SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed1_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed1_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed1_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed2_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed2_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Feed2_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Wigth1", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Wigth2", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G03", parameterName = "Wigth3", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });

                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Temp_SP", unit = "℃", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "DO_SP", unit = "%", minValue = 0, maxValue = 20, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "PH_SP", unit = "", minValue = 0, maxValue = 14, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed1_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed1_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed1_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed2_PV", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed2_SP", unit = "rpm", minValue = 0, maxValue = 1000, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Feed2_FLOW", unit = "ml/min", minValue = 0, maxValue = 100, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Wigth1", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Wigth2", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
                parameters.Add(new Parameter() { deviceID = "G04", parameterName = "Wigth3", unit = "Kg", minValue = 0, maxValue = 10, parameterAttribute = (int)ParameterAttribute.PV, parameterType = (int)ParameterType.REAL, createTime = dateTime, createUser = AppSession.CurrentUser.UserName });
            }
            Parameters = new ObservableCollection<Parameter>(parameters);
        }

        public void Configure()
        {
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }

    /// <summary>
    /// 接种信息
    /// </summary>
    public class Parameter : BindableBase
    {
        public DateTime createTime { get; set; }
        public string createUser { get; set; }
        public string deviceID { get; set; }
        public string parameterName { get; set; }
        public float maxValue { get; set; }
        public float minValue { get; set; }
        public string unit {  get; set; }
        public int parameterType { get; set; }//类型

        public string parameterTypeStr 
        {
            get
            {
                switch (parameterType)
                {
                    case 0:
                        return "REAL";
                        break;
                    case 1:
                        return "BOOL";
                        break;

                }

                return parameterType.ToString();
            }
            set
            {
                if(value == "REAL")
                {
                    parameterType = 0;
                }
                if (value == "BOOL")
                {
                    parameterType = 1;
                }
            }
        }//类型
        public int parameterAttribute { get; set; }//属性

        public string parameterAttributeStr
        {
            get
            {
                switch (parameterAttribute)
                {
                    case 0:
                        return "PV";
                        break;
                    case 1:
                        return "SV";
                        break;
                    case 2:
                        return "Enable";

                }

                return "PV";
            }
            set
            {
                if (value == "PV")
                {
                    parameterAttribute = 0;
                }
                if (value == "SV")
                {
                    parameterAttribute = 1;
                }
                if (value == "Enable")
                {
                    parameterAttribute = 2;
                }
            }
        }
    }

    public enum ParameterType
    {
        REAL,
        BOOL
    }
    /// <summary>
    /// 属性
    /// </summary>
    public enum ParameterAttribute
    {
        PV,
        SV,
        Enable
    }
}
