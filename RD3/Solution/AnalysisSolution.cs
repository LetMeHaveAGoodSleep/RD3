using ImTools;
using Newtonsoft.Json;
using RD3.Common;
using RD3.Controller;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3
{
    public class AnalysisSolution:IDisposable
    {
        public AgitController AgitController { get; private set; }

        public DOController DOController { get; private set; }

        public TempController TempController { get; private set; }

        public pHController pHController { get; private set; }
        

        public ObservableCollection<DeviceParameter> ReactorCol
        {
            get;
            private set;
        } = new ObservableCollection<DeviceParameter>();

        public ObservableCollection<PumpInfo> PumpInfoCol
        {
            get;
            private set;
        } = new ObservableCollection<PumpInfo>();

        public ObservableCollection<MFCInfo> MFCInfoCol
        {
            get;
            private set;
        } = new ObservableCollection<MFCInfo>();

        public EventPublisher EventPublisher { get; private set; } = new EventPublisher();

        private static volatile AnalysisSolution _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private static readonly object _lock1 = new object(); // 锁对象
        private AnalysisSolution()
        {
            LoadSetting();

            AgitController = new();
            DOController = new();
            TempController = new();
            pHController = new();
        }

        public static AnalysisSolution GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new AnalysisSolution(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadSetting()
        {
            LoadReactor();
            LoadPumpSetting();
            LoadMFCSetting();
        }

        private void LoadReactor()
        {
            string dir = string.Format(FileConst.ReactorParamDir, AppSession.CurrentUser?.UserName?.ToString());
            if (!Directory.Exists(dir))
            {
                return;
            }
            string filePath = string.Format(FileConst.ReactorParamPath, AppSession.CurrentUser?.UserName?.ToString());
            if (!File.Exists(filePath))
            {
                return;
            }
            string jsonContent = AESEncryption.DecryptFile(filePath);
            ReactorCol = JsonConvert.DeserializeObject<ObservableCollection<DeviceParameter>>(jsonContent);
            if (ReactorCol != null && ReactorCol.Count > 1)
            {
                ReactorCol = new ObservableCollection<DeviceParameter>(ReactorCol.OrderBy(t => t.SerialNumber));
            }
        }

        private void LoadPumpSetting()
        {
            string filePath = FileConst.PumpInfoPath;
            if (!File.Exists(filePath))
            {
                return;
            }
            string jsonContent = AESEncryption.DecryptFile(filePath);
            PumpInfoCol = JsonConvert.DeserializeObject<ObservableCollection<PumpInfo>>(jsonContent);
            if (PumpInfoCol != null && PumpInfoCol.Count > 1)
            {
                PumpInfoCol = new ObservableCollection<PumpInfo>(PumpInfoCol.OrderBy(t => t.PumpIndex));
            }
        }

        private void LoadMFCSetting()
        {
            string filePath = FileConst.MFCInfoPath;
            if (!File.Exists(filePath))
            {
                return;
            }
            string jsonContent = AESEncryption.DecryptFile(filePath);
            MFCInfoCol = JsonConvert.DeserializeObject<ObservableCollection<MFCInfo>>(jsonContent);
            if (MFCInfoCol != null && MFCInfoCol.Count > 1)
            {
                MFCInfoCol = new ObservableCollection<MFCInfo>(MFCInfoCol.OrderBy(t => t.MFCIndex));
            }
        }

        public void SaveAllSetting()
        {
            SaveReactorSetting();
            SavePumpSetting();
            SaveMFCSetting();
        }

        public void SaveReactorSetting(ObservableCollection<DeviceParameter> dataList = null)
        {
            lock (_lock1)
            {
                if (dataList != null && !dataList.Equals(ReactorCol))
                {
                    ReactorCol = dataList;
                }
                ReactorCol = [.. ReactorCol.DistinctBy(t => t.Name)];
                string json = JsonConvert.SerializeObject(ReactorCol);
                string filePath = string.Format(FileConst.ReactorParamPath, AppSession.CurrentUser?.UserName?.ToString());
                string dir = string.Format(FileConst.ReactorParamDir, AppSession.CurrentUser?.UserName?.ToString());
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Delete(filePath);
                File.WriteAllText(filePath, json);
            }
        }

        public void SavePumpSetting(ObservableCollection<PumpInfo> dataList = null)
        {
            if (dataList != null && !dataList.Equals(PumpInfoCol))
            {
                PumpInfoCol = dataList;
            }
            string json = JsonConvert.SerializeObject(PumpInfoCol);
            File.Delete(FileConst.PumpInfoPath);
            File.WriteAllText(FileConst.PumpInfoPath, json);
        }

        public void SaveMFCSetting(ObservableCollection<MFCInfo> dataList = null)
        {
            if (dataList != null && !dataList.Equals(MFCInfoCol))
            {
                MFCInfoCol = dataList;
            }
            string json = JsonConvert.SerializeObject(MFCInfoCol);
            File.Delete(FileConst.MFCInfoPath);
            File.WriteAllText(FileConst.MFCInfoPath, json);
        }

        public void SavePumpMFCToOld(string reactorName)
        {
            var dictionary = PumpMFCConfig.GetValue(reactorName);
            if (dictionary == null || dictionary.Keys.Count < 1)
            {
                dictionary = new Dictionary<string, string>();
            }

            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 1; i < 7; i++)
            {
                PumpInfo pumpInfo = PumpInfoCol.FindFirst(t => t.PumpIndex == i);
                if (pumpInfo == null) continue;
                dictionary[$"Pump{i}"] = pumpInfo.Pump.ToString();
            }
            for (int i = 1; i < 4; i++)
            {
                MFCInfo mfcInfo = MFCInfoCol.FindFirst(t => t.MFCIndex == i);
                if (mfcInfo == null) continue;
                dictionary[$"MFC{i}"] = mfcInfo.Gas.ToString();
            }
            PumpMFCConfig.SetValue(reactorName, dictionary);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
