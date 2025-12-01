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
using System.Xml.Linq;

namespace RD3
{
    public class AnalysisSolution : IDisposable
    {
        private Fermentor _currentFermentor;
        public Fermentor CurrentFermentor
        {
            get => _currentFermentor;
            set => _currentFermentor = value;
        }

        //public ObservableCollection<DeviceParameter> ReactorCol
        //{
        //    get;
        //    private set;
        //} = new ObservableCollection<DeviceParameter>();

        public ObservableCollection<ReportNode> ReportNodeCol
        {
            get;
            private set;
        } = new ObservableCollection<ReportNode>();

        public ObservableCollection<Fermentor> FermentorCol
        {
            get;
            private set;
        } = new ObservableCollection<Fermentor>();

        public EventPublisher EventPublisher { get; private set; } = new EventPublisher();

        private static volatile AnalysisSolution _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private static readonly object _lock1 = new object(); // 锁对象
        private AnalysisSolution()
        {
            string json = File.ReadAllText(FileConst.ReportNodesPath);
            ReportNodeCol = JsonConvert.DeserializeObject<ObservableCollection<ReportNode>>(json);

            LoadSetting();

            if (FermentorCol == null || FermentorCol.Count < 1)
            {
                var device = new DeviceParameter(GeneratePumpSetting(), GenerateMFCSetting()) { Name = "G01"};
                var instance = Fermentor.GetInstance(device);
                FermentorCol.Add(instance);
            }
            CurrentFermentor = FermentorCol.FirstOrDefault();
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
            //LoadPumpSetting();
            //LoadMFCSetting();
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
            FermentorCol = JsonConvert.DeserializeObject<ObservableCollection<Fermentor>>(jsonContent);
        }

        private ObservableCollection<PumpInfo> GeneratePumpSetting()
        {
            ObservableCollection<PumpInfo> col = new ObservableCollection<PumpInfo>();
            string filePath = FileConst.PumpInfoPath;
            if (!File.Exists(filePath))
            {
                return col;
            }
            string jsonContent = AESEncryption.DecryptFile(filePath);
            col = JsonConvert.DeserializeObject<ObservableCollection<PumpInfo>>(jsonContent);
            if (col != null && col.Count > 1)
            {
                col = new ObservableCollection<PumpInfo>(col.OrderBy(t => t.PumpIndex));
            }
            return col;
        }

        private ObservableCollection<MFCInfo> GenerateMFCSetting()
        {
            ObservableCollection<MFCInfo> col = new ObservableCollection<MFCInfo>();
            string filePath = FileConst.MFCInfoPath;
            if (!File.Exists(filePath))
            {
                return col;
            }
            string jsonContent = AESEncryption.DecryptFile(filePath);
            col = JsonConvert.DeserializeObject<ObservableCollection<MFCInfo>>(jsonContent);
            if (col != null && col.Count > 1)
            {
                col = new ObservableCollection<MFCInfo>(col.OrderBy(t => t.MFCIndex));
            }
            return col;
        }

        //public void SaveAllSetting()
        //{
        //    SaveReactorSetting();
        //    //SavePumpSetting();
        //    //SaveMFCSetting();
        //}

        public void SaveReactorSetting(ObservableCollection<Fermentor> dataList = null)
        {
            lock (_lock1)
            {
                if (dataList != null && !dataList.Equals(FermentorCol))
                {
                    FermentorCol = dataList;
                }
                FermentorCol = [.. FermentorCol.DistinctBy(t => t.Device.Name)];
                string json = JsonConvert.SerializeObject(FermentorCol);
                string filePath = string.Format(FileConst.ReactorParamPath, AppSession.CurrentUser?.UserName?.ToString());
                string dir = string.Format(FileConst.ReactorParamDir, AppSession.CurrentUser?.UserName?.ToString());
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string originalFile = filePath;
                string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
                // 检查文件是否存在并重命名
                if (File.Exists(originalFile))
                {
                    File.WriteAllText(newFile, json);
                    File.Move(newFile, originalFile, true);
                    File.Delete(newFile);
                }
                else
                {
                    File.WriteAllText(originalFile, json);
                }
            }
        }

        //public void SavePumpSetting(ObservableCollection<PumpInfo> dataList = null)
        //{
        //    if (dataList != null && !dataList.Equals(PumpInfoCol))
        //    {
        //        PumpInfoCol = dataList;
        //    }
        //    string json = JsonConvert.SerializeObject(PumpInfoCol);

        //    string originalFile = FileConst.PumpInfoPath;
        //    string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
        //    // 检查文件是否存在并重命名
        //    if (File.Exists(originalFile))
        //    {
        //        File.WriteAllText(newFile, json);
        //        File.Move(newFile, originalFile, true);
        //        File.Delete(newFile);
        //    }
        //    else
        //    {
        //        File.WriteAllText(originalFile, json);
        //    }
        //}

        //public void SaveMFCSetting(ObservableCollection<MFCInfo> dataList = null)
        //{
        //    if (dataList != null && !dataList.Equals(MFCInfoCol))
        //    {
        //        MFCInfoCol = dataList;
        //    }
        //    string json = JsonConvert.SerializeObject(MFCInfoCol);
        //    string originalFile = FileConst.MFCInfoPath;
        //    string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
        //    // 检查文件是否存在并重命名
        //    if (File.Exists(originalFile))
        //    {
        //        File.WriteAllText(newFile, json);
        //        File.Move(newFile, originalFile, true);
        //        File.Delete(newFile);
        //    }
        //    else
        //    {
        //        File.WriteAllText(originalFile, json);
        //    }
        //}

        public void SavePumpMFCToOld(string reactorName)
        {
            var dictionary = PumpMFCConfig.GetValue(reactorName);
            if (dictionary == null || dictionary.Keys.Count < 1)
            {
                dictionary = new Dictionary<string, string>();
            }
            var fermentor = FermentorCol.FindFirst(t => t.Device.Name == reactorName);
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 1; i < 7; i++)
            {
                PumpInfo pumpInfo = fermentor.Device.PumpInfoCol.FindFirst(t => t.PumpIndex == i);
                if (pumpInfo == null) continue;
                dictionary[$"Pump{i}"] = pumpInfo.Pump.ToString();
            }
            for (int i = 1; i < 4; i++)
            {
                MFCInfo mfcInfo = fermentor.Device.MFCInfoCol.FindFirst(t => t.MFCIndex == i);
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
