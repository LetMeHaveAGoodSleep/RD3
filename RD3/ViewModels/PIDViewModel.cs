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
using Fpi.Util.WinApiUtil.CommDataType;
using ScottPlot;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace RD3.ViewModels
{
    public class PIDViewModel : BaseViewModel, IDialogAware
    {
        PIDInfo _selectedPIDInfo;
        public PIDInfo SelectedPIDInfo
        {
            get { return _selectedPIDInfo; }
            set { SetProperty(ref _selectedPIDInfo, value); }
        }

        private ObservableCollection<PIDInfo> _pidInfos = [];
        public ObservableCollection<PIDInfo> PidInfos { get { return _pidInfos; } set { SetProperty(ref _pidInfos, value); } }
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

        public DelegateCommand AddCommand => new(() =>
        {
            PidInfos.Add(new PIDInfo());
        });

        public DelegateCommand DeleteCommand => new(() =>
        {
            if (SelectedPIDInfo != null)
                PidInfos.Remove(SelectedPIDInfo);
        });

        /// <summary>
        /// 保存pid参数
        /// </summary>
        public DelegateCommand SaveCommand => new(() => 
        {
            //string result = CustomApp.JsonHelper.ObjectToString(PidInfos.ToList());
            //File.WriteAllText(FileConst.PidInfoPath,result);
            PidInfos = new ObservableCollection<PIDInfo>(PidInfos.OrderBy(t => t.deviceID));
            string json = JsonConvert.SerializeObject(PidInfos);
            File.Delete(FileConst.PidInfoPath);
            File.WriteAllText(FileConst.PidInfoPath, json);

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public PIDViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();
            List<PIDInfo> pIDInfos = new List<PIDInfo>();
            if (File.Exists(FileConst.PidInfoPath))
            {
                string result = File.ReadAllText(FileConst.PidInfoPath);
                pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                var array = EnumUtil.GetEnumDescriptions<PIDFactor>();
                foreach (PIDInfo pidInfo in pIDInfos)
                {
                    var factor = EnumUtil.GetEnumByDescription<PIDFactor>(pidInfo.PidName);
                    pidInfo.Factor = factor;
                }
            }
            else
            {

            }
            PidInfos = new ObservableCollection<PIDInfo>(pIDInfos);
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

    public class PIDInfo : BindableBase
    {
        public PIDInfo()
        {
            createTime = DateTime.Now;
            maxSpeed = 100;
            Interval = 1;
        }

        public DateTime createTime { get; set; }
        public string createUser { get; set; }
        public string deviceID { get; set; }
        public float startTime { get; set; }
        public float endTime { get; set; }
        public float P { get; set; }
        public float I { get; set; }
        public float D { get; set; }
        public int Interval { get; set; }

        public float Threshold { get; set; }

        public float deadArea { get; set; }

        public float maxSpeed { get; set; }

        private string _excuteType = "自动";
        public string excuteType {
            get => _excuteType;
            set
            {
                _excuteType = value;
                SetProperty(ref _excuteType, value); 
            }
        }

        private bool _used = false;
        public bool Used
        {
            get => _used;
            set
            {
                SetProperty(ref _used, value);
            }
        }


        private string _pidName = "";
        public string PidName
        {
            get => _pidName;
            set
            {
                SetProperty(ref _pidName, value);
            }
        }

        private PIDFactor _fator = PIDFactor.Unknown;
        public PIDFactor Factor
        {
            get => _fator;
            set
            {
                PidName = EnumUtil.GetEnumDescription(value);
                SetProperty(ref _fator, value);
            }
        }

        public void SetExcuteType(string ex)
        {
            excuteType = ex;
        }

        public string connectedWith { get; set; }//关联项

        public override bool Equals(object obj)
        {
            // 检查null和类型是否匹配
            if (obj == null || GetType() != obj.GetType())
                return false;

            PIDInfo other = (PIDInfo)obj;
            return P == other.P && I == other.I && D == other.D && deviceID == other.deviceID && Interval == other.Interval && Threshold == other.Threshold
                && deadArea == other.deadArea && Factor == other.Factor;
        }

        // 必须同时重写GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(P, I, D, deviceID, Interval, Threshold, deadArea, Factor);
        }

        /// <summary>
        /// 克隆参数
        /// </summary>
        /// <param name="source"></param>
        public void CloneArgs(PIDInfo soure)
        {
            this.Used = soure.Used;
            this.deadArea = soure.deadArea;
            this.D = soure.D;
            this.I = soure.I;
            this.P = soure.P;
            this.Interval = soure.Interval;
            this.Threshold = soure.Threshold;
            this.maxSpeed = soure.maxSpeed;
        }
    }

    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value.ToString();


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
