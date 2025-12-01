using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Fpi.Communication.Commands;
using ImTools;
using RD3.Common;

namespace RD3.Shared
{
    public class VirtualCommandWrapper : ICommandWrapper
    {
        OptimizedSlidingFilter tempFilter = new OptimizedSlidingFilter(20);
        List<double> rawTempData = [];
        List<double> list = [];
        double old = 0;
        float mfc1FlowRate = 0;
        float mfc2FlowRate = 0;
        int agit = 0;
        float temperature = 0;
        float pump1FlowRate = 0;
        float pump2FlowRate = 0;
        float pump3FlowRate = 0;
        float pump4FlowRate = 0;
        float pump5FlowRate = 0;
        float pump6FlowRate = 0;

        private Dictionary<string, KalmanFilter1D> dicDOFilter = new Dictionary<string, KalmanFilter1D>();

        public int GetAgitSpeed(string insID)
        {
            return RandomNumberUtil.GetRandomInt(1000,2000);
        }

        public DefoamingParam  GetAutoDefoamingSetting(string insID)
        {
            return new DefoamingParam();
        }

        public Tuple<int, int> GetMCUSensorTypeSetting(string insID, int sensorType)
        {
            return Tuple.Create(sensorType, RandomNumberUtil.GetRandomInt(1, 4));
        }

        public string GetMCUVersion(string insID, byte boardType)
        {
            return "Virtual V1.0";
        }

        public PeristalticPumpControlParam GetPeristalticPumpControlParam(string insID, int pumpNo)
        {
            return new PeristalticPumpControlParam();
        }

        public float GetPeristalticPumpCorrect(string insID, int pumpNo)
        {
            return RandomNumberUtil.GetRandomInt(1, 4);
        }

        public SensorCorrectParam GetSensorCorrect(string insID, byte sensorType)
        {
            return new SensorCorrectParam();
        }

