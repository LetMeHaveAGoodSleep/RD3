using ImTools;
using RD3.Common;
using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.Shared
{
    /// <summary>
    /// Probing控制
    /// </summary>
    public class ProbingController
    {
        float Otol;
        float Tpulse;
        float Tcontrol;
        float rp;
        float Fpulse;
        ProbingParameter prob;
        bool ctrlFlag = false;

        float sumOffsetY = 0;
        float flowRate = 0;
        DeviceParameter deviceParameter = null;

        /// <summary>
        /// 初始化参数
        /// </summary>
        public void InitProbParam(ProbingParameter prob)
        {
            Otol = 0.2f * prob.Oreac;
            Tpulse = prob.Tmax;
            Tcontrol = prob.Tmax * 4;
            this.prob = prob;
            this.flowRate = prob.F;
        }

        public void SetDevice(DeviceParameter deviceParameter)
        {
            this.deviceParameter = deviceParameter;
        }

        /// <summary>
        /// 开始控制
        /// </summary>
        public void StartCtrl()
        {
            ctrlFlag = true;
            sumOffsetY = 0;
            new Task(Prob).Start();//异步启动prob控制
        }
        /// <summary>
        /// 结束控制
        /// </summary>
        public void StopCtrl()
        {
            ctrlFlag = false;
        }
        /// <summary>
        /// 控制补料
        /// </summary>
        private void ProbF()
        {
            float yr = GetYr();
            int pulseCount = (int)Tpulse;//脉冲
            float rF = prob.F;//是否需要迭代？

            while (ctrlFlag)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == prob?.DeviceID);
                InitProbParam(param);

                if (!DOControlFeed())
                {
                    float op = GetYk();
                    if (DoStability(op))//Do是否稳定
                    {
                        op = GetYk();
                        float opFrontPulse = op;
                        float rp = 4 * prob.Oreac / op;
                        Fpulse = rp * prob.F;
                        prob.F = prob.F + Fpulse;
                        //设置补料
                        DoFeedCtrl(prob.F);

                        int count = pulseCount;
                        while (count > 0)//脉冲
                        {
                            if (!ctrlFlag)
                            {
                                return;
                            }
                            count--;
                            Thread.Sleep(1000);
                            op = GetYk();
                        }
                        //脉冲结束，做逻辑判断
                        float offsetOp = Math.Abs(op - opFrontPulse);
                        if (op >= opFrontPulse || (op < opFrontPulse && offsetOp < prob.Oreac))//??正脉冲后，如果Op不下降或下降幅度<Oreac
                        {
                            //减小F（当前补料），Finc = -Fpulse
                            prob.F -= Fpulse;
                        }
                        else if (op < opFrontPulse && offsetOp > prob.Oreac)//如果正脉冲后Op下降超过Oreac，
                        {
                            int N = GetN();
                            if (N < prob.Nmax1) //如果N < Nmax -,则增加F
                            {
                                prob.F = prob.k * prob.F * Math.Abs(op) / (op * (-prob.Osp));
                            }
                            else if (N > prob.Nmax2)//如果N>Nmax+，则减小F，Finc =-Fpulse
                            {
                                prob.F -= Fpulse;
                            }
                            //否则不变
                        }
                        //设置补料
                        DoFeedCtrl(prob.F);

                        count = (int)Tcontrol;//控制
                        while (count > 0)
                        {
                            if (!ctrlFlag)
                            {
                                return;
                            }
                            count--;
                            Thread.Sleep(1000);
                            GetYk();
                        }

                        continue;
                    }
                }
                else//PI控制
                {
                    float yk = GetYk();

                    float offsetY = Math.Abs(yk) - yr;
                    float offsetF = (prob.k * offsetY + prob.ki * sumOffsetY) * prob.F / yk;//PI控制
                    sumOffsetY += offsetY;//累积量

                    prob.F += offsetF;

                    //设置补料
                    DoFeedCtrl(prob.F);

                    Thread.Sleep(1000);
                }
            }
        }

        private void Prob()
        {
            LogHelper.Debug(string.Format("Probe:开始：泵速{0}", flowRate));

            DoFeedCtrl(flowRate);

            while (ctrlFlag)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == prob?.DeviceID);
                InitProbParam(param);

                var realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(prob?.DeviceID);
                while (Math.Abs(realTimeParam.DO - prob.Osp)  <= prob.AllowDiff * prob.Oreac)
                {
                    while (DOControlFeed())
                    {
                        AppSession.DOPause = false;
                        DoFeedCtrl(flowRate);
                        Thread.Sleep(1000);
                        continue;
                    }

                    LogHelper.Debug(string.Format("Probe:暂停DO控制", flowRate));
                    AppSession.DOPause = true;
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(prob?.DeviceID);
                    float oldDO = realTimeParam.DO;
                    float coefficient = 4 * prob.Oreac / (100 - prob.Osp);
                    float fPluse = coefficient * flowRate;
                    float temp = fPluse + flowRate;

                    LogHelper.Debug(string.Format("Probe:暂停DO控制，速度{0}", temp));
                    DoFeedCtrl(temp);

                    int count = Convert.ToInt32(prob.Tmax);
                    while (count > 0)
                    {
                        if (!ctrlFlag)
                        {
                            return;
                        }

                        count--;
                        Thread.Sleep(1000);
                    }

                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(prob?.DeviceID);
                    float diff = oldDO - realTimeParam.DO;

                    if (diff < prob.Oreac)//下降幅度小于Oreac
                    {
                        flowRate -= fPluse;
                        LogHelper.Debug(string.Format("Probe:下降幅度小，设定速度{0}，脉冲速度{1}", flowRate, fPluse));
                        DoFeedCtrl(flowRate);
                    }
                    else if (diff > prob.Oreac)//下降幅度大于Oreac
                    {
                        float finc = prob.k * flowRate * Math.Abs(diff) / (100 - prob.Osp);
                        flowRate = finc;
                        LogHelper.Debug(string.Format("Probe:下降幅度大，设定速度{0},变更速度{1}", flowRate, finc));
                        DoFeedCtrl(flowRate);
                    }

                    AppSession.DOPause = false;
                    int controlCount = Convert.ToInt32(Tcontrol);
                    while (controlCount > 0)
                    {
                        if (!ctrlFlag)
                        {
                            return;
                        }
                        controlCount--;
                        Thread.Sleep(1000);
                    }
                }

                AppSession.DOPause = false;
                DoFeedCtrl(flowRate);
                Thread.Sleep(1000);
            }

            AppSession.DOPause = false;
            DoFeedCtrl(flowRate);
            Thread.Sleep(1000);
        }

        /// <summary>
        /// 控制补料速度
        /// </summary>
        private void DoFeedCtrl(float rF)
        {
            int pumpNo = PumpMFCUtil.GetPumpIndex(prob?.DeviceID, PeristalticPump.FeedPump);
            if (pumpNo > -1)
            {
                rF = rF >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : rF <= 0 ? 0 : rF;
                deviceParameter.FeedParam1.Feed_PV = rF;
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = pumpNo,
                    FlowSpeed = rF,
                    FlowCapacity = rF * Tpulse / 3600f
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(prob?.DeviceID, param);
                LogHelper.Debug(string.Format("Probe:泵速{0}", rF));
            }

        }

        /// <summary>
        /// Do是否稳定
        /// </summary>
        /// <returns></returns>
        public bool DoStability(float yk)
        {
            return Math.Abs(yk) <= Otol;
        }
        /// <summary>
        /// N-转数
        /// </summary>
        /// <returns></returns>
        private int GetN()
        {
            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(prob?.DeviceID);
            int? N = realTime?.Agit;
            return N == null ? -1 : Convert.ToInt32(N);
        }
        /// <summary>
        /// 控制脉冲高度Ops,通常为3-5%
        /// </summary>
        /// <returns></returns>
        private float GetYr()
        {
            var yr = prob?.Ops;
            return yr == null ? float.MaxValue : Convert.ToSingle(yr);
        }
        /// <summary>
        /// y(k) = |Op|
        /// 返回O*-Osp
        /// </summary>
        /// <returns></returns>
        private float GetYk()
        {
            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(prob?.DeviceID);
            var d = realTime?.DO;
            var yk = d - prob?.Osp;
            //yList.Add(Math.Abs(yk));
            return yk == null ? float.MaxValue : Convert.ToSingle(yk);
        }
        /// <summary>
        /// 如果DO正在控制补料，暂停
        /// </summary>
        /// <returns></returns>
        public bool DOControlFeed()
        {
            if (deviceParameter == null)
            {
                foreach (var item in AnalysisSolution.GetInstance().ReactorCol.Where(t => t.Name == prob?.DeviceID))
                {
                    deviceParameter = item.Clone() as DeviceParameter;
                    break;
                }
            }

            return Convert.ToBoolean(deviceParameter?.FeedSuspend);
        }
    }
}
