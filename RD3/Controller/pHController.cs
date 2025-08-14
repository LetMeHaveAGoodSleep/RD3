using ImTools;
using Newtonsoft.Json;
using Prism.Mvvm;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.Controller
{
    public class pHController
    {
        private float _pHSP = -1;

        private float _delta = 0f;

        private bool _workerWorking = false;

        private QPIDController _pidController = new QPIDController();

        private IntelligentPHController _intelligentPHController = new IntelligentPHController();

        private BackgroundWorker _backgroundWorker;

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }

        public PumpInfo AcidPumpInfo
        {
            get
            {
                return AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.Pump == PeristalticPump.AcidPump && t.IsEnable);
            }
        }

        public PumpInfo BasePumpInfo
        {
            get
            {
                return AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.Pump == PeristalticPump.BasePump && t.IsEnable);
            }
        }

        public pHController()
        {

        }

        private void CloseAcidBase()
        {
            if (AcidPumpInfo != null)
            {
                AcidPumpInfo.FlowRate_SP = 0;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = AcidPumpInfo.PumpIndex,
                    Pump = AcidPumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = AcidPumpInfo.FlowRate_SP,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(AcidPumpInfo.DeviceID, param);
            }


            if (BasePumpInfo != null)
            {
                BasePumpInfo.FlowRate_SP = 0;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = BasePumpInfo.PumpIndex,
                    Pump = BasePumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = BasePumpInfo.FlowRate_SP,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(BasePumpInfo.DeviceID, param);
            }
        }

        private void AddAcidByFlowRate(float flowRate, bool associated)
        {
            CloseAcidBase();

            if (associated)
            {
                if (AcidPumpInfo == null) return;

                AcidPumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = AcidPumpInfo.PumpIndex,
                    Pump = AcidPumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = AcidPumpInfo.FlowRate_SP,
                    FlowCapacity = Const.MaxPumpFlowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(AcidPumpInfo.DeviceID, param);
            }

        }

        private void AddBaseByFlowRate(float flowRate, bool associated)
        {
            CloseAcidBase();

            if (associated) 
            {
                if (BasePumpInfo == null) return;

                BasePumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = BasePumpInfo.PumpIndex,
                    Pump = BasePumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = BasePumpInfo.FlowRate_SP,
                    FlowCapacity = Const.MaxPumpFlowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(BasePumpInfo.DeviceID, param);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowCapacity">单位：mL</param>
        /// <param name="seconds">单位：秒</param>
        /// <param name="associated"></param>
        private void AddAcidByFlowCapacity(float flowCapacity, int seconds, bool associated)
        {
            CloseAcidBase();

            if (associated)
            {
                if (double.IsPositiveInfinity(flowCapacity) || double.IsNegativeInfinity(flowCapacity))
                {
                    flowCapacity = 0;
                }
                if (AcidPumpInfo == null) return;

                var volume = flowCapacity;
                var hour = seconds / 3600.0f;
                var flowRate = MathF.Round(volume / hour, Const.NumericalPrecision);
                flowRate = Math.Clamp(flowRate, 0, Const.MaxPumpFlowRate);
                AcidPumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = AcidPumpInfo.PumpIndex,
                    Pump = AcidPumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = AcidPumpInfo.FlowRate_SP,
                    FlowCapacity = flowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(AcidPumpInfo.DeviceID, param);
            }
        }

        /// <summary>
        /// 返回时间，单位：秒
        /// </summary>
        /// <param name="flowCapacity">单位：mL</param>
        /// <param name="flowRate">单位：mL/h</param>
        /// <param name="associated"></param>
        /// <returns></returns>
        private int AddAcidByFlowCapacity(float flowCapacity, float flowRate, bool associated)
        {
            int result = 0;
            CloseAcidBase();

            if (associated)
            {
                if (double.IsPositiveInfinity(flowCapacity) || double.IsNegativeInfinity(flowCapacity))
                {
                    flowCapacity = 0;
                }
                if (AcidPumpInfo == null) return result;

                result = (int)Math.Ceiling(flowCapacity / flowRate * 3600f);
                AcidPumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = AcidPumpInfo.PumpIndex,
                    Pump = AcidPumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = AcidPumpInfo.FlowRate_SP,
                    FlowCapacity = flowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(AcidPumpInfo.DeviceID, param);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowCapacity">单位：mL</param>
        /// <param name="seconds">单位：秒</param>
        /// <param name="associated"></param>
        private void AddBaseByFlowCapacity(float flowCapacity,int seconds, bool associated)
        {
            CloseAcidBase();

            if (associated)
            {
                if (double.IsPositiveInfinity(flowCapacity) || double.IsNegativeInfinity(flowCapacity))
                {
                    flowCapacity = 0;
                }
                if (BasePumpInfo == null) return;

                var volume = (float)flowCapacity * 1000;
                var hour = seconds / 3600.0f;
                var flowRate = MathF.Round(volume / hour, Const.NumericalPrecision);
                flowRate = Math.Clamp(flowRate, 0, Const.MaxPumpFlowRate);
                BasePumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = BasePumpInfo.PumpIndex,
                    Pump = BasePumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = BasePumpInfo.FlowRate_SP,
                    FlowCapacity = flowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(BasePumpInfo.DeviceID, param);
            }
        }


        /// <summary>
        /// 返回时间，单位：秒
        /// </summary>
        /// <param name="flowCapacity">单位：mL</param>
        /// <param name="flowRate">单位：mL/h</param>
        /// <param name="associated"></param>
        /// <returns></returns>
        private int AddBaseByFlowCapacity(float flowCapacity, float flowRate, bool associated)
        {
            int result = 0;
            CloseAcidBase();

            if (associated)
            {
                if (double.IsPositiveInfinity(flowCapacity) || double.IsNegativeInfinity(flowCapacity))
                {
                    flowCapacity = 0;
                }
                if (BasePumpInfo == null) return result;

                result = (int)Math.Ceiling(flowCapacity / flowRate * 3600f);
                BasePumpInfo.FlowRate_SP = flowRate;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = BasePumpInfo.PumpIndex,
                    Pump = BasePumpInfo.Pump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = BasePumpInfo.FlowRate_SP,
                    FlowCapacity = flowCapacity
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(BasePumpInfo.DeviceID, param);
            }
            return result;
        }

        /// <summary>
        /// 判断ph是否稳定
        /// </summary>
        /// <param name="ph1"></param>
        /// <param name="ph2"></param>
        /// <returns></returns>
        private bool phIsSteady(double ph1, double ph2)
        {
            double offset = Math.Abs(ph1 - ph2);
            return offset < 0.2;
        }

        public void StartWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                return;
            }

            _pidController.Reset();
            _delta = 0f;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (s, e) =>
            {
                _workerWorking = true;
                var deviceParameter = CurrentDeviceParameter;
                e.Result = deviceParameter.Name;
                if (deviceParameter.PHParam.PHControlMode == PHControlMode.PID)
                {
                    if (AcidPumpInfo != null && deviceParameter.PHParam.AcidAssociated)
                    {
                        AcidPumpInfo.IsControlled = AcidPumpInfo.IsControling = true;
                    }
                    if (BasePumpInfo != null && deviceParameter.PHParam.BaseAssociated)
                    {
                        BasePumpInfo.IsControlled = BasePumpInfo.IsControling = true;
                    }
                    PIDInfo info = null;
                    PIDInfo lastPid = null;
                    while (true)
                    {
                        try
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            var pIDInfos = PIDInfoManager.GetInstance().PIDInfos;
                            if (pIDInfos == null)
                            {
                                HandyControl.Controls.MessageBox.Show("PID调控策略列表为空", "温馨提示");
                                _workerWorking = false;
                                return;
                            }

                            RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);

                            if (realTimeParam.PH >= deviceParameter.PHParam.PH_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("PH_酸") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTimeParam.PH <= deviceParameter.PHParam.PH_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("PH_碱") && t.deviceID == deviceParameter.Name);
                            }
                            if (info == null)
                            {
                                HandyControl.Controls.MessageBox.Show(string.Format("反应器{0}不存在PH的PID调控策略", deviceParameter.Name));
                                _workerWorking = false;
                                return;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                _pidController.Reset();
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("PH调控：由{0}切换至{1}", lastPid.PidName, info.PidName));
                                    _delta = 0;
                                }
                            }

                            lastPid = info;

                            _pidController.SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold);
                            _pidController.SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            _pidController.SetIntegralLimits(-20, 20);
                            _pidController.SetTarget(deviceParameter.PHParam.PH_PV);

                            LogHelper.Debug(string.Format("反应器{5},PH预设值：{0}，PH当前值：{4}，P：{1}，I：{2}，D：{3}", deviceParameter.PHParam.PH_PV, info.P, info.I, info.D, realTimeParam.PH, deviceParameter.Name));
                            if (realTimeParam.PH >= deviceParameter.PHParam.PH_PV - info.deadArea && realTimeParam.PH <= deviceParameter.PHParam.PH_PV + info.deadArea)
                            {
                                CloseAcidBase();

                                //进入死区后，重置Pid的积分系数和误差数组
                                _pidController.Reset();
                                _delta = 0;

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index = 0;
                                while (index < count)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }

                                    index += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                            // 增量式使用
                            float temp = _pidController.CalculateIncremental((float)realTimeParam.PH);
                            _delta += temp;
                            LogHelper.Debug(string.Format("{1} PH 总Delta:{0} 单次Delta:{2}", _delta, deviceParameter.Name, temp));
                            float flowRate = Math.Clamp(Math.Abs(_delta), 0, Const.MaxPumpFlowRate);
                            if (_delta < 0)//酸泵
                            {
                                AddAcidByFlowRate(flowRate, deviceParameter.PHParam.AcidAssociated);
                            }
                            else if (_delta > 0)//碱泵
                            {
                                AddBaseByFlowRate(flowRate, deviceParameter.PHParam.BaseAssociated);
                            }

                            int count1 = info.Interval <= 0 ? 1 : info.Interval;
                            int index1 = 0;
                            while (index1 < count1)
                            {
                                if (_backgroundWorker.CancellationPending)
                                {
                                    _workerWorking = false;
                                    return;
                                }

                                index1 += 1;
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("PHPID调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
                else if (deviceParameter.PHParam.PHControlMode == PHControlMode.Buffer)
                {
                    try
                    {
                        while (true)
                        {
                            if (_backgroundWorker.CancellationPending)
                            {
                                _workerWorking = false;
                                return;
                            }

                            pHControlUtils.InitpHinfo();
                            var phInfo = pHControlUtils.phInfo;
                            string deviceId = deviceParameter.Name;
                            double aslope = phInfo.aslope;//酸
                            double bslope = phInfo.bslope;//碱
                            double dead_zone = 0.1;
                            double workpH = phInfo.ph0;//
                            int index = ClockSupervisor.realDatasDic[deviceId].Count - 2;
                            double lastpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                            index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                            double currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;

                            bool steady = phIsSteady(currentpH, lastpH);//判断是否稳定

                            if (steady)//稳定
                            {
                                //double offsetpH = currentpH - workpH;
                                double offsetpHAbs = Math.Abs(currentpH - workpH);

                                if (offsetpHAbs < dead_zone)//是否在死区外，是否满足控制策略如酸调、碱调)
                                {
                                    Thread.Sleep(1000);
                                    lastpH = currentpH;
                                    index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                    currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;//当前ph
                                    continue;
                                }

                                if (offsetpHAbs < 1) //在buffer区，
                                {
                                    double slope = aslope;
                                    if (currentpH < workpH)
                                    {//加碱
                                        slope = aslope;
                                    }
                                    else
                                    {//加酸
                                        slope = bslope;
                                    }
                                    double newAslope = slope;
                                    //double newAslope = slope;//
                                    bool needControl = offsetpHAbs > dead_zone;
                                    if (needControl)//在死区外
                                    {
                                        double lastph = currentpH;
                                        //1 打入改变0.1pH(根据上述slope计算)的试剂量
                                        double pump = 0.1 / slope;//单位L-打入
                                        if (currentpH < workpH)// 判断加酸还是加碱
                                        {
                                            //加碱
                                            pump = 0.1 / bslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量
                                            LogHelper.Debug(string.Format("bslope:{0};pump:{1}", bslope, pump));

                                            AddBaseByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                        }
                                        else//加酸
                                        {
                                            pump = 0.1 / aslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));
                                            AddAcidByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                        }

                                        while (true)
                                        {
                                            int index1 = 0;
                                            int time = (int)(60 * phInfo.T90);//等待3分钟，判断是否不在死区内，
                                            while (time > 0)
                                            {
                                                if (_backgroundWorker.CancellationPending)
                                                { 
                                                    return;
                                                }

                                                Thread.Sleep(1000);
                                                lastpH = currentpH;
                                                index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                                currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;//当前ph
                                                time--;
                                                index1 += 1;
                                                if (index1 >= 120)
                                                {
                                                    deviceParameter.BaseParam.Base_PV = 0;
                                                    deviceParameter.AcidParam.Acid_PV = 0;
                                                }
                                            }
                                            double offset = currentpH - workpH;
                                            if (Math.Abs(offset) > dead_zone)//如不在死区内，修正slope=上述slope*实际pH改变/0.1
                                            {
                                                newAslope = newAslope * Math.Abs(lastph - currentpH) / 0.1;
                                            }
                                            else//在死区内，跳出
                                            {
                                                break;
                                            }

                                            offsetpHAbs = currentpH - workpH;
                                            pump = 0.5 * offsetpHAbs / newAslope; //单位L - 打入
                                            if (currentpH < workpH)// 判断加酸还是加碱
                                            {
                                                LogHelper.Debug(string.Format("newAslope:{0};pump:{1}", newAslope, pump));
                                                //加碱
                                                AddBaseByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                            }
                                            else//加酸
                                            {
                                                LogHelper.Debug(string.Format("newAslope:{0};pump:{1}", newAslope, pump));
                                                AddAcidByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                            }
                                            lastph = currentpH;
                                        }
                                    }
                                }
                                else//不在buffer区
                                {
                                    while (true)
                                    {
                                        double pump = 0.1 / aslope;
                                        if (currentpH < workpH)// 判断加酸还是加碱
                                        {
                                            //加碱
                                            pump = 0.1 / bslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));

                                            AddBaseByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                        }
                                        else//加酸
                                        {
                                            pump = 0.1 / aslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));

                                            AddAcidByFlowCapacity((float)pump, 120, deviceParameter.PHParam.BaseAssociated);
                                        }

                                        //等待3分钟
                                        int time = (int)(60 * phInfo.T90);
                                        int index1 = 0;
                                        while (time > 0)
                                        {
                                            if (_backgroundWorker.CancellationPending)
                                            {
                                                _workerWorking = false;
                                                return;
                                            }

                                            Thread.Sleep(1000);
                                            lastpH = currentpH;
                                            index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                            currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                            time--;

                                            index1 += 1;
                                            if (index1 >= 120)
                                            {
                                                deviceParameter.BaseParam.Base_PV = 0;
                                                deviceParameter.AcidParam.Acid_PV = 0;
                                            }
                                        }
                                        if (Math.Abs(currentpH - workpH) < 1)
                                        {
                                            break;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                int count = 0;
                                while (!steady)
                                {
                                    int time = (int)(60 * phInfo.T90);
                                    int index1 = 0;
                                    while (time > 0)//等一分钟
                                    {
                                        if (_backgroundWorker.CancellationPending)
                                        {
                                            _workerWorking = false;
                                            return;
                                        }

                                        Thread.Sleep(1000);
                                        lastpH = currentpH;
                                        index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                        currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                        time--;

                                        index1 += 1;
                                        if (index1 >= 120)
                                        {
                                            deviceParameter.BaseParam.Base_PV = 0;
                                            deviceParameter.AcidParam.Acid_PV = 0;
                                        }
                                    }
                                    steady = phIsSteady(lastpH, currentpH);//判断是否稳定
                                    lastpH = currentpH;
                                    index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                    currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                    count++;
                                    if (count > 5)//提示报警
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (deviceParameter.PHParam.PHControlMode == PHControlMode.Adaptive)
                {
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();

                    _intelligentPHController = new IntelligentPHController()
                    {
                        TargetPH = deviceParameter.PHParam.PH_PV
                    };

                    while (true)
                    {
                        if (_backgroundWorker.CancellationPending)
                        {
                            _workerWorking = false;
                            return;
                        }
                        _intelligentPHController.TargetPH = deviceParameter.PHParam.PH_PV;
                        PropertyMapper.Map(deviceParameter.AdaptivepHParameter, _intelligentPHController);

                        var realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        var (isAlkali, volume) = _intelligentPHController.CalculateDosing(currentPH: realTimeParam.PH, currentRPM: realTimeParam.Agit, currentVolume_L: realTimeParam.JarWeight / 1000);
                        if (volume > 0)
                        {
                            if (isAlkali)//加碱
                            {
                                int waitSeconds = AddBaseByFlowCapacity((float)volume, AppSession.DefaultPumpFlowRate, deviceParameter.PHParam.BaseAssociated);
                                LogHelper.Debug(string.Format("反应器{0},PH预设值：{1}，PH当前值：{2}，体积：{3}", deviceParameter.Name, deviceParameter.PHParam.PH_PV, realTimeParam.PH, volume));
                                while (waitSeconds > 0 && !InstrumentSolution.GetInstance().IsSimulation)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                    waitSeconds--;
                                }
                            }
                            else//加酸
                            {
                                int waitSeconds = AddAcidByFlowCapacity((float)volume, AppSession.DefaultPumpFlowRate, deviceParameter.PHParam.AcidAssociated);
                                LogHelper.Debug(string.Format("反应器{0},PH预设值：{1}，PH当前值：{2}，体积：{3}", deviceParameter.Name, deviceParameter.PHParam.PH_PV, realTimeParam.PH, volume));
                                while (waitSeconds > 0 && !InstrumentSolution.GetInstance().IsSimulation)
                                {
                                    if (_backgroundWorker.CancellationPending)
                                    {
                                        _workerWorking = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                    waitSeconds--;
                                }
                            }
                        }
                        else
                        {
                            CloseAcidBase();
                            Thread.Sleep(1000);
                        }
                    }
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                try
                {
                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;

                    CloseAcidBase();

                    if (AcidPumpInfo != null && CurrentDeviceParameter.PHParam.AcidAssociated)
                    {
                        AcidPumpInfo.IsControlled = AcidPumpInfo.IsControling = false;
                    }
                    if (BasePumpInfo != null && CurrentDeviceParameter.PHParam.BaseAssociated)
                    {
                        BasePumpInfo.IsControlled = BasePumpInfo.IsControling = false;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("PH调控事件完成出错" + ex.Message);
                }
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

            CloseAcidBase();

            if (AcidPumpInfo != null && CurrentDeviceParameter.PHParam.AcidAssociated)
            {
                AcidPumpInfo.IsControlled = AcidPumpInfo.IsControling = false;
            }
            if (BasePumpInfo != null && CurrentDeviceParameter.PHParam.BaseAssociated)
            {
                BasePumpInfo.IsControlled = BasePumpInfo.IsControling = false;
            }
        }
    }
}
