using Fpi.Instruments;
using ImTools;
using Newtonsoft.Json;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XZ.SQLite;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace RD3.Controller
{
    public class DOController
    {

        private DateTime _tsStartTime;

        private Thread _tsThread;

        // 线程退出标志（必须用 volatile 修饰）
        private static volatile bool _shouldStop = false;

        private int _agitSP = -1;

        private float _agitDelta = 0f;

        private BackgroundWorker _backgroundWorker;

        private QPIDController _agitPIDController = new QPIDController();

        private QPIDController _airPIDController = new QPIDController();

        private QPIDController _o2PIDController = new QPIDController();

        private bool _workerWorking = false;

        private int _airIndex = -1;

        private int _o2Index = -1;

        private int _tempIndex = -1;

        private int _feedIndex = -1;

        public Fermentor CurrentFermentor
        {
            get { return AnalysisSolution.GetInstance().CurrentFermentor; }
        }

        public PumpInfo FeedPumpInfo
        {
            get => CurrentFermentor.Device.PumpInfoCol.FindFirst(t => t.Pump == PeristalticPump.FeedPump && t.IsEnable);
        }

        public MFCInfo MFCAir
        {
            get => CurrentFermentor.Device.MFCInfoCol.FindFirst(t => t.Gas == GasType.Air && t.IsEnable);
        }

        public MFCInfo MFCO2
        {
            get => CurrentFermentor.Device.MFCInfoCol.FindFirst(t => t.Gas == GasType.O2 && t.IsEnable);
        }

        public DOController()
        {
        }

        public void StartWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                return;
            }

            bool firstInitFeed = true;
            bool firstInitTemp = true;

            _agitPIDController.Reset();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;      // 允许报告进度
            _backgroundWorker.WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            _backgroundWorker.DoWork += ((s, e) =>
            {
                _workerWorking = true;

                CurrentFermentor.Device.FeedSuspend = false;
                CurrentFermentor.Device.DOParam.InitialTemp = CurrentFermentor.Device.DOParam.SP;
                e.Result = CurrentFermentor.Device.Name;

                float maxGas = 0;//用于通气量的总和
                float initialGas = 0;

                int factorIndex = -1;//当前执行索引
                int lastFactorIndex = -1;//当前执行索引
                int lastDODelta = 0;//低通滤波的上个值
                var baseAgit = -1;//转速底值
                PIDInfo info = null;
                PIDInfo lastPid = null;

                int sleepCount = 1;

                while (AppSession.DOPause)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        _workerWorking = false;
                        return;
                    }

                    Thread.Sleep(1000);
                }

                RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                while (realTimeParam.DO < CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsDirect)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        _workerWorking = false;
                        return;
                    }
                    while (AppSession.DOPause)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                }

                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                while (realTimeParam.DO > CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsReverse)
                {
                    if (_backgroundWorker.CancellationPending)
                    {
                        _workerWorking = false;
                        return;
                    }
                    while (AppSession.DOPause)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                }

                //mid-ranging控制
                if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                {
                    info = null;
                    lastPid = null;
                    baseAgit = -1;
                    lastDODelta = 0;//低通滤波的上个值
                    factorIndex = -1;//当前执行索引
                    lastFactorIndex = -1;//当前执行索引

                    MidRangingParam param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                    param.AgitLowerLimit = CurrentFermentor.Device.AgitParam.LowerLimit;
                    param.AgitUpperLimit = CurrentFermentor.Device.AgitParam.UpperLimit;
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                    CurrentFermentor.Device.AgitParam.SP = Math.Clamp(realTimeParam.Agit, CurrentFermentor.Device.AgitParam.LowerLimit, CurrentFermentor.Device.AgitParam.UpperLimit);
                    CurrentFermentor.Device.AgitParam.IsControling = true;

                    ObservableCollection<DOControlFactor> collection = [.. param.FactorCol];
                    if (collection.Contains(DOControlFactor.Air))
                    {
                        if (MFCAir != null)
                        {
                            MFCAir.IsControlled = true;
                        }
                    }
                    if (collection.Contains(DOControlFactor.O2))
                    {
                        if (MFCO2 != null)
                        {
                            MFCO2.IsControlled = true;
                        }
                    }
                    if (collection.Contains(DOControlFactor.Feed))
                    {
                        if (FeedPumpInfo != null)
                        {
                            FeedPumpInfo.IsControlled = true;
                        }
                    }

                    if (collection.Count > 0 && (collection[0] == DOControlFactor.Air || collection[0] == DOControlFactor.O2))
                    {
                        if (param.Unit == 0)//VVM
                        {
                            initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                            maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                        }
                        else if (param.Unit == 1)//L/min
                        {
                            initialGas = param.InitialAir;
                            maxGas = param.AirUpperLimit;
                        }

                        switch (collection[0])
                        {
                            case DOControlFactor.Air:
                                if (MFCAir != null)
                                {
                                    MFCAir.FlowRate_SP = Math.Clamp(realTimeParam.AirFlowSpeed, initialGas, maxGas);
                                    MFCAir.IsControling = true;
                                }
                                break;
                            case DOControlFactor.O2:
                                if (MFCO2 != null)
                                {
                                    MFCO2.FlowRate_SP = Math.Clamp(realTimeParam.O2FlowSpeed, initialGas, maxGas);
                                    MFCO2.IsControling = true;
                                }
                                break;
                        }
                    }

                    sleepCount = 5;
                    while (sleepCount > 0)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }
                        while (AppSession.DOPause)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }
                            Thread.Sleep(1000);
                        }
                        sleepCount--;
                        Thread.Sleep(1000);
                    }

                    ResetDOParam(CurrentFermentor.Device);
                    while (true)
                    {
                        try
                        {
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            while (realTimeParam.DO < CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsDirect)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            while (realTimeParam.DO > CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsReverse)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            }

                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }

                                Thread.Sleep(1000);
                            }
                            var pIDInfos = PIDInfoManager.GetInstance().PIDInfos;
                            if (pIDInfos == null)
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                if (realTimeParam.DO <= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = new PIDInfo() { Factor = PIDFactor.DO_Dircet, P = 5, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                }
                                else if (realTimeParam.DO >= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = new PIDInfo() { Factor = PIDFactor.DO_Reverse, P = 5, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                }
                            }
                            else
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                if (realTimeParam.DO <= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == CurrentFermentor.Device.Name);
                                }
                                else if (realTimeParam.DO >= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == CurrentFermentor.Device.Name);
                                }
                            }

                            if (baseAgit == -1)
                            {
                                baseAgit = realTimeParam.Agit;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, CurrentFermentor.Device.Name));
                                    baseAgit = CurrentFermentor.Device.AgitParam.SP;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", CurrentFermentor.Device.Name, realTimeParam.Agit, CurrentFermentor.Device.AgitParam.SP, baseAgit));

                                ResetDOParam(CurrentFermentor.Device);
                            }
                            lastPid = info;

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);

                            if (Math.Abs(realTimeParam.DO - CurrentFermentor.Device.DOParam.SP) <= info.deadArea)
                            {
                                baseAgit = CurrentFermentor.Device.AgitParam.SP;
                                ResetDOParam(CurrentFermentor.Device);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            _agitPIDController.SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            _agitPIDController.SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            _agitPIDController.SetIntegralLimits(-2000, 2000);
                            _agitPIDController.SetTarget(CurrentFermentor.Device.DOParam.SP);

                            LogHelper.Debug(string.Format("反应器{6} Mid-Ranging DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", CurrentFermentor.Device.DOParam.SP, realTimeParam.DO, info.P, info.I, info.D, info.Interval, CurrentFermentor.Device.Name));

                            float temp = _agitPIDController.CalculatePositional_DO((float)realTimeParam.DO);
                            int tempAgit = Convert.ToInt32(baseAgit + temp);

                            _agitDelta = tempAgit;
                            if (CurrentFermentor.Device.DOFilterEnable)
                            {
                                //增加低通滤波 
                                var lowPassDelta = Convert.ToInt32(RCFilter.LowPass(tempAgit, lastDODelta, CurrentFermentor.Device.AgitSampleCycle, CurrentFermentor.Device.AgitSampleFrequency));
                                lastDODelta = lowPassDelta;
                                _agitDelta = lowPassDelta;
                            }

                            LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 转速底值：{1}，Delta：{2},原始值{3}，滤波值{4}", CurrentFermentor.Device.Name, baseAgit, temp, tempAgit, _agitDelta));

                            CurrentFermentor.Device.AgitParam.SP = Math.Clamp((int)_agitDelta,CurrentFermentor.Device.AgitParam.LowerLimit, CurrentFermentor.Device.AgitParam.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentFermentor.Device.Name, CurrentFermentor.Device.AgitParam.SP);

                            sleepCount = info.Interval <= 0 ? 1 : info.Interval;
                            while (sleepCount > 0)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                sleepCount--;
                                Thread.Sleep(1000);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            if (Math.Abs(realTimeParam.DO - CurrentFermentor.Device.DOParam.SP) <= info.deadArea)
                            {
                                baseAgit = CurrentFermentor.Device.AgitParam.SP;

                                ResetDOParam(CurrentFermentor.Device);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            if (CurrentFermentor.Device.AgitParam.SP > param.AgitHigh && factorIndex == -1)
                            {
                                LogHelper.Debug($"到达设定转速高限:{param.AgitHigh}");
                                lastFactorIndex = factorIndex;
                                factorIndex = 0;
                            }

                            if (factorIndex < 0 || collection.Count <= factorIndex)//如果未达到高限或者没有其他执行参数，则一直循环
                            {
                                continue;
                            }

                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                            PIDInfo info1 = null;
                            var previousElements = collection.Take(factorIndex);
                            bool isExistOtherGas = false;//在当前气体之前是否存在气体

                            switch (collection[factorIndex])
                            {
                                case DOControlFactor.Air:
                                    if (MFCAir == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("通气MFC不存在。", "温馨提示");
                                        return;
                                    }
                                    if ( !MFCAir.IsControling)
                                    {
                                        MFCAir.IsControling = true;
                                    }
                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.InitialAir;
                                        maxGas = param.AirUpperLimit;
                                    }

                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("通气") && t.deviceID == CurrentFermentor.Device.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }

                                    _airPIDController.Reset();
                                    _airPIDController.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    _airPIDController.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    _airPIDController.SetIntegralLimits(-2000, 2000);
                                    _airPIDController.SetTarget(_agitDelta);
                                    float tempAir = _airPIDController.CalculateIncremental(param.AgitHigh);
                                    float airSpeed = MFCAir.FlowRate_SP + tempAir;
                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.O2).Count() > 0;
                                    float minAir = isExistOtherGas == true ? 0 : initialGas;
                                    if (airSpeed >= maxGas)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (airSpeed <= minAir)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    airSpeed = airSpeed >= maxGas ? maxGas : airSpeed < minAir ? minAir : MathF.Round(airSpeed, 2);
                                    MFCAir.FlowRate_SP = airSpeed;
                                    if (isExistOtherGas)
                                    {
                                        MFCO2.IsControling = true;
                                        MFCO2.FlowRate_SP = MathF.Round(maxGas - airSpeed, 2);
                                    }

                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 通气预设值：{1}，当前：{2}，delta：{3}", CurrentFermentor.Device.Name, airSpeed, MFCAir.FlowRate_SP, tempAir));
                                    sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.O2:
                                    if (MFCO2 == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("氧气MFC不存在。", "温馨提示");
                                        return;
                                    }
                                    if (!MFCO2.IsControling)
                                    {
                                        MFCO2.IsControling = true;
                                    }
                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.InitialAir;
                                        maxGas = param.AirUpperLimit;
                                    }

                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("氧气") && t.deviceID == CurrentFermentor.Device.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    _o2PIDController.Reset();
                                    _o2PIDController.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    _o2PIDController.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    _o2PIDController.SetIntegralLimits(-2000, 2000);
                                    _o2PIDController.SetTarget(_agitDelta);
                                    float tempO2 = _o2PIDController.CalculateIncremental((float)param.AgitHigh);
                                    float o2Speed = MFCO2.FlowRate_SP + tempO2;
                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.Air).Count() > 0;
                                    float minO2 = isExistOtherGas == true ? 0 : initialGas;
                                    if (o2Speed >= maxGas)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (o2Speed <= minO2)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    o2Speed = o2Speed >= maxGas ? maxGas : o2Speed < minO2 ? minO2 : MathF.Round(o2Speed, 2);
                                    MFCO2.FlowRate_SP = o2Speed;
                                    if (isExistOtherGas)
                                    {

                                        MFCAir.IsControling = true;
                                        MFCAir.FlowRate_SP = maxGas - o2Speed > 0 ? MathF.Round(maxGas - o2Speed, 2) : 0;
                                    }
                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 氧气预设值：{1}，底值：{2}，delta：{3}", CurrentFermentor.Device.Name, o2Speed, MFCO2.FlowRate_SP, tempO2));
                                    sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.Temp:
                                    
                                    if (!CurrentFermentor.Device.DOParam.IsControling)
                                    {
                                        AnalysisSolution.GetInstance().CurrentFermentor.TempController.StartWork();
                                        Thread.Sleep(1000);
                                    }
                                    if (firstInitTemp)
                                    {
                                        CurrentFermentor.Device.DOParam.InitialTemp = CurrentFermentor.Device.DOParam.SP;
                                        firstInitTemp = false;
                                    }

                                    QPIDController pIDController = new QPIDController();
                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("降温") && t.deviceID == CurrentFermentor.Device.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    pIDController.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    pIDController.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    pIDController.SetIntegralLimits(-2000, 2000);
                                    pIDController.SetTarget(param.AgitHigh);
                                    float increment = pIDController.CalculateIncremental(_agitDelta);
                                    float currentTemp = CurrentFermentor.Device.DOParam.SP + increment;
                                    if (currentTemp <= CurrentFermentor.Device.TempDOLowerLimit)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (currentTemp >= CurrentFermentor.Device.DOParam.InitialTemp)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    currentTemp = currentTemp <= CurrentFermentor.Device.TempDOLowerLimit ? CurrentFermentor.Device.TempDOLowerLimit : currentTemp >= CurrentFermentor.Device.DOParam.InitialTemp ? CurrentFermentor.Device.DOParam.InitialTemp : currentTemp;
                                    CurrentFermentor.Device.DOParam.SP = MathF.Round(currentTemp, 2);
                                    LogHelper.Debug(string.Format("反应器{0} 起始溶氧{1} 单次delta{2} 实际溶氧{3}", CurrentFermentor.Device.Name, CurrentFermentor.Device.DOParam.InitialTemp, increment, currentTemp));
                                    sleepCount = info1.Interval <= 0 ? 1 : info1.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }

                                    if (CurrentFermentor.Device.DOParam.SP <= CurrentFermentor.Device.TempDOLowerLimit || CurrentFermentor.Device.DOParam.SP >= CurrentFermentor.Device.DOParam.InitialTemp)
                                    {
                                        while (true)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            while (AppSession.DOPause)
                                            {
                                                if (_backgroundWorker.CancellationPending)
                                                {
                                                    _workerWorking = false;
                                                    return;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                            if (Math.Abs(realTimeParam.Temp - CurrentFermentor.Device.DOParam.SP) <= 0.2)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    break;
                                case DOControlFactor.Feed:
                                    if (FeedPumpInfo == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("补料泵不存在。", "温馨提示");
                                        return;
                                    }

                                    if (!FeedPumpInfo.IsControling)
                                    {
                                        if (factorIndex < collection.Count - 1 && lastFactorIndex <= factorIndex)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                        else if (factorIndex - 1 > -1 && lastFactorIndex >= factorIndex)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    if (firstInitFeed)
                                    {
                                        CurrentFermentor.Device.FeedSuspend = true;
                                        CurrentFermentor.Device.DOParam.InitialFeed = FeedPumpInfo.FlowRate_SP;
                                        firstInitFeed = false;
                                    }
                                    QPIDController controller = new QPIDController();
                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("补料") && t.deviceID == CurrentFermentor.Device.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    controller.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    controller.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    controller.SetIntegralLimits(-2000, 2000);
                                    controller.SetTarget(param.AgitHigh);
                                    float incrementFeed = controller.CalculateIncremental(_agitDelta);
                                    float currentFeed = FeedPumpInfo.FlowRate_SP + incrementFeed;
                                    if (currentFeed <= CurrentFermentor.Device.FeedDOLowerLimit)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (currentFeed >= CurrentFermentor.Device.DOParam.InitialFeed)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    currentFeed = currentFeed <= CurrentFermentor.Device.FeedDOLowerLimit ? CurrentFermentor.Device.FeedDOLowerLimit : currentFeed >= CurrentFermentor.Device.DOParam.InitialFeed ? CurrentFermentor.Device.DOParam.InitialFeed : currentFeed;
                                    FeedPumpInfo.FlowRate_SP = Math.Clamp(MathF.Round(currentFeed, 2), 0, Const.MaxPumpFlowRate);
                                    var controlParam = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = FeedPumpInfo.PumpIndex,
                                        Pump = FeedPumpInfo.Pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = FeedPumpInfo.FlowRate_SP,
                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentFermentor.Device.Name, controlParam);
                                    LogHelper.Debug(string.Format("反应器{0} 起始补料{1} 单次delta{2} 实际补料{3}", CurrentFermentor.Device.Name, CurrentFermentor.Device.DOParam.InitialFeed, incrementFeed, currentFeed));
                                    sleepCount = info1.Interval <= 0 ? 1 : info1.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            HandyControl.Controls.MessageBox.Show(string.Format("DO调整失败_Mid-Ranging，错误信息：{0}", ex.Message));
                            return;
                        }
                    }
                }
                //周期
                else if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Cycle)
                {
                    CurrentFermentor.Device.AgitParam.IsControling = true;
                    while (true)
                    {
                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                        while (realTimeParam.DO < CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsDirect)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }
                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                        }

                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                        while (realTimeParam.DO > CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsReverse)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }
                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                        }

                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }

                        while (AppSession.DOPause)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            Thread.Sleep(1000);
                        }

                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                        if (realTimeParam.DO < CurrentFermentor.Device.DOParam.SP)
                        {
                            int temp = CurrentFermentor.Device.AgitParam.SP + CurrentFermentor.Device.DOParam.AgitCycle.DirectStep;
                            CurrentFermentor.Device.AgitParam.SP = Math.Clamp(temp, CurrentFermentor.Device.DOParam.AgitCycle.LowerLimit, CurrentFermentor.Device.DOParam.AgitCycle.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentFermentor.Device.Name, CurrentFermentor.Device.AgitParam.SP);

                            int count = CurrentFermentor.Device.DOParam.AgitCycle.DirectInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }

                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                index += 1;
                                Thread.Sleep(1000);
                            }
                        }
                        else if (realTimeParam.DO > CurrentFermentor.Device.DOParam.SP)
                        {
                            CurrentFermentor.Device.AgitParam.SP -= CurrentFermentor.Device.DOParam.AgitCycle.ReverseStep;

                            if (CurrentFermentor.Device.AgitParam.SP < CurrentFermentor.Device.DOParam.AgitCycle.LowerLimit)
                            {
                                CurrentFermentor.Device.AgitParam.SP = CurrentFermentor.Device.DOParam.AgitCycle.LowerLimit;
                            }
                            else if (CurrentFermentor.Device.AgitParam.SP > CurrentFermentor.Device.DOParam.AgitCycle.UpperLimit)
                            {
                                CurrentFermentor.Device.AgitParam.SP = CurrentFermentor.Device.DOParam.AgitCycle.UpperLimit;
                            }
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentFermentor.Device.Name, CurrentFermentor.Device.AgitParam.SP);

                            int count = CurrentFermentor.Device.DOParam.AgitCycle.ReverseInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                index += 1;
                                Thread.Sleep(1000);
                            }
                        }

                    }

                }
                //级联通气控制
                else if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Step)
                {
                    Stopwatch gasRetentionTime = new Stopwatch();

                    e.Result = CurrentFermentor.Device.Name;
                    DateTime startTime = DateTime.Now;
                    _feedIndex = _tempIndex = _airIndex = _o2Index = 0;

                    info = null;
                    lastPid = null;
                    baseAgit = -1;
                    lastDODelta = 0;//低通滤波的上个值
                    factorIndex = 0;//当前执行索引
                    lastFactorIndex = -1;//当前执行索引

                    CurrentFermentor.Device.AgitParam.IsControling = true;

                    DOAssParam param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                    param.AgitLowerLimit = CurrentFermentor.Device.AgitParam.LowerLimit;
                    param.AgitUpperLimit = CurrentFermentor.Device.AgitParam.UpperLimit;

                    ObservableCollection<DOControlFactor> collection = [.. param.FactorCol];

                    if (collection.Contains(DOControlFactor.Air))
                    {
                        if (MFCAir != null)
                        {
                            MFCAir.IsControlled = true;
                        }
                    }
                    if (collection.Contains(DOControlFactor.O2))
                    {
                        if (MFCO2 != null)
                        {
                            MFCO2.IsControlled = true;
                        }
                    }
                    if (collection.Contains(DOControlFactor.Feed))
                    {
                        if (FeedPumpInfo != null)
                        {
                            FeedPumpInfo.IsControlled = true;
                        }
                    }

                    if (collection.Count > 0)
                    {
                        if (collection.Contains(DOControlFactor.Air))
                        {
                            if (param.Unit == 0)//VVM
                            {
                                _airIndex = param.CascadeCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || MathF.Round(param.CascadeCol[x.Index - 1].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000, 2) <= realTimeParam.AirFlowSpeed) &&
                                 (x.Index == param.CascadeCol.Count - 1 || MathF.Round(param.CascadeCol[x.Index + 1].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000, 2) >= realTimeParam.AirFlowSpeed))?.Index ?? -1;
                            }
                            else if (param.Unit == 1)//L/min
                            {
                                _airIndex = param.CascadeCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || param.CascadeCol[x.Index - 1].AirFlowRate <= realTimeParam.AirFlowSpeed) &&
                                 (x.Index == param.CascadeCol.Count - 1 || param.CascadeCol[x.Index + 1].AirFlowRate >= realTimeParam.AirFlowSpeed))?.Index ?? -1;
                            }

                            LogHelper.Debug(string.Format("阶梯级联：通气档位为{0}", _airIndex + 1));
                            if (MFCAir != null)
                            {
                                MFCAir.IsControling = true;
                                gasRetentionTime.Start();
                            }
                        }

                        if (collection.Contains(DOControlFactor.O2))
                        {
                            if (param.Unit == 0)//VVM
                            {
                                _o2Index = param.CascadeCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || MathF.Round(param.CascadeCol[x.Index - 1].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000, 2) <= realTimeParam.O2FlowSpeed) &&
                                 (x.Index == param.CascadeCol.Count - 1 || MathF.Round(param.CascadeCol[x.Index + 1].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000, 2) >= realTimeParam.O2FlowSpeed))?.Index ?? -1;
                            }
                            else if (param.Unit == 1)//L/min
                            {
                                _o2Index = param.CascadeCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || param.CascadeCol[x.Index - 1].O2FlowRate <= realTimeParam.O2FlowSpeed) &&
                                 (x.Index == param.CascadeCol.Count - 1 || param.CascadeCol[x.Index + 1].O2FlowRate >= realTimeParam.O2FlowSpeed))?.Index ?? -1;
                            }

                            LogHelper.Debug(string.Format("阶梯级联：氧气档位为{0}", _o2Index + 1));
                            if (MFCO2 != null)
                            {
                                MFCO2.IsControling = true;
                                gasRetentionTime.Start();
                            }
                        }
                    }

                    sleepCount = 5;
                    while (sleepCount > 0)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }
                        while (AppSession.DOPause)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }
                            Thread.Sleep(1000);
                        }
                        sleepCount--;
                        Thread.Sleep(1000);
                    }

                    ResetDOParam(CurrentFermentor.Device);

                    while (true)
                    {
                        try
                        {
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            while (realTimeParam.DO < CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsDirect)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            while (realTimeParam.DO > CurrentFermentor.Device.DOParam.SP && !CurrentFermentor.Device.DOParam.IsReverse)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            }

                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }

                                Thread.Sleep(1000);
                            }

                            var pIDInfos = PIDInfoManager.GetInstance().PIDInfos;
                            if (pIDInfos == null)
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                if (realTimeParam.DO <= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = new PIDInfo() { Factor = PIDFactor.DO_Dircet, P = 5, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                }
                                else if (realTimeParam.DO >= CurrentFermentor.Device.DOParam.SP)
                                {
                                    info = new PIDInfo() { Factor = PIDFactor.DO_Reverse, P = 5, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                }
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            if (realTimeParam.DO <= CurrentFermentor.Device.DOParam.SP)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == CurrentFermentor.Device.Name);
                            }
                            else if (realTimeParam.DO >= CurrentFermentor.Device.DOParam.SP)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == CurrentFermentor.Device.Name);
                            }

                            if (baseAgit == -1)
                            {
                                baseAgit = realTimeParam.Agit;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, CurrentFermentor.Device.Name));
                                    baseAgit = CurrentFermentor.Device.AgitParam.SP;
                                }

                                ResetDOParam(CurrentFermentor.Device);
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", CurrentFermentor.Device.Name, realTimeParam.Agit, CurrentFermentor.Device.AgitParam.SP, baseAgit));
                            }
                            lastPid = info;

                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            if (Math.Abs(realTimeParam.DO - CurrentFermentor.Device.DOParam.SP) <= info.deadArea)
                            {
                                baseAgit = CurrentFermentor.Device.AgitParam.SP;
                                ResetDOParam(CurrentFermentor.Device);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            _agitPIDController.SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            _agitPIDController.SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            _agitPIDController.SetIntegralLimits(-2000, 2000);
                            _agitPIDController.SetTarget(CurrentFermentor.Device.DOParam.SP);

                            LogHelper.Debug(string.Format("反应器{6} 阶梯级联 DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", CurrentFermentor.Device.DOParam.SP, realTimeParam.DO, info.P, info.I, info.D, info.Interval, CurrentFermentor.Device.Name));

                            float temp = _agitPIDController.CalculatePositional_DO((float)realTimeParam.DO);
                            float timeOffset = Convert.ToSingle((DateTime.Now - startTime).TotalMinutes);
                            temp = 1 * temp;//系数都默认为1
                            int tempAgit = Convert.ToInt32(baseAgit + temp);
                            _agitDelta = tempAgit;
                            if (CurrentFermentor.Device.DOFilterEnable)
                            {
                                //增加低通滤波 
                                var lowPassDelta = Convert.ToInt32(RCFilter.LowPass(tempAgit, lastDODelta, CurrentFermentor.Device.AgitSampleCycle, CurrentFermentor.Device.AgitSampleFrequency));
                                lastDODelta = lowPassDelta;
                                _agitDelta = lowPassDelta;
                            }
                            LogHelper.Debug(string.Format("反应器{0} 阶梯级联 转速底值：{1}，Delta：{2},原始值{3}，滤波值{4}", CurrentFermentor.Device.Name, baseAgit, temp, tempAgit, _agitDelta));

                            CurrentFermentor.Device.AgitParam.SP = (int)Math.Clamp(_agitDelta, CurrentFermentor.Device.AgitParam.LowerLimit, CurrentFermentor.Device.AgitParam.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentFermentor.Device.Name, CurrentFermentor.Device.AgitParam.SP);

                            sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                            while (sleepCount > 0)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                sleepCount--;
                                Thread.Sleep(1000);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                            if (Math.Abs(realTimeParam.DO - CurrentFermentor.Device.DOParam.SP) <= info.deadArea)
                            {
                                baseAgit = CurrentFermentor.Device.AgitParam.SP;

                                ResetDOParam(CurrentFermentor.Device);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            if (factorIndex < 0 || collection.Count <= factorIndex)
                            {
                                continue;
                            }

                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                            var previousElements = collection.Take(factorIndex);
                            bool isExistOtherGas = false;//在当前气体之前是否存在气体
                            switch (collection[factorIndex])
                            {
                                case DOControlFactor.Air:
                                    if (MFCAir == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("通气MFC不存在。", "温馨提示");
                                        return;
                                    }
                                    if (!MFCAir.IsControling)
                                    {
                                        MFCAir.IsControling = true;
                                    }

                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.CascadeCol[0].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.CascadeCol[param.CascadeCol.Count - 1].AirFlowRate * (CurrentFermentor.Device.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.CascadeCol[0].AirFlowRate;
                                        maxGas = param.CascadeCol[param.CascadeCol.Count - 1].AirFlowRate;
                                    }

                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.O2).Count() > 0;

                                    if (_agitDelta <= param.AgitLowerLimit)
                                    {
                                        if (gasRetentionTime.IsRunning)
                                        {
                                            gasRetentionTime.Stop();
                                            if (gasRetentionTime.Elapsed.TotalMinutes >= param.GasWaitTime)
                                            {
                                                #region 气体需要在当前阶段保持一段时间
                                                if (_airIndex > 0)//还存在上一阶梯
                                                {
                                                    _airIndex -= 1;

                                                    float airFlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                                    }
                                                    MFCAir.FlowRate_SP = airFlowSpeed;

                                                    if (isExistOtherGas)
                                                    {
                                                        if (_o2Index < param.CascadeCol.Count - 1)//加一档
                                                        {
                                                            _o2Index += 1;

                                                            float o2FlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                                            }
                                                            MFCO2.FlowRate_SP = o2FlowSpeed;
                                                            MFCO2.IsControling = true;
                                                        }
                                                    }
                                                }
                                                else if (factorIndex > 0)//非第一因子,所以不需要最低通气
                                                {
                                                    if (isExistOtherGas)
                                                    {
                                                        MFCAir.FlowRate_SP = 0;

                                                        if (_o2Index < param.CascadeCol.Count - 1)//加一档
                                                        {
                                                            _o2Index += 1;

                                                            float o2FlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                                            }
                                                            MFCO2.FlowRate_SP = o2FlowSpeed;
                                                            MFCO2.IsControling = true;
                                                        }
                                                    }

                                                    lastFactorIndex = factorIndex;
                                                    factorIndex -= 1;
                                                }
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            gasRetentionTime.Start();
                                        }
                                        
                                    }
                                    else if (_agitDelta >= param.AgitUpperLimit)
                                    {
                                        if (gasRetentionTime.IsRunning)
                                        {
                                            gasRetentionTime.Stop();
                                            if (gasRetentionTime.Elapsed.TotalMinutes >= param.GasWaitTime)
                                            {
                                                #region 气体需要在当前阶段保持一段时间
                                                if (_airIndex < param.CascadeCol.Count - 1)//还存在下一阶梯
                                                {
                                                    _airIndex += 1;

                                                    float airFlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                                    }
                                                    MFCAir.FlowRate_SP = airFlowSpeed;

                                                    if (isExistOtherGas)
                                                    {
                                                        if (_o2Index > 0)
                                                        {
                                                            _o2Index -= 1;

                                                            float o2FlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                                            }
                                                            MFCO2.FlowRate_SP = o2FlowSpeed;
                                                            MFCO2.IsControling = true;
                                                        }
                                                        else
                                                        {
                                                            MFCO2.FlowRate_SP = 0;
                                                            MFCO2.IsControling = true;
                                                        }
                                                    }
                                                }
                                                else if (factorIndex < collection.Count - 1)
                                                {
                                                    lastFactorIndex = factorIndex;
                                                    factorIndex += 1;
                                                }
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            gasRetentionTime.Start();
                                        }
                                        
                                    }
                                    else
                                    {
                                        float airFlowSpeed = 0f;
                                        if (param.Unit == 0)//VVM
                                        {
                                            airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        }
                                        else if (param.Unit == 1)//L/min
                                        {
                                            airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                        }
                                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                        if (Math.Abs(realTimeParam.AirFlowSpeed - airFlowSpeed) > 0.05)
                                        {
                                            MFCAir.FlowRate_SP = airFlowSpeed;
                                            if (!gasRetentionTime.IsRunning)
                                            {
                                                gasRetentionTime.Start();
                                            }
                                        }

                                        if (isExistOtherGas)
                                        {
                                            float o2FlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                            if (Math.Abs(realTimeParam.O2FlowSpeed - o2FlowSpeed) > 0.05)
                                            {
                                                MFCO2.FlowRate_SP = o2FlowSpeed;
                                            }
                                        }
                                    }

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.O2:
                                    if (MFCO2 == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("氧气MFC不存在。", "温馨提示");
                                        return;
                                    }
                                    if (!MFCO2.IsControling)
                                    {
                                        MFCO2.IsControling = true;
                                    }

                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.CascadeCol[0].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.CascadeCol[param.CascadeCol.Count - 1].O2FlowRate * (CurrentFermentor.Device.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.CascadeCol[0].O2FlowRate;
                                        maxGas = param.CascadeCol[param.CascadeCol.Count - 1].O2FlowRate;
                                    }

                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.Air).Count() > 0;

                                    if (_agitDelta <= param.AgitLowerLimit)
                                    {
                                        if (gasRetentionTime.IsRunning)
                                        {
                                            gasRetentionTime.Stop();
                                            if (gasRetentionTime.Elapsed.TotalMinutes >= param.GasWaitTime)
                                            {
                                                #region 气体需要在当前阶段保持一段时间
                                                if (_o2Index > 0)//还存在上一阶梯
                                                {
                                                    _o2Index -= 1;

                                                    float o2FlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                                    }
                                                    MFCO2.FlowRate_SP = o2FlowSpeed;

                                                    if (isExistOtherGas)
                                                    {
                                                        if (_airIndex < param.CascadeCol.Count - 1)//加一档
                                                        {
                                                            _airIndex += 1;

                                                            float airFlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                                            }
                                                            MFCAir.FlowRate_SP = airFlowSpeed;
                                                            MFCAir.IsControling = true;
                                                        }
                                                    }
                                                }
                                                else if (factorIndex > 0)//非第一因子，不需要最低通气量
                                                {
                                                    if (isExistOtherGas)
                                                    {
                                                        MFCO2.FlowRate_SP = 0;

                                                        if (_airIndex < param.CascadeCol.Count - 1)//加一档
                                                        {
                                                            _airIndex += 1;

                                                            float airFlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                                            }
                                                            MFCAir.FlowRate_SP = airFlowSpeed;
                                                            MFCAir.IsControling = true;
                                                        }
                                                    }

                                                    lastFactorIndex = factorIndex;
                                                    factorIndex -= 1;
                                                }
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            gasRetentionTime.Start();
                                        }
                                    }
                                    else if (_agitDelta >= param.AgitUpperLimit)
                                    {
                                        if (gasRetentionTime.IsRunning)
                                        {
                                            gasRetentionTime.Stop();
                                            if (gasRetentionTime.Elapsed.TotalMinutes >= param.GasWaitTime)
                                            {
                                                #region 气体需要在当前阶段保持一段时间
                                                if (_o2Index < param.CascadeCol.Count - 1)//还存在下一阶梯
                                                {
                                                    _o2Index += 1;

                                                    float o2FlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                                    }
                                                    MFCO2.FlowRate_SP = o2FlowSpeed;

                                                    if (isExistOtherGas)
                                                    {
                                                        if (_airIndex > 0)
                                                        {
                                                            _airIndex -= 1;

                                                            float airFlowSpeed = 0f;
                                                            if (param.Unit == 0)//VVM
                                                            {
                                                                airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                                            }
                                                            else if (param.Unit == 1)//L/min
                                                            {
                                                                airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                                            }
                                                            MFCAir.FlowRate_SP = airFlowSpeed;
                                                            MFCAir.IsControling = true;
                                                        }
                                                        else
                                                        {
                                                            MFCAir.FlowRate_SP = 0;
                                                            MFCAir.IsControling = true;
                                                        }
                                                    }
                                                }
                                                else if (factorIndex < collection.Count - 1)
                                                {
                                                    lastFactorIndex = factorIndex;
                                                    factorIndex += 1;
                                                }
                                                #endregion
                                            }
                                        }
                                        else
                                        {
                                            gasRetentionTime.Start();
                                        }
                                    }
                                    else
                                    {
                                        float o2FlowSpeed = 0f;
                                        if (param.Unit == 0)//VVM
                                        {
                                            o2FlowSpeed = MathF.Round((float)(param.CascadeCol[_o2Index].O2FlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                        }
                                        else if (param.Unit == 1)//L/min
                                        {
                                            o2FlowSpeed = param.CascadeCol[_o2Index].O2FlowRate;
                                        }
                                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                        if (Math.Abs(realTimeParam.O2FlowSpeed - o2FlowSpeed) > 0.05)
                                        {
                                            MFCO2.FlowRate_SP = o2FlowSpeed;
                                            if (!gasRetentionTime.IsRunning)
                                            {
                                                gasRetentionTime.Start();
                                            }
                                        }

                                        if (isExistOtherGas)
                                        {
                                            float airFlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                airFlowSpeed = MathF.Round((float)(param.CascadeCol[_airIndex].AirFlowRate * (realTimeParam.JarWeight - InstrumentSolution.GetInstance().ReactorWeight) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                airFlowSpeed = param.CascadeCol[_airIndex].AirFlowRate;
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                            if (Math.Abs(realTimeParam.AirFlowSpeed - airFlowSpeed) > 0.1)
                                            {
                                                MFCAir.FlowRate_SP = airFlowSpeed;
                                            }
                                        }
                                    }

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.Temp:
                                    if (!CurrentFermentor.Device.DOParam.IsControling)
                                    {
                                        AnalysisSolution.GetInstance().CurrentFermentor.TempController.StartWork();
                                        Thread.Sleep(1000);
                                    }

                                    if (_agitDelta <= param.AgitLowerLimit)
                                    {
                                        if (_tempIndex > 0)//还存在上一阶梯
                                        {
                                            _tempIndex -= 1;

                                            CurrentFermentor.Device.DOParam.SP = param.CascadeCol[_tempIndex].Temp;
                                        }
                                        else if (factorIndex > 0)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;

                                            CurrentFermentor.Device.DOParam.SP = CurrentFermentor.Device.DOParam.InitialTemp;
                                        }
                                    }
                                    else if (_agitDelta >= param.AgitUpperLimit)
                                    {
                                        if (_tempIndex < param.CascadeCol.Count - 1)//还存在下一阶梯
                                        {
                                            _tempIndex += 1;

                                            CurrentFermentor.Device.DOParam.SP = param.CascadeCol[_tempIndex].Temp;
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;

                                            CurrentFermentor.Device.DOParam.SP = CurrentFermentor.Device.DOParam.InitialTemp;
                                        }
                                    }
                                    else
                                    {
                                        CurrentFermentor.Device.DOParam.SP = param.CascadeCol[_tempIndex].Temp;
                                    }

                                    if (_tempIndex <= 0 || _tempIndex >= param.CascadeCol.Count - 1)
                                    {
                                        while (true)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            while (AppSession.DOPause)
                                            {
                                                if (_backgroundWorker.CancellationPending)
                                                {
                                                    _workerWorking = false;
                                                    return;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(CurrentFermentor.Device.Name);
                                            if (Math.Abs(realTimeParam.Temp - CurrentFermentor.Device.DOParam.SP) <= 0.2)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    break;
                                case DOControlFactor.Feed:
                                    if (FeedPumpInfo == null)
                                    {
                                        HandyControl.Controls.MessageBox.Show("补料泵不存在。", "温馨提示");
                                        return;
                                    }

                                    if (!FeedPumpInfo.IsControling)
                                    {
                                        if (factorIndex < collection.Count - 1 && lastFactorIndex <= factorIndex)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                        else if (factorIndex - 1 > -1 && lastFactorIndex >= factorIndex)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    if (firstInitFeed)
                                    {
                                        CurrentFermentor.Device.FeedSuspend = true;
                                        CurrentFermentor.Device.DOParam.InitialFeed = FeedPumpInfo.FlowRate_SP;
                                        firstInitFeed = false;
                                    }

                                    if (_agitDelta <= param.AgitLowerLimit)
                                    {
                                        if (_feedIndex > 0)//还存在上一阶梯
                                        {
                                            _feedIndex -= 1;

                                            float coeff = param.CascadeCol[_feedIndex].FeedFlowRate;
                                            FeedPumpInfo.FlowRate_SP = MathF.Round(CurrentFermentor.Device.DOParam.InitialFeed * coeff / 100, 2);
                                        }
                                        else if (factorIndex > 0)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;

                                            FeedPumpInfo.FlowRate_SP = CurrentFermentor.Device.DOParam.InitialFeed;
                                        }
                                    }
                                    else if (_agitDelta >= param.AgitUpperLimit)
                                    {
                                        if (_feedIndex < param.CascadeCol.Count - 1)//还存在下一阶梯
                                        {
                                            _feedIndex += 1;

                                            float coeff = param.CascadeCol[_feedIndex].FeedFlowRate;
                                            FeedPumpInfo.FlowRate_SP = MathF.Round(CurrentFermentor.Device.DOParam.InitialFeed * coeff / 100, 2);
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;

                                            FeedPumpInfo.FlowRate_SP = CurrentFermentor.Device.DOParam.InitialFeed;
                                        }
                                    }
                                    else
                                    {
                                        float coeff = param.CascadeCol[_feedIndex].FeedFlowRate;
                                        FeedPumpInfo.FlowRate_SP = MathF.Round(CurrentFermentor.Device.DOParam.InitialFeed * coeff / 100, 2);
                                    }

                                    FeedPumpInfo.FlowRate_SP = Math.Clamp(FeedPumpInfo.FlowRate_SP, 0, Const.MaxPumpFlowRate);
                                    var controlParam = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = FeedPumpInfo.PumpIndex,
                                        Pump = FeedPumpInfo.Pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = FeedPumpInfo.FlowRate_SP,
                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentFermentor.Device.Name, controlParam);
                                    LogHelper.Debug(string.Format("反应器{0} 起始补料{1} 实际补料{2}", CurrentFermentor.Device.Name, CurrentFermentor.Device.DOParam.InitialFeed, FeedPumpInfo.FlowRate_SP));

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败：阶梯级联：错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
            });
            _backgroundWorker.RunWorkerCompleted += ((s, e) =>
            {
                try
                {
                    var fermentor = AnalysisSolution.GetInstance().FermentorCol.FindFirst(t => t.Device.Name == e.Result?.ToString());
                    if (CurrentFermentor.Device == null)
                    {
                        LogHelper.Debug(string.Format("溶氧控制：事件完成出错,未找到反应器{0}" + e.Result?.ToString()));
                        return;
                    }
                    CurrentFermentor.Device.DOParam.IsControling = false;
                    CurrentFermentor.Device.FeedSuspend = false;

                    if (MFCAir != null)
                    {
                        MFCAir.IsControlled = false;
                    }
                    if (MFCO2 != null)
                    {
                        MFCO2.IsControlled = false;
                    }
                    if (FeedPumpInfo != null)
                    {
                        FeedPumpInfo.IsControlled = false;
                        if (FeedPumpInfo.IsControling)
                        {
                            if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                            {
                                var param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                                if (param.FactorCol.Contains(DOControlFactor.Temp))
                                {
                                    FeedPumpInfo.FlowRate_SP = CurrentFermentor.Device.DOParam.InitialFeed;
                                }
                            }
                            else if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Step)
                            {
                                var param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                                if (param.FactorCol.Contains(DOControlFactor.Temp))
                                {
                                    FeedPumpInfo.FlowRate_SP = CurrentFermentor.Device.DOParam.InitialFeed;
                                }
                            }
                        }
                    }

                    if (CurrentFermentor.Device.DOParam.IsControling)
                    {
                        if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                        {
                            var param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                CurrentFermentor.Device.DOParam.SP = CurrentFermentor.Device.DOParam.InitialTemp;
                            }
                        }
                        else if (CurrentFermentor.Device.DOParam.ControlStrategy == DOControlStrategy.Step)
                        {
                            var param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == CurrentFermentor.Device.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                CurrentFermentor.Device.DOParam.SP = CurrentFermentor.Device.DOParam.InitialTemp;
                            }
                        }
                    }
                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("溶氧控制：取消报错" + ex.Message);
                }
            });
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopWork(DeviceParameter device = null)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                while (_workerWorking)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void ResetDOParam(DeviceParameter deviceParameter)
        {
            _agitPIDController.Reset();
            _agitDelta = 0;
            _airPIDController.Reset();
            _o2PIDController.Reset();
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
            LogHelper.Debug($"执行溶氧时间序列项：[{item.StartTime}-{item.EndTime}]，操作值：{item.Value}");
            // 示例：设置SP值（根据实际业务替换）
            CurrentFermentor.Device.DOParam.SP = (float)item.Value;
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
            var timeSeries = CurrentFermentor.Device.DOParam.TimeSeries;
            var items = timeSeries.TimeSeriesItemCol.OrderBy(t => t.StartTime).ThenBy(t => t.EndTime).ToList();

            this._tsStartTime = DateTime.Now;
            if (CurrentFermentor.Device.DOParam.TimeSeries.TimeType == TimeType.RelativeTime)
            {
                if (CurrentFermentor.Device.BatchID < 1)
                {
                    _tsStartTime = DateTime.Now;
                }
                else
                {
                    var batch = RD3SQLHelper.QueryBatchByID(CurrentFermentor.Device.BatchID);
                    _tsStartTime = Convert.ToDateTime(batch.startDateTime);
                }
            }
            StopTimeSeriesWork();
            _shouldStop = false;
            _tsThread = new Thread(() =>
            {
                TimeSeriesItem lastExecutedItem = null; // 记录上一个执行的序列项（避免重复执行）
                LogHelper.Debug($"{CurrentFermentor.Device.Name}的溶氧时间序列开始执行（{timeSeries.TimeType}模式）");

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
                            CurrentFermentor.Device.DOParam.TimeSeries.RunningInfo = $"时间序列运行到{currentStage}/{items.Count}阶段，该阶段已运行：{$"{timeSpan.Days:00}天{timeSpan.Hours:00}时{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒"}";
                            //LogHelper.Debug(CurrentFermentor.Device.DOParam.TimeSeries.RunningInfo);

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
                            CurrentFermentor.Device.DOParam.TimeSeries.RunningInfo = $"溶氧时间序列已全部执行完成";
                            LogHelper.Debug($"{CurrentFermentor.Device.Name}的溶氧时间序列已全部执行完成");
                            _shouldStop = true;
                        }
                        else
                        {
                            TimeSpan timeSpan = TimeSpan.FromSeconds(currentMatchTime * 60);
                            CurrentFermentor.Device.DOParam.TimeSeries.RunningInfo = $"时间序列已运行：{$"{timeSpan.Days:00}天{timeSpan.Hours:00}时{timeSpan.Minutes:00}分{timeSpan.Seconds:00}秒"}";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"{CurrentFermentor.Device.Name}的溶氧时间序列执行出错：{ex.Message}", ex);
                    }

                    // 轮询间隔（500ms，可根据精度需求调整）
                    Thread.Sleep(500);
                }

                CurrentFermentor.Device.DOParam.IsControling = false;
                LogHelper.Debug($"{CurrentFermentor.Device.Name}的溶氧时间序列线程已停止");
            });
            _tsThread.Priority = ThreadPriority.Lowest;
            _tsThread.IsBackground = true;
            _tsThread.Start();
        }

        public void StopTimeSeriesWork()
        {
            _shouldStop = true; // 设置退出标志
            _tsThread?.Join(); // 等待线程结束
            CurrentFermentor.Device.DOParam.TimeSeries.RunningInfo = string.Empty;
        }
    }
}
