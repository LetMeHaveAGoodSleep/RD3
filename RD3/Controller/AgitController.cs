using ImTools;
using Prism.Mvvm;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3.Controller
{
    public class AgitController
    {
        private DateTime _tsStartTime;

        private Thread _tsThread;

        // 线程退出标志（必须用 volatile 修饰）
        private static volatile bool _shouldStop = false;

        private int _agitSP = -1;

        private BackgroundWorker _backgroundWorker;

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get => _currentDeviceParameter;
            private set => _currentDeviceParameter = value;
        }

        public AgitController(DeviceParameter deviceParameter)
        {
            _currentDeviceParameter = deviceParameter;
        }

        public void StartWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                return;
            }

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (s, e) =>
            {
                try
                {
                    CurrentDeviceParameter.AgitParam.SP = Math.Clamp(CurrentDeviceParameter.AgitParam.SP, CurrentDeviceParameter.AgitParam.LowerLimit, CurrentDeviceParameter.AgitParam.UpperLimit);
                    InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, CurrentDeviceParameter.AgitParam.SP);
                    _agitSP = CurrentDeviceParameter.AgitParam.SP;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("设置转速异常" + ex.Message);
                }
                BackgroundWorker worker = s as BackgroundWorker;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    try
                    {
                        if (_agitSP != CurrentDeviceParameter.AgitParam.SP || CurrentDeviceParameter.AgitParam.SP < CurrentDeviceParameter.AgitParam.LowerLimit || CurrentDeviceParameter.AgitParam.SP > CurrentDeviceParameter.AgitParam.UpperLimit)
                        {
                            CurrentDeviceParameter.AgitParam.SP = Math.Clamp(CurrentDeviceParameter.AgitParam.SP, CurrentDeviceParameter.AgitParam.LowerLimit, CurrentDeviceParameter.AgitParam.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, CurrentDeviceParameter.AgitParam.SP);
                            _agitSP = CurrentDeviceParameter.AgitParam.SP;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("转速控制失败" + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;
            };
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                Thread.Sleep(100);
            }

            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, 0);
        }


        /// <summary>
        /// 将时间值转换为分钟（统一单位用于比较）
        /// </summary>
        /// <param name="value">原始时间值</param>
        /// <param name="unit">时间单位</param>
        private double ConvertToMinutes(double value, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Hour => value * 60,    // 小时 -> 分钟
                TimeUnit.Day => value * 60 * 24, // 天 -> 分钟
                _ => value, // 默认分钟
            };
        }

        /// <summary>
        /// 将绝对时间转换为指定单位的数值（用于匹配序列项）
        /// </summary>
        private double ConvertToMinutes(DateTime time, TimeUnit unit)
        {
            return unit switch
            {
                TimeUnit.Minute => time.TimeOfDay.TotalMinutes, // 当天分钟数
                TimeUnit.Hour => time.TimeOfDay.TotalHours,     // 当天小时数
                TimeUnit.Day => (time - new DateTime(time.Year, time.Month, 1)).TotalDays + 1, // 当月天数
                _ => time.TimeOfDay.TotalMinutes,
            };
        }

        /// <summary>
        /// 执行时间序列项的操作（需根据实际业务实现）
        /// </summary>
        private void ExecuteTimeSeriesItem(TimeSeriesItem item)
        {
            LogHelper.Debug($"执行转速时间序列项：[{item.StartTime}-{item.EndTime}]，操作值：{item.Value}");
            // 示例：设置SP值（根据实际业务替换）
            CurrentDeviceParameter.AgitParam.SP = (int)item.Value;
            // 其他操作：如控制设备、记录日志等
        }

        /// <summary>
        /// 判断时间序列是否已全部执行完成
        /// </summary>
        private bool IsTimeSeriesCompleted(List<TimeSeriesItem> sortedItems, double currentTime, TimeUnit unit)
        {
            var lastItem = sortedItems.LastOrDefault();
            if (lastItem == null) return true;

            double lastItemEnd = ConvertToMinutes(lastItem.EndTime, unit);
            return currentTime > lastItemEnd; // 当前时间超过最后一项的结束时间
        }

        /// <summary>
        /// 绝对时间：当前时间
        /// 相对时间：批次开始时间
        /// </summary>
        public void StartTimeSeriesWork()
        {
            var timeSeries = CurrentDeviceParameter.AgitParam.TimeSeries;
            var items = timeSeries.TimeSeriesItemCol.OrderBy(t => t.StartTime).ThenBy(t => t.EndTime).ToList();

            this._tsStartTime = DateTime.Now;
            if (CurrentDeviceParameter.AgitParam.TimeSeries.TimeType == TimeType.RelativeTime)
            {
                if (CurrentDeviceParameter.BatchID < 1)
                {
                    _tsStartTime = DateTime.Now;
                }
                else
                {
                    var batch = RD3SQLHelper.QueryBatchByID(CurrentDeviceParameter.BatchID);
                    _tsStartTime = Convert.ToDateTime(batch.startDateTime);
                }
            }
            StopTimeSeriesWork();
            _shouldStop = false;
            _tsThread = new Thread(() =>
            {
                TimeSeriesItem lastExecutedItem = null; // 记录上一个执行的序列项（避免重复执行）
                LogHelper.Debug($"{CurrentDeviceParameter.Name}的转速时间序列开始执行（{timeSeries.TimeType}模式）");

                while (!_shouldStop)
                {
                    try
                    {
                        // 1. 计算当前时间对应的“匹配时间”（相对时间/绝对时间）
                        double currentMatchTime;
                        DateTime currentTime = DateTime.Now;

                        if (timeSeries.TimeType == TimeType.RelativeTime)
                        {
                            // 相对时间：计算相对于基准时间的已运行分钟数
                            currentMatchTime = (currentTime - _tsStartTime).TotalMinutes;
                            // 转换单位（将序列项的时间转换为分钟，与currentMatchTime统一单位）
                            currentMatchTime = ConvertToMinutes(currentMatchTime, timeSeries.Timer);
                        }
                        else
                        {
                            // 绝对时间：直接用当前时间的分钟数（或根据单位转换）
                            currentMatchTime = ConvertToMinutes(currentTime, timeSeries.Timer);
                        }

                        // 2. 查找当前时间匹配的序列项（在StartTime和EndTime之间）
                        var matchedItem = items.FirstOrDefault(item =>
                        {
                            double itemStart = ConvertToMinutes(item.StartTime, timeSeries.Timer);
                            double itemEnd = ConvertToMinutes(item.EndTime, timeSeries.Timer);
                            return currentMatchTime >= itemStart && currentMatchTime <= itemEnd;
                        });

                        if (matchedItem != null)
                        {
                            // 获取当前阶段索引（+1转为阶段序号）
                            int currentStageIndex = items.IndexOf(matchedItem);
                            int currentStage = currentStageIndex + 1;

                            // 计算该阶段已运行时长（当前时间 - 阶段开始时间）
                            double stageBegin = ConvertToMinutes(matchedItem.StartTime, timeSeries.Timer);
                            double elapsedInStage = currentMatchTime - stageBegin;

                            // 生成运行信息（包含阶段进度）
                            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedInStage * 60);
                            CurrentDeviceParameter.AgitParam.TimeSeries.RunningInfo = $"时间序列运行到{currentStage}/{items.Count}阶段，该阶段已运行：{$"{timeSpan.Days:00}天{timeSpan.Hours:00}时{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒"}";
                            LogHelper.Debug(CurrentDeviceParameter.AgitParam.TimeSeries.RunningInfo);

                            // 执行阶段操作（仅首次匹配时）
                            if (matchedItem != lastExecutedItem)
                            {
                                ExecuteTimeSeriesItem(matchedItem);
                                lastExecutedItem = matchedItem;
                                LogHelper.Debug($"进入第{currentStage}阶段：[{matchedItem.StartTime}-{matchedItem.EndTime}]{timeSeries.Timer}");
                            }
                        }

                        // 4. 检查是否已超出所有序列项的结束时间（终止线程）
                        else if (IsTimeSeriesCompleted(items, currentMatchTime, timeSeries.Timer))
                        {
                            CurrentDeviceParameter.AgitParam.TimeSeries.RunningInfo = $"转速时间序列已全部执行完成";
                            LogHelper.Debug($"{CurrentDeviceParameter.Name}的转速时间序列已全部执行完成");
                            _shouldStop = true;
                        }
                        else
                        {
                            TimeSpan timeSpan = TimeSpan.FromSeconds(currentMatchTime * 60);
                            CurrentDeviceParameter.AgitParam.TimeSeries.RunningInfo = $"时间序列已运行：{$"{timeSpan.Days:00}天{timeSpan.Hours:00}时{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒"}";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"{CurrentDeviceParameter.Name}的转速时间序列执行出错：{ex.Message}", ex);
                    }

                    // 轮询间隔（500ms，可根据精度需求调整）
                    Thread.Sleep(500);
                }

                CurrentDeviceParameter.AgitParam.IsControling = false;
                LogHelper.Debug($"{CurrentDeviceParameter.Name}的转速时间序列线程已停止");
            });
            _tsThread.Priority = ThreadPriority.Lowest;
            _tsThread.IsBackground = true;
            _tsThread.Start();
        }

        public void StopTimeSeriesWork()
        {
            _shouldStop = true; // 设置退出标志
            _tsThread?.Join(); // 等待线程结束
            CurrentDeviceParameter.AgitParam.TimeSeries.RunningInfo = string.Empty;
        }
    }
}
