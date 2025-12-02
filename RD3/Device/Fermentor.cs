using Newtonsoft.Json;
using Prism.Mvvm;
using RD3.Common;
using RD3.Controller;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3
{
    public class Fermentor : BindableBase
    {
        public Fermentor()
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (Device == null)
                        {
                            continue;
                        }
                        var auditParams = AuditLogUtil.ExtractDeviceParamAuditLogs(Device);
                        foreach (AuditParam param in auditParams) 
                        {
                            foreach (var item in param.AuditLogs)
                            {
                                item.Operator = AppSession.CurrentUser.UserName;
                                string remark = param.ModuleName + "：" + item.PropertyName + "由" + "[" + item.OldValue + "]" + "变更为" + "[" + item.NewValue + "]";
                                RD3SQLHelper.AddAuditRecord(Device.Name, Device.BatchID.ToString(), item.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"), remark, item.Operator);
                                item.IsUsed = true;
                            }
                            param.RemoveUsedLogs();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(5000);
                    }
                }
            });
            thread.Priority = ThreadPriority.Lowest;
            thread.IsBackground = true;
            thread.Start();
        }

        private DeviceParameter _device;
        public DeviceParameter Device
        {
            get => _device;
            set 
            { 
                SetProperty(ref _device, value);

                AgitController = new(Device);
                DOController = new(Device);
                TempController = new(Device);
                pHController = new(Device);
                CondensationController = new(Device);
            }
        }

        [JsonIgnore]
        public AgitController AgitController { get; private set; }

        [JsonIgnore]
        public DOController DOController { get; private set; }

        [JsonIgnore]
        public TempController TempController { get; private set; }

        [JsonIgnore]
        public pHController pHController { get; private set; }

        [JsonIgnore]
        public CondensationController CondensationController { get; private set; }

        private static volatile Fermentor _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private Fermentor(DeviceParameter device) : this()
        {
            Device = device;
        }

        public static Fermentor GetInstance(DeviceParameter device)
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new Fermentor(device); // 实例化
                    }
                }
            }
            return _instance;
        }
    }
}
