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
using MathNet.Symbolics;

namespace RD3.ViewModels
{
    public class NewFeedViewModel : BaseViewModel, IDialogAware
    {
        private ObservableCollection<FeedInfo> _feedInfos = [];
        public ObservableCollection<FeedInfo> FeedInfos { get { return _feedInfos; } set { SetProperty(ref _feedInfos, value); } }
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
        public DelegateCommand<PIDInfo> ExecutePIDCommand => new((pidInfo) =>
        {

        });

        public DelegateCommand<PIDInfo> StopPIDCommand => new((pidInfo) =>
        {

        });
        /// <summary>
        /// 保存pid参数
        /// </summary>
        public DelegateCommand SaveCommand => new(() => 
        {
            string result = JsonConvert.SerializeObject(FeedInfos.ToList());
            File.WriteAllText(FileConst.PidInfoPath,result);
        });

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public NewFeedViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();
            List<FeedInfo> pIDInfos = new List<FeedInfo>();
            if (File.Exists(FileConst.FeedInfoPath))
            {
                string result = File.ReadAllText(FileConst.FeedInfoPath);
                pIDInfos = CustomApp.JsonHelper.StringToObject<List<FeedInfo>>(result);
            }
            else
            {
                DateTime dateTime = DateTime.Now;
                pIDInfos.Add(new FeedInfo() { ID = "1", Name = "Polynomial Prototype", Equation = "y=a*(t-Δt)^2+b*(t-Δt)+c", Variable = "a, b, c, At", Description = "y: Flow SP, mL/h;\r\na: 2nd order Acceleration, ml/h^3,\r\nb: 1st order Acceleration, ml/h^2,\r\nc: initial flow rate, ml/h;\r\nt: PLC Runtime, h;\r\nAt: Time offset, when t<At, y=c;", SetValueID = "", ProcessValueID = "", FeedInfoType = "Polynomial Prototype" });
                pIDInfos.Add(new FeedInfo() { ID = "2", Name = "Exponential Prototype", Equation = "F1(t)=F1(0)*exp(μ*(t-△t))", Variable = "F1(0), μ At", Description = "where F1(0)=μ*V(0)X(0)/(*S) is pre-determined.\r\nu: specific growth rate, 1/h;\r\nt: PLC Runtime, h;\r\nV(0): initial volume, L;\r\nX(0): initial cell density, g/L;\r\nY: Yield coefficient, g/g;\r\nS: Substrate concentration, g/L;\r\nAt: Time offset, when t<At, output=F1(0)", SetValueID = "", ProcessValueID = "", FeedInfoType = "Exponential Prototype" });
                pIDInfos.Add(new FeedInfo() { ID = "3", Name = "DO Feedback Prototype", Equation = "Bang-Bang based on Do", Variable = "Xp, Xn, Bp, Bn", Description = "1: if DO PV <= Xn: Feed Flow_Sp = Bn;\r\n2: if SP >= Xp: Feed_Flow_sp = Bp,", SetValueID = "", ProcessValueID = "", FeedInfoType = "DO Feedback Prototype" });
                pIDInfos.Add(new FeedInfo() { ID = "4", Name = "pH Feedback Prototype", Equation = "Bang-Bang based on pH", Variable = "Xp, Xn, Bp, Bn", Description = "1: if pH_PV <= Xn: Feed Flow_Sp = Bn;\r\n2:if pH_PV >=Xp: Feed_Flow_SP = Bp:", SetValueID = "", ProcessValueID = "", FeedInfoType = "pH Feedback Prototype" });
            }
            FeedInfos = new ObservableCollection<FeedInfo>(pIDInfos);
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

    public class FeedInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Equation{ get; set; }

        public string Variable { get; set; }

        public string Description { get; set; }

        public string SetValueID { get; set; }
        public string ProcessValueID { get; set; }

        public string FeedInfoType { get; set; }
    }
}
