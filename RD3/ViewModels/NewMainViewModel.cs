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
using System.IO;
using System.Windows.Controls.Ribbon;
using System.Threading;
using OpenTK.Audio.OpenAL;
using Fpi.Util.WinApiUtil.CommDataType;
using System.Data;
using XZ.SQLite;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Fpi.Communication.Protocols;
using MessageBoxOptions = System.Windows.Forms.MessageBoxOptions;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RD3.ViewModels
{
    public class NewMainViewModel : BaseViewModel, IConfigureService
    {
        private Visibility _menuVisibility = Visibility.Visible;

        public Visibility MenuVisibility
        {
            get => _menuVisibility;
            set { SetProperty(ref _menuVisibility, value); }
        }

        private int _selectedIndex = 0;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        private ObservableCollection<DeviceParameter> _deviceParameterCol = [];
        public ObservableCollection<DeviceParameter> DeviceParameterCol
        {
            get { return _deviceParameterCol; }
            set { SetProperty(ref _deviceParameterCol, value); }
        }

        private string userName;

        private User _user;
        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
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

        private IRegionNavigationService navigationService;

        public DelegateCommand GoBackCommand => new(() =>
        {
            if (journal != null && journal.CanGoBack)
                journal.GoBack();
        });
        public DelegateCommand GoForwardCommand => new(() =>
        {
            if (journal != null && journal.CanGoForward)
                journal.GoForward();
        });
        public DelegateCommand LoginOutCommand => new(() => { App.LoginOut(ContainerProvider); });
        public DelegateCommand ManageUserCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(UserView), callback =>
            {
            });
        });
        public DelegateCommand ChangeLanguageCommand => new(() => 
        {
            if (LanguageName == Const.CHNLanguage)
            {
                LanguageName = Const.ENGLanguage;
            }
            else
            {
                LanguageName = Const.CHNLanguage;
            }
        });
        public DelegateCommand AboutCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(AboutView), callback =>
            {
            });
        }); 

        public void UpdateDeviece(RD3Device deviece)
        {
            //DevieceInfo[0].DescriptionValue = deviece.Name;
            //DevieceInfo[1].DescriptionValue = deviece.No;
            //DevieceInfo[2].DescriptionValue = deviece.DeviceType;
            //DevieceInfo[3].DescriptionValue = deviece.Status;
            //DevieceInfo[4].DescriptionValue = deviece.BatchID.ToString();
            //DevieceInfo[5].DescriptionValue = deviece.BatchName;
            //DevieceInfo[6].DescriptionValue = deviece.BatchStatus;
            //DevieceInfo[7].DescriptionValue = deviece.Remark;
            //DevieceInfo[8].DescriptionValue = deviece.CreateUserName;
            //DevieceInfo[9].DescriptionValue = deviece.IP;

            DevieceInfo[0].DescriptionValue = deviece.DeviceType;
            DevieceInfo[1].DescriptionValue = deviece.Status;
            DevieceInfo[2].DescriptionValue = deviece.BatchID.ToString();
            var batch = RD3SQLHelper.QueryBatchByID(deviece.BatchID);
            DevieceInfo[3].DescriptionValue = batch.Strain;
            DevieceInfo[4].DescriptionValue = batch.Tester;
            DevieceInfo[5].DescriptionValue = batch.description;
            DevieceInfo[6].DescriptionValue = deviece.IP;
        }

        public DelegateCommand RegisterCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add("RegistrationCode",RegisterManager.GetRegistrationCode());
            DialogHostService.ShowOnce(nameof(RegisterView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        public NewMainViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

            var typeFactoriesField = typeof(ViewModelLocationProvider).GetField("_typeFactories",BindingFlags.Static | BindingFlags.NonPublic);

            var registeredPages = ((Dictionary<string, Type>)typeFactoriesField.GetValue(null))
                .Keys.Select(t => t.Substring(t.LastIndexOf('.') + 1))
                .ToList();

            System.Timers.Timer experimentTimer = new System.Timers.Timer
            {
                Interval = 1000
            };
            experimentTimer.Elapsed += ExperimentTimer_Elapsed;
            //elapsedTime = TimeSpan.Zero;
            experimentTimer.AutoReset = true;
            experimentTimer.Start();

            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();

            //string[] rowHeaders = new string[] { "ID", "序号", "类型", "状态", "最近批次ID", "最近批次号", "进度", "备注", "创建", "IP地址" };
            //ObservableCollection<Description> devieceInfo = new ObservableCollection<Description>();
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[0], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[1], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[2], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[3], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[4], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[5], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[6], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[7], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[8], DescriptionValue = "deviece.Name" });
            //devieceInfo.Add(new Description() { DescriptionName = rowHeaders[9], DescriptionValue = "deviece.Name" });

            string[] rowHeaders = new string[] { "类型", "状态", "批次ID", "菌种信息", "测试人员", "备注", "IP地址" };

            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[0], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[1], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[2], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[3], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[4], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[5], DescriptionValue = "deviece.Name" });
            DevieceInfo.Add(new Description() { DescriptionName = rowHeaders[6], DescriptionValue = "deviece.Name" });

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += (s, e) =>
            {
                while (ReactorCol == null)
                {
                    Thread.Sleep(5000);
                    continue;
                   
                }

                Dictionary<string, bool> dictionary = new Dictionary<string, bool>();

                Dictionary<string, int> dicCount = new Dictionary<string, int>();
                foreach (var item in ReactorCol)
                {
                    dictionary.Add(item.Name, false);

                    dicCount.Add(item.Name, 0);
                }

                while (true)
                {
                    try
                    {
                        foreach (var item in ReactorCol)
                        {
                            Pipe pipe = PortManager.GetInstance().FindSendPipe(item.Name);
                            if (pipe == null)
                            {
                                item.ConnectStatus = "断开";
                                continue;
                            }
                            item.IP = (pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.Substring(0, Convert.ToInt32((pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.IndexOf(":")));
                            if (InstrumentSolution.GetInstance().IsSimulation)
                            {
                                item.ConnectStatus = "模拟";
                                item.ReactorStatus = ReactorStatus.Simulated;
                            }
                            else
                            {
                                item.ConnectStatus = pipe.Connected == true ? "连接" : "断开";
                                if (pipe.Connected || item.ReactorStatus == ReactorStatus.Simulated)
                                {
                                    if (!dictionary[item.Name])
                                    {
                                        Task.Run(() =>
                                        {
                                            try
                                            {
                                                //保持所有功能控制模式为开 
                                                //foreach (var item1 in EnumUtil.GetEnumValues<ControlObject>())
                                                //{
                                                //    InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(item.Name, item1, SwitchMode.Open);
                                                //    Thread.Sleep(300);
                                                //}
                                                dictionary[item.Name] = true;
                                            }
                                            catch (Exception ex)
                                            {
                                                LogHelper.Debug(string.Format("{0}下发模块打开命令失败", item.Name) + ex.Message + ex.StackTrace);
                                            }

                                        });
                                    }

                                    if (dicCount[item.Name] % 60 == 0)
                                    {
                                        InstrumentSolution.GetInstance().CommandWrapper.SetTimeSync(item.Name, DateTime.Now,AppSession.RunningTimeSpan);
                                    }
                                    dicCount[item.Name] += 1;
                                }
                                else
                                {
                                    dictionary[item.Name] = false;
                                    dicCount[item.Name] = 0;
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
                    Thread.Sleep(1000);
                }
            };
            backgroundWorker.RunWorkerAsync();

            var back =new BackgroundWorker();
            back.DoWork += (s, e) => 
            {
                int count = 1;
                List<object[]> list = [];
                Dictionary<string, DateTime> dictionary = new Dictionary<string, DateTime>();
                Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
                while (true)
                {
                    try
                    {
                        if (ReactorCol == null)
                        {
                            continue;
                        }
                        //string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();
                        foreach (var item in ReactorCol)
                        {
                            RD3Device device = item as RD3Device;
                            int index = ClockSupervisor.realDatasDic[device.Name].Count - 1;
                            var realTimeParam = ClockSupervisor.realDatasDic[device.Name][index];
                            //方成 开机以后保存罐体初始重量
                            if (device.Status == "Running")
                            {
                                if (!AppSession.DicTankWeight.ContainsKey(device.Name))
                                {
                                    AppSession.DicTankWeight[device.Name] = Tuple.Create(realTimeParam.JarWeight, realTimeParam.Pump1FlowCapacity, realTimeParam.Pump2FlowCapacity, realTimeParam.Pump3FlowCapacity, realTimeParam.Pump4FlowCapacity, realTimeParam.Pump5FlowCapacity, realTimeParam.Pump6FlowCapacity);
                                }
                            }

                            if (!dictionary.ContainsKey(device.Name))
                            {
                                var realTimeParams = ClockSupervisor.realDatasDic[device.Name];
                                for (int i = 0; i < realTimeParams.Count; i++)
                                {
                                    var realTime = realTimeParams[i];
                                    dictionary[device.Name] = realTime.SampleTime;
                                    if ((string.IsNullOrWhiteSpace(realTime.ReactorName) || InstrumentSolution.GetInstance().IsSimulation) && device.BatchID < 1)
                                    {
                                        continue;
                                    }
                                    if (!keyValuePairs.ContainsKey(device.Name))
                                    {
                                        keyValuePairs[device.Name] = 1;
                                    }
                                    else
                                    {
                                        keyValuePairs[device.Name] += 1;
                                    }

                                    List<object> values = new List<object>();
                                    values.Add(device.Name);
                                    values.Add(device.BatchID.ToString());
                                    values.Add(realTime.SampleTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    foreach (var p in propertyInfos)
                                    {
                                        object o = p.GetValue(realTime);
                                        values.Add(o);
                                    }
                                    list.Add(values.ToArray());
                                }
                            }
                            else
                            {
                                var realTimeParams = ClockSupervisor.realDatasDic[device.Name].FindAll(t => t.SampleTime > dictionary[device.Name]);
                                for (int i = 0; i < realTimeParams.Count; i++)
                                {
                                    var realTime = realTimeParams[i];
                                    dictionary[device.Name] = realTime.SampleTime;
                                    if ((string.IsNullOrWhiteSpace(realTime.ReactorName) || InstrumentSolution.GetInstance().IsSimulation) && device.BatchID < 1)
                                    {
                                        continue;
                                    }
                                    if (!keyValuePairs.ContainsKey(device.Name))
                                    {
                                        keyValuePairs[device.Name] = 1;
                                    }
                                    else
                                    {
                                        keyValuePairs[device.Name] += 1;
                                    }
                                    List<object> values = new List<object>();
                                    values.Add(device.Name);
                                    values.Add(device.BatchID.ToString());
                                    values.Add(realTime.SampleTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    foreach (var p in propertyInfos)
                                    {
                                        object o = p.GetValue(realTime);
                                        values.Add(o);
                                    }
                                    list.Add(values.ToArray());
                                }
                            }
                        }

                        if (keyValuePairs.Keys.Count < 1) continue;
                        var maxValue = keyValuePairs.OrderByDescending(x => x.Value).First().Value;
                        if (maxValue > 0 && maxValue >= 60)
                        {
                            RD3SQLHelper.BulkInsertRealTimeParam(list);

                            for (int i = 0; i < list.Count; i++)
                            {
                                list[i] = null;
                            }

                            list.Clear();
                            list = null;

                            keyValuePairs.Clear();
                            keyValuePairs = null;

                            list = [];
                            keyValuePairs = new Dictionary<string, int>();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("保存实时信息出错" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    finally
                    {
                        count += 1;
                        Thread.Sleep(1000);
                    }
                    if (DateTime.Now.Second % 15 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            };
            back.RunWorkerAsync();
        }

        private void ExperimentTimer_Elapsed(object sender, EventArgs e)
        {
            AppSession.RunningTimeSpan += TimeSpan.FromSeconds(1);
        }
        private ObservableCollection<Description> _devieceInfo = [];
        public ObservableCollection<Description> DevieceInfo
        {
            get { return _devieceInfo; }
            set
            {
                SetProperty(ref _devieceInfo, value);
            }
        }

        private ObservableCollection<RD3Device> _reactorCol = null;// new ObservableCollection<Device>();//DeviceManager.GetInstance().Devices;
        public ObservableCollection<RD3Device> ReactorCol
        {
            //get => _reactorCol;
            get
            {
                return _reactorCol;
            }
            set { _reactorCol = value; RaisePropertyChanged(); }
        }


        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            ReactorCol = GetReactorCol();
            LanguageName = Const.CHNLanguage;
            User = AppSession.CurrentUser;
            aggregator.SendMessage("", nameof(NewMainViewModel), ReactorCol);

            ObservableCollection<DeviceParameter> temp = [];
            for (int i = 0; i < ReactorCol.Count; i++)
            {
                Device reactor = ReactorCol[i];
                DeviceParameter device = new()
                {
                    Name = reactor.Name,
                    SerialNumber = i + 1,
                };
                //加载 消泡参数
                if (File.Exists(FileConst.DefoamingPath))
                {
                    string result = File.ReadAllText(FileConst.DefoamingPath);
                    device.DefoamingSetting = JsonConvert.DeserializeObject<DefoamingSetting>(result);
                }
                else
                {
                    device.DefoamingSetting = new DefoamingSetting()
                    {
                        AutoDefoaming = false,
                        DefoamingThreshold = 0.7f,
                        FlowSpeed = 1,
                        FlowCapacity = 1,
                    };
                }
                temp.Add(device);
            }

            DeviceParameterCol = new ObservableCollection<DeviceParameter>(temp);
            CurrentDeviceParameter = DeviceParameterCol.Count > 0 ? DeviceParameterCol[0] : null;

            //    var navigationParameters = new NavigationParameters();
            //    regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(
            //new Uri(nameof(IndexView) + navigationParameters.ToString(), UriKind.Relative), navigationCallback =>
            //{
            //    if ((bool)navigationCallback.Result)
            //    {
            //        journal = navigationCallback.Context.NavigationService.Journal;
            //    }
            //});
            //    RegisterManager.MonitorRegister();


            RegUtils.StartChecked();
        }

        private ObservableCollection<RD3Device> GetReactorCol()
        {
            ObservableCollection<RD3Device>  reactorCol = new ObservableCollection<RD3Device>();
            if (AppSession.CurrentUser != null)
            {
                foreach (Device item in DeviceManager.GetInstance().Devices)
                {
                    if (!AppSession.CurrentUser.UserDevieceIDs.Contains(item.Name))
                    {
                        continue;
                    }

                    RD3Device rD3Device = new RD3Device()
                    {
                        Name = item.Name,
                        IP = "127.0.0.1",
                        CreateUserID = AppSession.CurrentUser.UserName,
                        DeviceType = "Unit",
                        Status = "Avaliable"
                    };
                    //方成 IP从Pipe中获取
                    Pipe pipe = PortManager.GetInstance().FindSendPipe(item.Name);//
                    if (pipe != null)
                    {
                        rD3Device.IP = (pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.Substring(0, Convert.ToInt32((pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.IndexOf(":")));
                    }
                    rD3Device.No = rD3Device.Name.Replace("G", "");
                    reactorCol.Add(rD3Device);
                }
            }
            return reactorCol;
        }

        public DelegateCommand ReactorCommand => new(() =>
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Content == null) continue;
                string formName = item.Content?.ToString().Substring(item.Content.ToString().LastIndexOf(".") + 1);
                if (formName == nameof(ReactorView))
                {
                    item.Activate();
                    item.WindowState = WindowState.Normal;
                    return;
                }
            }

            CurrentDeviceParameter = DeviceParameterCol[SelectedIndex];
            if (AppSession.RunningDevices.Count < 1 || AppSession.RunningDevices.FindIndex(t => t.Name == CurrentDeviceParameter.Name) < 0)
            {
                MessageBox.Show(string.Format("反应器{0}未开始批次实验，实验数据不会自动保存！", CurrentDeviceParameter.Name), "温馨提示",System.Windows.Forms.MessageBoxButtons.OK);
            }
            DialogParameters keyValuePairs = new DialogParameters() { { nameof(DeviceParameter), CurrentDeviceParameter }, { nameof(ReactorCol), ReactorCol } };
            DialogHostService.ShowOnce(nameof(ReactorView), keyValuePairs, callback =>
            {

            });
        });

        public DelegateCommand CameraCommand => new(() =>
        {
            CurrentDeviceParameter = DeviceParameterCol[SelectedIndex];
            DialogParameters keyValuePairs = new DialogParameters()
            {
                { nameof(DeviceParameter),CurrentDeviceParameter }
            };
            DialogHostService.ShowOnce(nameof(CameraImageView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });
    }

    /// <summary>
    /// 字段描述
    /// </summary>
    public class Description : BindableBase
    {
        private string _descriptionValue;
        public string DescriptionValue
        {
            get => _descriptionValue;
            set { SetProperty(ref _descriptionValue, value); }
        }
        private string _descriptionName;
        public string DescriptionName
        {
            get => _descriptionName;
            set { SetProperty(ref _descriptionName, value); }
        }
    }
}
