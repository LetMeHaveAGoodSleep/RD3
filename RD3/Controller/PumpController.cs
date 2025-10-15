using Fpi.Communication.Commands;
using ImTools;
using RD3.Common;
using RD3.Shared;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.Controller
{
    public class PumpController
    {
        private float _flowRateSP = -1;

        private BackgroundWorker _backgroundWorker;

        private ProbingController _probingController;

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }

        private PumpInfo _pumpInfo;
        public PumpInfo PumpInfo
        {
            get => _pumpInfo;
            set => _pumpInfo = value;
        }

        public PumpController()
        {
        }

        /// <summary>
        /// 下发控制指令
        /// </summary>
        /// <param name="flowRate"></param>
        /// <param name="flowCapacity"></param>
        private void SetCommand(float flowRate,float flowCapacity)
        {
            PumpInfo.FlowRate_SP = Math.Clamp(flowRate, 0, Const.MaxPumpFlowRate);
            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
            {
                PumpNo = PumpInfo.PumpIndex,
                Pump = PumpInfo.Pump,
                ControlMode = PumpControlMode.Direct,
                FlowSpeed = PumpInfo.FlowRate_SP,
                FlowCapacity = flowCapacity
            };
            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
        }

        public void StartWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                return;
            }

            if (_probingController != null)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == CurrentDeviceParameter.Name);
                _probingController.SetDevice(CurrentDeviceParameter);
                _probingController.InitProbParam(param);
                _probingController.StopCtrl();
                Thread.Sleep(100);
            }

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (s, e) =>
            {
                double totalSeconds = 0;
                double totalSecond = 0;
                double secondCount = 0;
                BackgroundWorker worker = s as BackgroundWorker;
                e.Result = CurrentDeviceParameter.Name;
                var deviceParameter = CurrentDeviceParameter;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam();

                if (PumpInfo.Pump != PeristalticPump.FeedPump && PumpInfo.Pump != PeristalticPump.Feed2Pump)
                {
                    try
                    {
                        PumpInfo.FlowRate_SP = Math.Clamp(PumpInfo.FlowRate_SP, 0, Const.MaxPumpFlowRate);
                        param = new PeristalticPumpControlParam()
                        {
                            PumpNo = PumpInfo.PumpIndex,
                            Pump = PumpInfo.Pump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = PumpInfo.FlowRate_SP,
                            FlowCapacity = PumpInfo.FlowRate_SP * PumpInfo.RunningTime_SP / 3600f
                        };
                        if (PumpInfo.IsConstSpeed)
                        {
                            param.FlowCapacity = int.MaxValue;
                        }
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
                        _flowRateSP = PumpInfo.FlowRate_SP;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug($"泵{PumpInfo.PumpIndex}设置流速出错" + ex.Message);
                    }
                    
                    while (true)
                    {
                        if (worker.CancellationPending)
                        {
                            return;
                        }
                        try
                        {
                            if (_flowRateSP != PumpInfo.FlowRate_SP)
                            {
                                PumpInfo.FlowRate_SP = Math.Clamp(PumpInfo.FlowRate_SP, 0, Const.MaxPumpFlowRate);
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = PumpInfo.PumpIndex,
                                    Pump = PumpInfo.Pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = PumpInfo.FlowRate_SP,
                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                };
                                if (PumpInfo.IsConstSpeed)
                                {
                                    param.FlowCapacity = int.MaxValue;
                                }
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
                                _flowRateSP = PumpInfo.FlowRate_SP;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug($"泵{PumpInfo.PumpIndex}设置流速出错" + ex.Message);
                        }
                        Thread.Sleep(1000);
                    }
                }

                switch (PumpInfo.FeedMode)
                {
                    case FeedControlMode.ConstantSpeed:
                        #region 恒速补料
                        try
                        {
                            SetCommand(PumpInfo.FlowRate_SP, Const.MaxPumpFlowCapacity);
                            _flowRateSP = PumpInfo.FlowRate_SP;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug($"恒速补料：泵{PumpInfo.PumpIndex}设置流速出错" + ex.Message);
                        }
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            try
                            {
                                if (_flowRateSP != PumpInfo.FlowRate_SP)
                                {
                                    SetCommand(PumpInfo.FlowRate_SP, Const.MaxPumpFlowCapacity);
                                    _flowRateSP = PumpInfo.FlowRate_SP;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug($"恒速补料：泵{PumpInfo.PumpIndex}设置流速出错" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                        #endregion
                        break;
                    case FeedControlMode.Polynomial:
                        #region 多项式
                        totalSecond = 0d;
                        totalSecond = 0d;
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                if (deviceParameter.FeedSuspend)
                                {
                                    Thread.Sleep(1000);
                                    continue;
                                }

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "Polynomial".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在多项式策略", deviceParameter.Name));
                                    return;
                                }

                                double interval = secondCount / 3600;
                                double a = f.A;//20
                                double b = double.Parse(f.B);//0.7
                                double c = f.C;//40
                                double deltaT = double.Parse(f.D);
                                double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                                double feed = Math.Round(a * Math.Pow(diff, 2) + b * diff + c, 2);

                                SetCommand((float)feed, Const.MaxPumpFlowCapacity);

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }

                                secondCount += 1;

                                if (f.StatDisable)
                                {
                                    totalSecond = 0;
                                }
                                else
                                {
                                    totalSecond += 1;
                                }

                                if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                {
                                    if (f.CurveDOStat || f.CurvepHStat)
                                    {
                                        SetCommand(0, 0);
                                    }

                                    if (f.CurveDOStat)
                                    {
                                        while (true)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }
                                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (f.StatSymbol == 0)//小于
                                            {
                                                if (realTime.DO < f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            else if (f.StatSymbol == 1)//大于
                                            {
                                                if (realTime.DO > f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (f.CurvepHStat)
                                    {
                                        int pauseSecond = 0;

                                        while (true)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }
                                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (f.StatSymbol == 0)//小于
                                            {
                                                if (realTime.PH < f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            else if (f.StatSymbol == 1)//大于
                                            {
                                                if (realTime.PH > f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            Thread.Sleep(1000);
                                            pauseSecond += 1;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料多项式调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(AppSession.Interval * 1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.Exponential:
                        #region 指数
                        totalSecond = 0d;
                        totalSecond = 0d;
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                if (deviceParameter.FeedSuspend)
                                {
                                    Thread.Sleep(1000);
                                    continue;
                                }
                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }
                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "Exponential".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在指数策略", deviceParameter.Name));
                                    return;
                                }

                                double interval = secondCount / 3600;
                                double f1 = f.A;//20
                                double μ = double.Parse(f.B);//0.7
                                double deltaT = f.C;//Δt
                                double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                                double feed = Math.Round(f1 * Math.Exp(μ * (diff)), 2);

                                SetCommand((float)feed, Const.MaxPumpFlowCapacity);

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }
                                secondCount += 1;

                                if (f.StatDisable)
                                {
                                    totalSecond = 0;
                                }
                                else
                                {
                                    totalSecond += 1;
                                }

                                if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                {
                                    if (f.CurveDOStat || f.CurvepHStat)
                                    {
                                        SetCommand(0, 0);
                                    }

                                    if (f.CurveDOStat)
                                    {
                                        while (true)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }
                                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (f.StatSymbol == 0)//小于
                                            {
                                                if (realTime.DO < f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            else if (f.StatSymbol == 1)//大于
                                            {
                                                if (realTime.DO > f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (f.CurvepHStat)
                                    {
                                        int pauseSecond = 0;

                                        while (true)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }
                                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (f.StatSymbol == 0)//小于
                                            {
                                                if (realTime.PH < f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            else if (f.StatSymbol == 1)//大于
                                            {
                                                if (realTime.PH > f.StatValue)
                                                {
                                                    totalSecond = 0;
                                                    break;
                                                }
                                            }
                                            Thread.Sleep(1000);
                                            pauseSecond += 1;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料指数调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(AppSession.Interval * 1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.TimeSeries:
                        #region 时间序列
                        bool QuantitativeFinish = false;//指示时间序列中定量补是否已经完成

                        bool flag = FeedGradientManager.GetInstance().FeedGradientCol.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                        if (!flag || feedGradientInfos == null)
                        {
                           HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                            return;
                        }
                        double endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime * 60;
                        double timeOffset = 0;//时间差-秒

                        totalSecond = 0;//用于stat的停顿计时

                        double statTotalSeconds = 0;//用于stat多项式|指数的时间计算

                        double calcTotalSeconds = 0;

                        string lastInfoType = "";

                        while (timeOffset < endTime)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            if (deviceParameter.FeedSuspend)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }

                            flag = FeedGradientManager.GetInstance().FeedGradientCol.TryGetValue(deviceParameter.Name, out feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }

                            FeedGradientInfo f = null;

                            foreach (var feedGradientInfo in feedGradientInfos)
                            {
                                if (feedGradientInfo.BeginTime <= timeOffset / 60d && feedGradientInfo.EndTime > timeOffset / 60d)
                                {
                                    f = feedGradientInfo;
                                    break;
                                }
                            }
                            if (f != null)
                            {
                                if (lastInfoType != f.InfoType)
                                {
                                    totalSecond = 0;
                                    calcTotalSeconds = 0;
                                    statTotalSeconds = 0;
                                }

                                switch (f.InfoType)
                                {
                                    case "Constant":
                                        SetCommand(f.A, Const.MaxPumpFlowCapacity);

                                        int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }

                                        if (f.StatDisable)
                                        {
                                            totalSecond = 0;
                                        }
                                        else
                                        {
                                            totalSecond += 1;
                                        }

                                        if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                        {
                                            SetCommand(0, 0);

                                            if (f.CurveDOStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.DO < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.DO > f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                            else if (f.CurvepHStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.PH < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.PH > f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                        }
                                        break;
                                    case "Polynomial"://多项式，执行业务逻辑
                                        double a = f.A;//20
                                        double b = double.Parse(f.B);//0.7
                                        double c = f.C;//40
                                        double calcTimeOffset = Math.Round(calcTotalSeconds / 3600, 2);
                                        double feed = Math.Round(a * Math.Pow(calcTimeOffset, 2) + b * calcTimeOffset + c, 2);

                                        SetCommand((float)feed, Const.MaxPumpFlowCapacity);

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }

                                        calcTotalSeconds += 1;

                                        if (f.StatDisable)
                                        {
                                            totalSecond = 0;
                                        }
                                        else
                                        {
                                            totalSecond += 1;
                                        }

                                        if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                        {
                                            SetCommand(0, 0);

                                            if (f.CurveDOStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.DO < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.DO > f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                            else if (f.CurvepHStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.PH < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.PH > f.StatValue)
                                                        {

                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                        }
                                        break;
                                    case "Exponential"://指数，执行业务逻辑
                                        double f1 = f.A;//20
                                        double μ = double.Parse(f.B);//0.7
                                        double calcTimeOffset1 = Math.Round(calcTotalSeconds / 3600, 2);
                                        double feed1 = Math.Round(f1 * Math.Exp(μ * calcTimeOffset1), 2);

                                        SetCommand((float)feed1, Const.MaxPumpFlowCapacity);

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }

                                        calcTotalSeconds += 1;

                                        if (f.StatDisable)
                                        {
                                            totalSecond = 0;
                                        }
                                        else
                                        {
                                            totalSecond += 1;
                                        }

                                        if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                        {
                                            SetCommand(0, 0);

                                            if (f.CurveDOStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.DO < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.DO > f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                            else if (f.CurvepHStat)
                                            {
                                                while (true)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                    if (f.StatSymbol == 0)//小于
                                                    {
                                                        if (realTime.PH < f.StatValue)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    else if (f.StatSymbol == 1)//大于
                                                    {
                                                        if (realTime.PH > f.StatValue)
                                                        {

                                                            break;
                                                        }
                                                    }
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }
                                        }
                                        break;
                                    case "DO_Feedback"://DO-stat(速度)
                                        if (deviceParameter.DO <= f.A)
                                        {
                                            if (f.IsConstant)
                                            {
                                                SetCommand(float.Parse(f.B), Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }

                                            statTotalSeconds += 1;

                                            count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else if (deviceParameter.DO >= f.C)
                                        {
                                            if (f.IsConstant)
                                            {
                                                SetCommand(float.Parse(f.D), Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }

                                            statTotalSeconds += 1;

                                            count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                            
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }
                                        break;
                                    case "PH_Feedback"://根据PH反馈控制，执行业务逻辑
                                        if (deviceParameter.PH <= f.A)
                                        {
                                            if (f.IsConstant)
                                            {
                                                SetCommand(float.Parse(f.B), Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }

                                            statTotalSeconds += 1;

                                            count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else if (deviceParameter.PH >= f.C)
                                        {
                                            if (f.IsConstant)
                                            {
                                                SetCommand(float.Parse(f.D), Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                                SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                            }

                                            statTotalSeconds += 1;

                                            count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }
                                        break;
                                    case "DO_Feedback_Total":
                                        if (deviceParameter.DO <= f.A)
                                        {
                                            if (f.IsConstant)
                                            {
                                                float flow = float.Parse(f.B);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp1 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp1 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp1;
                                                float speed1 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume1 = speed1 * count2 / 3600f;
                                                SetCommand(speed1, remainingVolume1);
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {

                                                        return;
                                                    }
                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp2 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp2 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp2;
                                                float speed2 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume2 = speed2 * count2 / 3600f;
                                                SetCommand(speed2, remainingVolume2);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp3 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp3 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp3;
                                                float speed3 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume3 = speed3 * count2 / 3600f;
                                                SetCommand(speed3, remainingVolume3);
                                                PumpInfo.FlowRate_SP = 0;
                                            }

                                            statTotalSeconds += 1;

                                            int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count5 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count5--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else if (deviceParameter.DO >= f.C)
                                        {
                                            if (f.IsConstant)
                                            {
                                                float flow = float.Parse(f.D);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp4 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp4 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp4;
                                                float speed4 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume4 = speed4 * count2 / 3600f;
                                                SetCommand(speed4, remainingVolume4);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp5 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp5 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp5;
                                                float speed5 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume5 = speed5 * count2 / 3600f;
                                                SetCommand(speed5, remainingVolume5);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp6 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp6 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp6;
                                                float speed6 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume6 = speed6 * count2 / 3600f;
                                                SetCommand(speed6, remainingVolume6);
                                                PumpInfo.FlowRate_SP = 0;
                                            }

                                            int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count5 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count5--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);   
                                            Thread.Sleep(1000);
                                        }
                                        break;
                                    case "PH_Feedback_Total"://根据PH反馈控制，执行业务逻辑
                                        if (deviceParameter.PH <= f.A)
                                        {
                                            if (f.IsConstant)
                                            {
                                                float flow = float.Parse(f.B);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp1 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp1 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp1;
                                                float speed1 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume1 = speed1 * count2 / 3600f;
                                                SetCommand(speed1, remainingVolume1);
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {

                                                        return;
                                                    }
                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp2 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp2 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp2;
                                                float speed2 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume2 = speed2 * count2 / 3600f;
                                                SetCommand(speed2, remainingVolume2);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp3 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp3 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp3;
                                                float speed3 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume3 = speed3 * count2 / 3600f;
                                                SetCommand(speed3, remainingVolume3);
                                                PumpInfo.FlowRate_SP = 0;
                                            }

                                            statTotalSeconds += 1;

                                            int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count5 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count5--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else if (deviceParameter.PH >= f.C)
                                        {
                                            if (f.IsConstant)
                                            {
                                                float flow = float.Parse(f.D);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp4 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp4 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp4;
                                                float speed4 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume4 = speed4 * count2 / 3600f;
                                                SetCommand(speed4, remainingVolume4);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsPolynomial)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramD > 0 ? offset - paramD : 0;
                                                double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp5 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp5 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp5;
                                                float speed5 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume5 = speed5 * count2 / 3600f;
                                                SetCommand(speed5, remainingVolume5);
                                                PumpInfo.FlowRate_SP = 0;
                                            }
                                            else if (f.IsExp)
                                            {
                                                double paramA, paramB, paramC, paramD;
                                                var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                                paramA = array[0];
                                                paramB = array[1];
                                                paramC = array[2];
                                                paramD = array[3];
                                                double offset = statTotalSeconds / 3600;
                                                double diff = offset - paramC > 0 ? offset - paramC : 0;
                                                double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                                if (flow > 0)
                                                {
                                                    SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                                }
                                                else
                                                {
                                                    SetCommand(0, 0);
                                                }

                                                int temp6 = 1;
                                                if (PumpInfo.FlowRate_SP > 0)
                                                {
                                                    temp6 = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                                }
                                                bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                                int count2 = flag2 == true ? 1 : temp6;
                                                float speed6 = PumpInfo.FlowRate_SP;
                                                while (count2 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }

                                                    if (deviceParameter.FeedSuspend)
                                                    {
                                                        break;
                                                    }

                                                    count2--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                                while (deviceParameter.FeedSuspend)
                                                {
                                                    Thread.Sleep(1000);
                                                }
                                                float remainingVolume6 = speed6 * count2 / 3600f;
                                                SetCommand(speed6, remainingVolume6);
                                                PumpInfo.FlowRate_SP = 0;
                                            }

                                            int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count5 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count5--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                            Thread.Sleep(1000);
                                        }
                                        break;
                                    case "Quantitative":
                                        if (f.A <= 0 || QuantitativeFinish)
                                        {
                                            continue;
                                        }
                                        float.TryParse(f.B, out var b1);
                                        SetCommand(b1, f.A);
                                        QuantitativeFinish = true;

                                        int count10 = Convert.ToInt32(Math.Ceiling(f.A / PumpInfo.FlowRate_SP * 3600)); ;
                                        while (count10 > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count10--;
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count10 / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        while (count10 > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }
                                            count10--;
                                            Thread.Sleep(1000);

                                            timeOffset++;
                                        }

                                        PumpInfo.FlowRate_SP = 0;
                                        break;
                                    case "Cycle":
                                        try
                                        {
                                            int costCycleSeconds = 1;
                                            double totalMinutes = 0;

                                            if (f.A <= 0 || !float.TryParse(f.B, out var paramB) || paramB <= 0 || f.C <= 0 || !float.TryParse(f.D, out var paramD) || paramD <= 0)
                                            {
                                                PumpInfo.FlowRate_SP = 0;
                                                continue;
                                            }
                                            SetCommand(f.C, paramD);

                                            int count11 = Convert.ToInt32(Math.Ceiling(paramD / PumpInfo.FlowRate_SP * 3600));
                                            while (count11 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                if (deviceParameter.FeedSuspend)
                                                {
                                                    break;
                                                }

                                                count11--;
                                                Thread.Sleep(1000);
                                                costCycleSeconds++;

                                                timeOffset++;
                                            }

                                            while (deviceParameter.FeedSuspend)
                                            {
                                                Thread.Sleep(1000);
                                            }
                                            float remainingVolume1 = PumpInfo.FlowRate_SP * count11 / 3600f;
                                            SetCommand(PumpInfo.FlowRate_SP, remainingVolume1);
                                            while (count11 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                count11--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }

                                            totalMinutes = costCycleSeconds / 60f;

                                            PumpInfo.FlowRate_SP = 0;

                                            double diff = f.A - totalMinutes;
                                            if (diff > 0)
                                            {
                                                int count12 = Convert.ToInt32(diff * 60);
                                                while (count12 > 0)
                                                {
                                                    if (worker.CancellationPending)
                                                    {
                                                        return;
                                                    }
                                                    count12--;
                                                    Thread.Sleep(1000);

                                                    timeOffset++;
                                                }
                                            }

                                            int count7 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                            while (count7 > 0)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }

                                                count7--;
                                                Thread.Sleep(1000);

                                                timeOffset++;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                                            Thread.Sleep(AppSession.Interval * 1000);
                                        }
                                        break;
                                    default:
                                        SetCommand(0, 0);
                                        Thread.Sleep(1000);

                                        timeOffset++;
                                        break;
                                }

                                lastInfoType = f.InfoType;
                            }
                            endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime * 60;
                        }
                        #endregion
                        break;
                    case FeedControlMode.DO_stat_Speed:
                        #region DO_stat_速度
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在DO_stat_流速策略", deviceParameter.Name));
                                    return;
                                }

                                if (deviceParameter.DO <= f.A)
                                {
                                    if (f.IsConstant)
                                    {
                                        SetCommand(float.Parse(f.B), Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }

                                    totalSeconds += 1;

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (deviceParameter.DO >= f.C)
                                {
                                    if (f.IsConstant)
                                    {
                                        SetCommand(float.Parse(f.D), Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }

                                    totalSeconds += 1;

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    SetCommand(0, 0);
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料DO反馈调整失败，错误信息：{0}", ex.Message));
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.pH_stat_Speed:
                        #region pH_stat_速度
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在pH_stat_流速策略", deviceParameter.Name));
                                    return;
                                }

                                if (deviceParameter.PH <= f.A)
                                {
                                    if (f.IsConstant)
                                    {
                                        SetCommand(float.Parse(f.B), Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }

                                    totalSeconds += 1;

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (deviceParameter.PH >= f.C)
                                {
                                    if (f.IsConstant)
                                    {
                                        SetCommand(float.Parse(f.D), Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                        SetCommand((float)flowRate, Const.MaxPumpFlowCapacity);
                                    }

                                    totalSeconds += 1;

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    SetCommand(0, 0);
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                            }
                            finally
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.DO_stat_Volume:
                        #region DO_stat_体积
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback_Total".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在DO_stat_体积策略", deviceParameter.Name));
                                    return;
                                }

                                if (deviceParameter.DO <= f.A)
                                {
                                    if (f.IsConstant)
                                    {
                                        float flow = float.Parse(f.B);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }

                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }

                                    totalSeconds += 1;

                                    int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count1 > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count1--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (deviceParameter.DO >= f.C)
                                {
                                    if (f.IsConstant)
                                    {
                                        float flow = float.Parse(f.D);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }

                                    totalSeconds += 1;

                                    int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count1 > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count1--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    SetCommand(0, 0);
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料DO反馈总量调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(AppSession.Interval * 1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.pH_stat_Volume:
                        #region pH_stat_体积
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback_Total".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在pH_stat_体积策略", deviceParameter.Name));
                                    return;
                                }

                                if (deviceParameter.PH <= f.A)
                                {
                                    if (f.IsConstant)
                                    {
                                        float flow = float.Parse(f.B);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }

                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }

                                    totalSeconds += 1;

                                    int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count1 > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count1--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (deviceParameter.PH >= f.C)
                                {
                                    if (f.IsConstant)
                                    {
                                        float flow = float.Parse(f.D);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsPolynomial)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramD > 0 ? offset - paramD : 0;
                                        double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }
                                    else if (f.IsExp)
                                    {
                                        double paramA, paramB, paramC, paramD;
                                        var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                        paramA = array[0];
                                        paramB = array[1];
                                        paramC = array[2];
                                        paramD = array[3];
                                        double offset = totalSeconds / 3600;
                                        double diff = offset - paramC > 0 ? offset - paramC : 0;
                                        double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                        if (flow > 0)
                                        {
                                            SetCommand(AppSession.DefaultPumpFlowRate, (float)flow);
                                        }
                                        else
                                        {
                                            SetCommand(0, 0);
                                        }


                                        int temp = 1;
                                        if (PumpInfo.FlowRate_SP > 0)
                                        {
                                            temp = Convert.ToInt32(Math.Ceiling(flow / PumpInfo.FlowRate_SP * 3600));
                                        }
                                        bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                        int count = flag2 == true ? 1 : temp;
                                        while (count > 0)
                                        {
                                            if (worker.CancellationPending)
                                            {
                                                return;
                                            }

                                            if (deviceParameter.FeedSuspend)
                                            {
                                                break;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                        while (deviceParameter.FeedSuspend)
                                        {
                                            Thread.Sleep(1000);
                                        }
                                        float remainingVolume = PumpInfo.FlowRate_SP * count / 3600f;
                                        SetCommand(PumpInfo.FlowRate_SP, remainingVolume);
                                        PumpInfo.FlowRate_SP = 0;
                                    }

                                    totalSeconds += 1;

                                    int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count1 > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }

                                        count1--;
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    SetCommand(0, 0);
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(AppSession.Interval * 1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.Quantitative:
                        #region 单次定量
                        if (PumpInfo.FlowCapacity_SP <= 0)
                        {
                            HandyControl.Controls.MessageBox.Warning("设定体积必须大于0", "温馨提示");
                            return;
                        }
                        SetCommand(PumpInfo.FlowRate_SP, PumpInfo.FlowCapacity_SP);

                        int countQuantitative = 0;
                        if (PumpInfo.FlowRate_SP > 0)
                        {
                            countQuantitative = Convert.ToInt32(Math.Ceiling(PumpInfo.FlowCapacity_SP / PumpInfo.FlowRate_SP * 3600));
                        }
                        while (countQuantitative > 0)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            if (deviceParameter.FeedSuspend)
                            {
                                break;
                            }

                            countQuantitative--;
                            Thread.Sleep(1000);
                        }
                        while (deviceParameter.FeedSuspend)
                        {
                            Thread.Sleep(1000);
                        }
                        float remainingVolumeQuantitative = PumpInfo.FlowRate_SP * countQuantitative / 3600f;
                        SetCommand(PumpInfo.FlowRate_SP, remainingVolumeQuantitative);
                        while (countQuantitative > 0)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            countQuantitative--;
                            Thread.Sleep(1000);
                        }
                        #endregion
                        break;
                    case FeedControlMode.Cycle:
                        #region 周期
                        while (true)
                        {
                            try
                            {
                                if (worker.CancellationPending)
                                {
                                    return;
                                }

                                int costCycleSeconds = 1;
                                double totalMinutes = 0;

                                bool isExist = FeedStrategyManager.GetInstance().FeedStrategyCol.TryGetValue(deviceParameter.Name, out var feedStrategyInfos);
                                if (!isExist || feedStrategyInfos == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                    return;
                                }

                                var f = feedStrategyInfos.FindFirst(t => t.InfoType.ToUpper() == "Cycle".ToUpper() && t.Pump == PumpInfo.Pump);
                                if (f == null)
                                {
                                    HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在周期补料策略", deviceParameter.Name));
                                    return;
                                }

                                if (f.A <= 0 || !float.TryParse(f.B, out var b) || b <= 0 || f.C <= 0 || !float.TryParse(f.D, out var d) || d <= 0)
                                {
                                    PumpInfo.FlowRate_SP = 0;
                                    continue;
                                }
                                PumpInfo.FlowRate_SP = f.C;
                                SetCommand(PumpInfo.FlowRate_SP, d);

                                int countCycle = 1;
                                if (PumpInfo.FlowRate_SP > 0)
                                {
                                    countCycle = Convert.ToInt32(Math.Ceiling(d / PumpInfo.FlowRate_SP * 3600));
                                }
                                while (countCycle > 0)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        return;
                                    }

                                    if (deviceParameter.FeedSuspend)
                                    {
                                        break;
                                    }
                                    countCycle--;
                                    Thread.Sleep(1000);
                                    costCycleSeconds++;
                                }

                                while (deviceParameter.FeedSuspend)
                                {
                                    Thread.Sleep(1000);
                                }
                                float remainingVolumeCycle = PumpInfo.FlowRate_SP * countCycle / 3600f;
                                SetCommand(PumpInfo.FlowRate_SP, remainingVolumeCycle);
                                while (countCycle > 0)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        return;
                                    }
                                    countCycle--;
                                    Thread.Sleep(1000);
                                }

                                totalMinutes = costCycleSeconds / 60d;
                                double diff = f.A - totalMinutes;
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                if (diff > 0)
                                {
                                    int count12 = Convert.ToInt32(diff * 60);
                                    while (count12 > 0)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        count12--;
                                        Thread.Sleep(1000);
                                    }
                                }

                                int count2 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count2 > 0)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        return;
                                    }

                                    count2--;
                                    Thread.Sleep(1000);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(AppSession.Interval * 1000);
                            }
                        }
                        #endregion
                        break;
                    case FeedControlMode.Probe:
                        #region 脉冲探索法
                        var probeParam = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == CurrentDeviceParameter.Name);
                        _probingController.SetDevice(CurrentDeviceParameter);
                        _probingController.InitProbParam(probeParam);
                        _probingController.StartCtrl();
                        #endregion
                        break;
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;

                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = PumpInfo.PumpIndex,
                    Pump = PumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
                PumpInfo.IsControling = false;
            };
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopWork(DeviceParameter CurrentDeviceParameter = null)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                Thread.Sleep(100);
            }

            if (_probingController != null)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == CurrentDeviceParameter.Name);
                _probingController.SetDevice(CurrentDeviceParameter);
                _probingController.InitProbParam(param);
                _probingController.StopCtrl();
                Thread.Sleep(100);
            }
        }
    }
}
