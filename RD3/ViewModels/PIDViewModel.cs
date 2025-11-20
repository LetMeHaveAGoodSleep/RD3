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
            PidInfos = new ObservableCollection<PIDInfo>(PidInfos.OrderBy(t => t.DeviceId));
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

    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value.ToString();


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
