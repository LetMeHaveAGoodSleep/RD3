using Newtonsoft.Json;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3
{
    public class AnalysisSolution
    {
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
    }
}
