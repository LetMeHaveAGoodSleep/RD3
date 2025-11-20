using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using ImTools;
using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using Prism.Events;
using Prism.Ioc;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3
{
    public class ClockSupervisor
    {
        private static volatile ClockSupervisor _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private static Dictionary<string,object> _dicLock = new Dictionary<string, object>(); // 锁对象

        public System.Timers.Timer MonitorTimer = new System.Timers.Timer();

        private ClockSupervisor()
        {
            StartMonitor();
        }

        public static ClockSupervisor GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ClockSupervisor(); // 实例化
                    }
                }
            }
            return _instance;
        }

        public static int dataMaxCount = 7200;//缓存7200个点
        //public static int dataMaxCount = 10;//缓存7200个点
        public static int pointInterval = 10;//点数间隔
        public static Dictionary<string, List<RealTimeParam>> realDatasDic = new Dictionary<string, List<RealTimeParam>>();
        public static Dictionary<string, List<RealTimeParam>> realDatasDic_tenSecond = new Dictionary<string, List<RealTimeParam>>();
        public static Dictionary<string,List<double>> realData_time = new Dictionary<string, List<double>>();
        Dictionary<string,Pipe> pipesDic = new Dictionary<string, Pipe>();
        void StartMonitor()
        {
            foreach(var item in InstrumentSolution.GetInstance().Instruments)
            {
                if (!AppSession.CurrentUser.DevieceIDs.Contains(item.id)) continue;
                Pipe pipe = PortManager.GetInstance().FindSendPipe(item.id);
                if (pipe != null && !pipesDic.ContainsKey(item.id))
                {
                    _dicLock.Add(item.id, new object());
                    pipesDic.Add(item.id, pipe);
                    realDatasDic.Add(item.id, new List<RealTimeParam>());
                    realDatasDic_tenSecond.Add(item.id, new List<RealTimeParam>());
                    realData_time.Add(item.id, new List<double>());
                    Task.Run(() => 
                    {
                        GetDeviceRealData(item.id);
                    });
                }
            }
        }


        private void GetDeviceRealData(string deviceID)
        {
            while (true)
            {
                try
                {
                    if (!pipesDic.ContainsKey(deviceID))
                        continue;
                    lock (_dicLock[deviceID])
                    {
                        RealTimeParam realTimeParam = new RealTimeParam();
                        if (InstrumentSolution.GetInstance().IsSimulation)//模拟模式
                        {
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceID);
                            OUR_CER(realTimeParam);
                            if (realDatasDic[deviceID].Count > 0)
                            {
                                var lastRealTime = realDatasDic[deviceID][realDatasDic[deviceID].Count - 1];
                                int count = realTimeParam.SampleTime.Second - lastRealTime.SampleTime.Second / AppSession.Interval;
                                for (int i = 1; i < count; i++)
                                {
                                    var copyLastRealTime = lastRealTime.Clone() as RealTimeParam;
                                    copyLastRealTime.SampleTime = copyLastRealTime.SampleTime.AddSeconds(i * AppSession.Interval);
                                    realDatasDic[deviceID].Add(copyLastRealTime);
                                    LogHelper.Warn($"实时信息读取丢点:{deviceID}--{copyLastRealTime.SampleTime.ToString()}");
                                }
                            }
                            realDatasDic[deviceID].Add(realTimeParam);//缓存数据
                            //保存报警码
                            Task.Run(() =>
                            {
                                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                                foreach (byte b in realTimeParam.AlarmBytes)
                                {
                                    try
                                    {
                                        string hex = $"0x{StringUtil.ByteToString(b)}";
                                        Alarm alarm = AlarmManager.GetInstance().Alarms.FindFirst(t => t.Code == hex);
                                        if (alarm == null) continue;
                                        AlarmRecord alarmRecord = new AlarmRecord()
                                        {
                                            Reactor = deviceID,
                                            Source = alarm.Source,
                                            Module = alarm.Module,
                                            Grade = alarm.Grade,
                                            Remark = alarm.Remark,
                                            Code = alarm.Code,
                                        };
                                        AlarmRecordManager.GetInstance().SetAlarmRecord(alarmRecord);
                                    }
                                    catch (Exception ex) { }
                                }
                                AlarmRecordManager.GetInstance().RemoveAlarmRecord(realTimeParam.AlarmBytes, deviceID);
                            });
                        }
                        else//真实模式
                        {
                            if (!pipesDic[deviceID].Connected)//未链接
                            {

                                if (realDatasDic[deviceID].Count > 0)
                                {
                                    var lastRealTime = realDatasDic[deviceID][realDatasDic[deviceID].Count - 1];
                                    int count = realTimeParam.SampleTime.Second - lastRealTime.SampleTime.Second / AppSession.Interval;
                                    for (int i = 1; i < count; i++)
                                    {
                                        var copyLastRealTime = lastRealTime.Clone() as RealTimeParam;
                                        copyLastRealTime.SampleTime = copyLastRealTime.SampleTime.AddSeconds(i * AppSession.Interval);
                                        realDatasDic[deviceID].Add(copyLastRealTime);
                                        LogHelper.Warn($"实时信息读取丢点:{deviceID}--{copyLastRealTime.SampleTime.ToString()}");
                                    }
                                }

                                realDatasDic[deviceID].Add(realTimeParam);
                            }
                            else
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceID);

                                if (realDatasDic[deviceID].Count > 0)
                                {
                                    var lastRealTime = realDatasDic[deviceID][realDatasDic[deviceID].Count - 1];
                                    int count = realTimeParam.SampleTime.Second - lastRealTime.SampleTime.Second / AppSession.Interval;
                                    for (int i = 1; i < count; i++)
                                    {
                                        var copyLastRealTime = lastRealTime.Clone() as RealTimeParam;
                                        copyLastRealTime.SampleTime = copyLastRealTime.SampleTime.AddSeconds(i * AppSession.Interval);
                                        realDatasDic[deviceID].Add(copyLastRealTime);
                                        LogHelper.Warn($"实时信息读取丢点:{deviceID}--{copyLastRealTime.SampleTime.ToString()}");
                                    }
                                }

                                realDatasDic[deviceID].Add(realTimeParam);

                                //保存报警码
                                Task.Run(() => 
                                {
                                    Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                                    foreach (byte b in realTimeParam.AlarmBytes)
                                    {
                                        try
                                        {
                                            string hex = $"0x{StringUtil.ByteToString(b)}";
                                            Alarm alarm = AlarmManager.GetInstance().Alarms.FindFirst(t => t.Code == hex);
                                            if (alarm == null) continue;
                                            AlarmRecord alarmRecord = new AlarmRecord()
                                            {
                                                Reactor = deviceID,
                                                AlarmTime = realTimeParam.SampleTime.ToString(),
                                                Source = alarm.Source,
                                                Module = alarm.Module,
                                                Grade = alarm.Grade,
                                                Remark = alarm.Remark,
                                                Code = alarm.Code,
                                            };
                                            AlarmRecordManager.GetInstance().SetAlarmRecord(alarmRecord);
                                        }
                                        catch (Exception ex) { }
                                    }
                                    AlarmRecordManager.GetInstance().RemoveAlarmRecord(realTimeParam.AlarmBytes, deviceID);
                                });
                            }
                        }
                        if (realTimeParam.SampleTime.Second % pointInterval == 0)
                        {
                            realData_time[deviceID].Add(realTimeParam.SampleTime.ToOADate());
                            realDatasDic_tenSecond[deviceID].Add(realTimeParam);//间隔10秒的数据
                        }

                        while (realDatasDic[deviceID].Count > dataMaxCount)
                        {
                            realDatasDic[deviceID].RemoveAt(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"{deviceID}_{ex.Message}\r\n{ex.StackTrace}";
                    LogHelper.Error(msg);
                }
                finally
                {
                    Thread.Sleep(AppSession.Interval * 1000);
                }
            }
        }

        /// <summary>
        /// 氧利用率
        /// 二氧化碳释放率
        /// </summary>
        private void OUR_CER(RealTimeParam realTimeParam)
        {
            double Fa_i = realTimeParam.AirFlowSpeed;
            double V = 700 / 1000;//700暂且写死 
            double nO2_i = realTimeParam.IntakeModuleO2Concentration;
            double nO2_o = realTimeParam.OffgasModuleO2Concentration;

            double nCO2_i = realTimeParam.IntakeModuleCO2Concentration;
            double nCO2_o = realTimeParam.OffgasModuleCO2Concentration;

            realTimeParam.OUR = (float)SoftwareSensorUtil.CalculateOUR(Fa_i, V, nO2_i, nO2_o, nCO2_o);

            realTimeParam.CER = (float)SoftwareSensorUtil.CalculateCER1(Fa_i, V, nO2_i, nCO2_i, nO2_o, nCO2_o);
            if (realTimeParam.OUR != 0)
            {
                realTimeParam.RQ = realTimeParam.CER / realTimeParam.OUR;
            }
        }
    }
}