        public RealTimeParam GetRealTime(string insID)
        {
            RealTimeParam realTime = new RealTimeParam();
            realTime.ReactorName = insID;
            realTime.Temp = temperature == 0 ? RandomNumberUtil.GetRandomSingle(20f, 37.1f) : temperature;
            realTime.Agit = agit == 0 ? RandomNumberUtil.GetRandomInt(300, 1500) : RandomNumberUtil.GetRandomInt(agit - 3, agit + 3);
            realTime.PH = RandomNumberUtil.GetRandomSingle(AppSession.VirtualpH - 0.1f, AppSession.VirtualpH + 0.1f);
            realTime.DO = RandomNumberUtil.GetRandomSingle(AppSession.VirtualDO, AppSession.VirtualDO);
            realTime.Pump1FlowRate = pump1FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump1FlowRate;
            realTime.Pump1Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump1FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.Pump2FlowRate = pump2FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump2FlowRate;
            realTime.Pump2Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump2FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.Pump3FlowRate = pump3FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump3FlowRate;
            realTime.Pump3Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump3FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.Pump4FlowRate = pump4FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump4FlowRate;
            realTime.Pump4Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump4FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.Pump5FlowRate = pump5FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump5FlowRate;
            realTime.Pump5Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump5FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.Pump6FlowRate = pump6FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : pump6FlowRate;
            realTime.Pump6Flow = RandomNumberUtil.GetRandomSingle();
            realTime.Pump6FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.MFC1FlowRate = mfc1FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : mfc1FlowRate;
            realTime.MFC1FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.MFC2FlowRate = mfc2FlowRate == 0 ? RandomNumberUtil.GetRandomSingle() : mfc2FlowRate;
            realTime.MFC2FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.MFC3FlowRate = RandomNumberUtil.GetRandomSingle();
            realTime.MFC3FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.MFC4FlowRate = RandomNumberUtil.GetRandomSingle();
            realTime.MFC4FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.MFC5FlowRate = RandomNumberUtil.GetRandomSingle();
            realTime.MFC5FlowCapacity = RandomNumberUtil.GetRandomSingle();
            realTime.JarWeight = RandomNumberUtil.GetRandomSingle();
            realTime.Bottle1Weight = RandomNumberUtil.GetRandomSingle();
            realTime.Bottle2Weight = RandomNumberUtil.GetRandomSingle();
            realTime.HeatingBaseCoolingNTCTemp = RandomNumberUtil.GetRandomSingle();
            realTime.HeatingBaseHeatingNTCTemp = RandomNumberUtil.GetRandomSingle();
            realTime.CoolingModuleCoolingNTCTemp = RandomNumberUtil.GetRandomSingle();
            realTime.CoolingModuleHeatingNTCTemp = RandomNumberUtil.GetRandomSingle();
            realTime.CoolingModuleRoomNTCTemp = RandomNumberUtil.GetRandomSingle();

            realTime.IntakeModuleCO2Concentration = RandomNumberUtil.GetRandomSingle();
            realTime.IntakeModuleO2Concentration = RandomNumberUtil.GetRandomSingle();
            realTime.IntakeModuleGasTemp = RandomNumberUtil.GetRandomSingle();
            realTime.IntakeModuleGasHumidity = RandomNumberUtil.GetRandomSingle();
            realTime.IntakeModuleGasPressure = RandomNumberUtil.GetRandomSingle();

            realTime.OffgasModuleCO2Concentration = RandomNumberUtil.GetRandomSingle();
            realTime.OffgasModuleO2Concentration = RandomNumberUtil.GetRandomSingle();
            realTime.OffgasModuleGasTemp = RandomNumberUtil.GetRandomSingle();
            realTime.OffgasModuleGasHumidity = RandomNumberUtil.GetRandomSingle();
            realTime.OffgasModuleGasPressure = RandomNumberUtil.GetRandomSingle();

            realTime.StirringMotorTemp = RandomNumberUtil.GetRandomSingle();
            realTime.StirringMotorPower = RandomNumberUtil.GetRandomSingle();

            realTime.AlarmBytes = [(byte)RandomNumberUtil.GetRandomInt(1,4)];
            realTime.WorkStatus = WorkStatus.Idle;

            #region 丢弃滑动窗口滤波 
            //rawTempData.Add(realTime.PH);
            //list = [];
            //bool flag = Convert.ToBoolean(VarConfig.GetValue("IsFilterWave")?.ToString());
            //foreach (var item in rawTempData)
            //{
            //    if (flag)
            //    {
            //        double filtered = tempFilter.Update(item);
            //        list.Add(filtered);
            //    }
            //    else
            //    {
            //        list.Add(item);
            //    }
            //}
            //rawTempData = list;
            #endregion

            #region 低通滤波
            bool flag = Convert.ToBoolean(VarConfig.GetValue("IsFilterWave")?.ToString());
            if (flag)
            {
                var device = AnalysisSolution.GetInstance().FermentorCol.FindFirst(t => t.Device.Name == insID).Device;
                realTime.Agit = Convert.ToInt32(RCFilter.LowPass(realTime.Agit, realTime.LastAgit, device.AgitSampleCycle, device.AgitSampleFrequency));
                realTime.LastAgit = realTime.Agit;

                if (!dicDOFilter.ContainsKey(insID))
                {
                    KalmanFilter1D kalman = new KalmanFilter1D();
                    kalman.SetParameter(0.03, 0.1, 1.5, realTime.DO);
                    dicDOFilter[insID] = kalman;
                }
                realTime.RawDO = realTime.DO;
                realTime.DO = Convert.ToSingle(dicDOFilter[insID].Update(realTime.DO));
                realTime.DOPredict = Convert.ToSingle(dicDOFilter[insID].Predict());
            }
            #endregion

            foreach (var item in EnumUtil.GetEnumValues<PeristalticPump>())
            {
                int pumpIndex = PumpMFCUtil.GetPumpIndex(insID, item);
                switch (item)
                {
                    case PeristalticPump.AcidPump:
                        if (pumpIndex == 1)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump1FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump1FlowCapacity;
                        }
                        else if (pumpIndex == 2)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump2FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump2FlowCapacity;
                        }
                        else if (pumpIndex == 3)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump3FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump3FlowCapacity;
                        }
                        else if (pumpIndex == 4)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump4FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump4FlowCapacity;
                        }
                        else if (pumpIndex == 5)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump5FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump5FlowCapacity;
                        }
                        else if (pumpIndex == 6)
                        {
                            realTime.AcidFlowSpeed = realTime.Pump6FlowRate;
                            realTime.AcidFlowCapacity = realTime.Pump6FlowCapacity;
                        }
                        break;
                    case PeristalticPump.BasePump:
                        if (pumpIndex == 1)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump1FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump1FlowCapacity;
                        }
                        else if (pumpIndex == 2)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump2FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump2FlowCapacity;
                        }
                        else if (pumpIndex == 3)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump3FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump3FlowCapacity;
                        }
                        else if (pumpIndex == 4)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump4FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump4FlowCapacity;
                        }
                        else if (pumpIndex == 5)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump5FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump5FlowCapacity;
                        }
                        else if (pumpIndex == 6)
                        {
                            realTime.BaseFlowSpeed = realTime.Pump6FlowRate;
                            realTime.BaseFlowCapacity = realTime.Pump6FlowCapacity;
                        }
                        break;
                    case PeristalticPump.FeedPump:
                        if (pumpIndex == 1)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump1FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump1FlowCapacity;
                        }
                        else if (pumpIndex == 2)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump2FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump2FlowCapacity;
                        }
                        else if (pumpIndex == 3)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump3FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump3FlowCapacity;
                        }
                        else if (pumpIndex == 4)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump4FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump4FlowCapacity;
                        }
                        else if (pumpIndex == 5)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump5FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump5FlowCapacity;
                        }
                        else if (pumpIndex == 6)
                        {
                            realTime.FeedFlowSpeed = realTime.Pump6FlowRate;
                            realTime.FeedFlowCapacity = realTime.Pump6FlowCapacity;
                        }
                        break;
                    case PeristalticPump.AFPump:
                        if (pumpIndex == 1)
                        {
                            realTime.AFFlowSpeed = realTime.Pump1FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump1FlowCapacity;
                        }
                        else if (pumpIndex == 2)
                        {
                            realTime.AFFlowSpeed = realTime.Pump2FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump2FlowCapacity;
                        }
                        else if (pumpIndex == 3)
                        {
                            realTime.AFFlowSpeed = realTime.Pump3FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump3FlowCapacity;
                        }
                        else if (pumpIndex == 4)
                        {
                            realTime.AFFlowSpeed = realTime.Pump4FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump4FlowCapacity;
                        }
                        else if (pumpIndex == 5)
                        {
                            realTime.AFFlowSpeed = realTime.Pump5FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump5FlowCapacity;
                        }
                        else if (pumpIndex == 6)
                        {
                            realTime.AFFlowSpeed = realTime.Pump6FlowRate;
                            realTime.AFFlowCapacity = realTime.Pump6FlowCapacity;
                        }
                        break;
                    case PeristalticPump.Feed2Pump:
                        if (pumpIndex == 1)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump1FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump1FlowCapacity;
                        }
                        else if (pumpIndex == 2)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump2FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump2FlowCapacity;
                        }
                        else if (pumpIndex == 3)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump3FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump3FlowCapacity;
                        }
                        else if (pumpIndex == 4)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump4FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump4FlowCapacity;
                        }
                        else if (pumpIndex == 5)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump5FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump5FlowCapacity;
                        }
                        else if (pumpIndex == 6)
                        {
                            realTime.Feed2FlowSpeed = realTime.Pump6FlowRate;
                            realTime.Feed2FlowCapacity = realTime.Pump6FlowCapacity;
                        }
                        break;
                }
            }

            foreach (var item in EnumUtil.GetEnumValues<GasType>())
            {
                int mfcIndex = PumpMFCUtil.GetMFCIndex(insID, item);
                switch (item)
                {
                    case GasType.Air:
                        if (mfcIndex == 1)
                        {
                            realTime.AirFlowSpeed = realTime.MFC1FlowRate;
                            realTime.AirFlowCapacity = realTime.MFC1FlowCapacity;
                        }
                        else if (mfcIndex == 2)
                        {
                            realTime.AirFlowSpeed = realTime.MFC2FlowRate;
                            realTime.AirFlowCapacity = realTime.MFC2FlowCapacity;
                        }
                        else if (mfcIndex == 3)
                        {
                            realTime.AirFlowSpeed = realTime.MFC3FlowRate;
                            realTime.AirFlowCapacity = realTime.MFC3FlowCapacity;
                        }
                        else if (mfcIndex == 4)
                        {
                            realTime.AirFlowSpeed = realTime.MFC4FlowRate;
                            realTime.AirFlowCapacity = realTime.MFC4FlowCapacity;
                        }
                        else if (mfcIndex == 5)
                        {
                            realTime.AirFlowSpeed = realTime.MFC5FlowRate;
                            realTime.AirFlowCapacity = realTime.MFC5FlowCapacity;
                        }
                        break;
                    case GasType.O2:
                        if (mfcIndex == 1)
                        {
                            realTime.O2FlowSpeed = realTime.MFC1FlowRate;
                            realTime.O2FlowCapacity = realTime.MFC1FlowCapacity;
                        }
                        else if (mfcIndex == 2)
                        {
                            realTime.O2FlowSpeed = realTime.MFC2FlowRate;
                            realTime.O2FlowCapacity = realTime.MFC2FlowCapacity;
                        }
                        else if (mfcIndex == 3)
                        {
                            realTime.O2FlowSpeed = realTime.MFC3FlowRate;
                            realTime.O2FlowCapacity = realTime.MFC3FlowCapacity;
                        }
                        else if (mfcIndex == 4)
                        {
                            realTime.O2FlowSpeed = realTime.MFC4FlowRate;
                            realTime.O2FlowCapacity = realTime.MFC4FlowCapacity;
                        }
                        else if (mfcIndex == 5)
                        {
                            realTime.O2FlowSpeed = realTime.MFC5FlowRate;
                            realTime.O2FlowCapacity = realTime.MFC5FlowCapacity;
                        }
                        break;
                    case GasType.CO2:
                        if (mfcIndex == 1)
                        {
                            realTime.CO2FlowSpeed = realTime.MFC1FlowRate;
                            realTime.CO2FlowCapacity = realTime.MFC1FlowCapacity;
                        }
                        else if (mfcIndex == 2)
                        {
                            realTime.CO2FlowSpeed = realTime.MFC2FlowRate;
                            realTime.CO2FlowCapacity = realTime.MFC2FlowCapacity;
                        }
                        else if (mfcIndex == 3)
                        {
                            realTime.CO2FlowSpeed = realTime.MFC3FlowRate;
                            realTime.CO2FlowCapacity = realTime.MFC3FlowCapacity;
                        }
                        else if (mfcIndex == 4)
                        {
                            realTime.CO2FlowSpeed = realTime.MFC4FlowRate;
                            realTime.CO2FlowCapacity = realTime.MFC4FlowCapacity;
                        }
                        else if (mfcIndex == 5)
                        {
                            realTime.CO2FlowSpeed = realTime.MFC5FlowRate;
                            realTime.CO2FlowCapacity = realTime.MFC5FlowCapacity;
                        }
                        break;
                    case GasType.N2:
                        if (mfcIndex == 1)
                        {
                            realTime.N2FlowSpeed = realTime.MFC1FlowRate;
                            realTime.N2FlowCapacity = realTime.MFC1FlowCapacity;
                        }
                        else if (mfcIndex == 2)
                        {
                            realTime.N2FlowSpeed = realTime.MFC2FlowRate;
                            realTime.N2FlowCapacity = realTime.MFC2FlowCapacity;
                        }
                        else if (mfcIndex == 3)
                        {
                            realTime.N2FlowSpeed = realTime.MFC3FlowRate;
                            realTime.N2FlowCapacity = realTime.MFC3FlowCapacity;
                        }
                        else if (mfcIndex == 4)
                        {
                            realTime.N2FlowSpeed = realTime.MFC4FlowRate;
                            realTime.N2FlowCapacity = realTime.MFC4FlowCapacity;
                        }
                        else if (mfcIndex == 5)
                        {
                            realTime.N2FlowSpeed = realTime.MFC5FlowRate;
                            realTime.N2FlowCapacity = realTime.MFC5FlowCapacity;
                        }
                        break;
                }
            }

            double Fa_i = realTime.AirFlowSpeed;
            double V = 700 / 1000;//700暂且写死 
            double nO2_i = realTime.IntakeModuleO2Concentration;
            double nO2_o = realTime.OffgasModuleO2Concentration;

            double nCO2_i = realTime.IntakeModuleCO2Concentration;
            double nCO2_o = realTime.OffgasModuleCO2Concentration;

            realTime.OUR = (float)SoftwareSensorUtil.CalculateOUR(Fa_i, V, nO2_i, nO2_o, nCO2_o);

            realTime.CER = (float)SoftwareSensorUtil.CalculateCER1(Fa_i, V, nO2_i, nCO2_i, nO2_o, nCO2_o);
            if (realTime.OUR != 0)
            {
                realTime.RQ = realTime.CER / realTime.OUR;
            }

            return realTime;
        }

        public TECParam GetTECPID(string insID)
        {
            return new TECParam();
        }

        public TempParam GetTempSetting(string insID)
        {
            return new TempParam() { SP = 37, LowerLimit = 0.1f, UpperLimit = 0.2f };
        }

        public void SetAgitSpeed(string insID, int speed)
        {
            agit = speed;
            return;
        }

        public void SetAutoDefoamingSetting(string insID, DefoamingParam param)
        {
            return;
        }

        public void SetDeviceParam(string insID, DeviceParam deviceParam)
        {
            return;
        }

        public void SetGasSpeed(string insID, GasParam param)
        {
            if (param.MFCNo == 1)
            {
                mfc1FlowRate = param.FlowSpeed;
            }
            else if(param.MFCNo == 2)
            {
                mfc2FlowRate = param.FlowSpeed;
            }
            return;
        }

        public void SetMCUSensorTypeSetting(string insID, int sensorType, int signal)
        {
            return;
        }


        public void SetPeristalticPumpControlParam(string insID, PeristalticPumpControlParam peristalticPumpControlParam)
        {
            if (peristalticPumpControlParam.PumpNo == 1)
            {
                pump1FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            else if (peristalticPumpControlParam.PumpNo == 2)
            {
                pump2FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            else if (peristalticPumpControlParam.PumpNo == 3)
            {
                pump3FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            else if (peristalticPumpControlParam.PumpNo == 4)
            {
                pump4FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            else if (peristalticPumpControlParam.PumpNo == 5)
            {
                pump5FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            else if (peristalticPumpControlParam.PumpNo == 6)
            {
                pump6FlowRate = peristalticPumpControlParam.FlowSpeed;
            }
            return;
        }

        public void SetPeristalticPumpCorrect(string insID, int pumpNo, int calitrationParam, float calitrationValue)
        {
            return;
        }

        public void SetSensorCorrect(string insID, SensorCorrectParam sensorCorrectParam)
        {
            return;
        }

        public void SetResetFlowCapacity(string insID, ClearModule clearModule, int serialNo = 1)
        {
            return;
        }

        public void SetTECPID(string insID, TECParam tECParam)
        {
            return;
        }

        public void SetTempSetting(string insID, TempParam tempParam)
        {
            temperature = tempParam.SP;
            return;
        }

        GasParam ICommandWrapper.GetGasSpeed(string insID, GasParam param)
        {
            param.FlowSpeed = RandomNumberUtil.GetRandomSingle();
            return param;
        }

        DeviceParam ICommandWrapper.GetDeviceParam(string insID)
        {
            return new DeviceParam() { MainIpAdress = [192, 168, 0, 166], MainPort = 8080, WifiIpAdress = [127, 0, 0, 1], WifiPort = 8081, MainGateway = [192, 168, 0, 1] };
        }

        public AlarmParam GetSoundLightAlarm(string insID)
        {
            return new AlarmParam();
        }

        public void SetSoundLightAlarm(string insID, AlarmParam alarmParam)
        {
            return;
        }

        public (System.DateTime, TimeSpan) GetTimeSync(string insID)
        {
            return (DateTime.Now, TimeSpan.Zero);
        }

        public void SetTimeSync(string insID, DateTime dateTime,TimeSpan timeSpan)
        {
            return;
        }

        public void SetSettingSync(string insID, ScreenParam param)
        {
            return;
        }

        public CondensationParam GetCondensationControl(string insID)
        {
           return new CondensationParam();
        }

        public void SetCondensationControl(string insID, CondensationParam param)
        {
            return;
        }

        public OffGasParam GetOffGas(string insID)
        {
            return new OffGasParam();
        }

        public MCUDownloadStatus GetMCUDownloadInfo(string insID)
        {
            throw new NotImplementedException();
        }

        public void SetMCUDownloadInfo(string insID, byte boardType)
        {
            return;
        }

        public void SetResetDefaultSetting(string insID, ClearModule clearModule, int serialNo = 1)
        {
            return;
        }

        public DeviceParam GetMCUDownloadAdress(string insID)
        {
            return new DeviceParam();
        }

        public void SetMCUDownloadAdress(string insID, DeviceParam deviceParam)
        {
            return;
        }

        public int GetStirringMotorType(string insID)
        {
            return -1;
        }

        public void SetStirringMotorType(string insID, int index)
        {
            return;
        }

        public float GetEPCPressure(string insID)
        {
            return 0;
        }

        public void SetEPCPressure(string insID, float pressure)
        {
            return;
        }

        public byte GetMagneticBase(string insID)
        {
            return 0;
        }

        public void SetMagneticBase(string insID, byte status)
        {
            return;
        }

        #region 0x21 读写PT100位置检测配置

        public bool GetPT100LocationCheckSetting(string insID)
        {
            return true;
        }

        public void SetPT100LocationCheckSetting(string insID, bool isEnabled)
        {
            return;
        }
        #endregion
    }
}
