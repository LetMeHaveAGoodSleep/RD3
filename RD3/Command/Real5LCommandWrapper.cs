using Fpi.Communication.Commands;
using Fpi.Communication.Commands.Config;
using Fpi.Communication.Manager;
using Fpi.Instruments;
using ImTools;
using Newtonsoft.Json.Linq;
using RD3.Common;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Real5LCommandWrapper : ICommandWrapper
    {
        OptimizedSlidingFilter tempFilter = new OptimizedSlidingFilter(20);
        List<double> rawTempData = [];
        List<double> list = [];
        double old = 0f;

        private Dictionary<string, KalmanFilter1D> dicDOFilter = new Dictionary<string, KalmanFilter1D>();

        public Real5LCommandWrapper()
        {
        }

        public RecvCommand Send(string insId, SendCommand sendCommand)
        {
            return (RecvCommand)PortManager.GetInstance().Send(insId, sendCommand);
        }

        #region 0x01 读写温度设置
        public TempParam GetTempSetting(string insID)
        {
            TempParam param = new TempParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTempParam, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.SP = recvCommand.GetSingle(ParamId_5L.RWTempParam_ReadWrite_Temp);
                    //param.IsEnable = recvCommand.GetByte(ParamId_5L.RWTempParam_ReadWrite_Enable) == 0x00 ? false : true;
                    param.TecControlMode= (TecControlMode)recvCommand.GetByte(ParamId_5L.RWTempParam_ReadWrite_Enable);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return param;
        }

        public void SetTempSetting(string insID, TempParam tempParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTempParam, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWTempParam_ReadWrite_Temp, tempParam.SP);
                sendCommand.SetValue(ParamId_5L.RWTempParam_ReadWrite_Enable, (byte)tempParam.TecControlMode);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x02 读写搅拌电机设置
        public int GetAgitSpeed(string insID)
        {
            var value = -1;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWAgitParam, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    value = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWAgitParam_ReadWrite_Agit));
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return value;
        }

        public void SetAgitSpeed(string insID, int speed)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWAgitParam, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWAgitParam_ReadWrite_Agit, BitConverter.GetBytes(speed));
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x03 蠕动泵控制
        public PeristalticPumpControlParam GetPeristalticPumpControlParam(string insID, int pumpNo)
        {
            PeristalticPumpControlParam param = new();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpControl, CommandExtendId.Read);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpControl_Read_SerialNo, (byte)pumpNo);
                RecvCommand recvCommand = Send(insID, sendCommand);

                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    //param.Pump = (PeristalticPump)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpControl_ReadResponse_SerialNo);
                    param.PumpNo = recvCommand.GetByte(ParamId_5L.RWPeristalticPumpControl_ReadWrite_SerialNo);
                    param.ControlMode = (PumpControlMode)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpControl_ReadWrite_ControlMode);
                    param.FlowSpeed = recvCommand.GetSingle(ParamId_5L.RWPeristalticPumpControl_ReadWrite_FlowSpeed);
                    param.FlowCapacity = recvCommand.GetSingle(ParamId_5L.RWPeristalticPumpControl_ReadWrite_FlowCapacity);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return param;
        }

        public void SetPeristalticPumpControlParam(string insID, PeristalticPumpControlParam peristalticPumpControlParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpControl, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpControl_ReadWrite_ControlMode, (byte)peristalticPumpControlParam.ControlMode);
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpControl_ReadWrite_SerialNo, (byte)peristalticPumpControlParam.PumpNo);
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpControl_ReadWrite_FlowSpeed, peristalticPumpControlParam.FlowSpeed);
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpControl_ReadWrite_FlowCapacity, peristalticPumpControlParam.FlowCapacity);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x04 读写自动消泡
        public DefoamingParam GetAutoDefoamingSetting(string insID)
        {
            DefoamingParam param = new();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWAutoDefoamingControl, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);

                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.SensorEnable = recvCommand.GetByte(ParamId_5L.RWAutoDefoamingControl_ReadWrite_Enable) == 0;//0:使能 1:不使能
                    param.PumpNo = recvCommand.GetByte(ParamId_5L.RWAutoDefoamingControl_ReadWrite_PumpNo);
                    param.Cycle = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWAutoDefoamingControl_ReadWrite_CheckCycle));
                    param.TimeRatio = recvCommand.GetSingle(ParamId_5L.RWAutoDefoamingControl_ReadWrite_DutyCycle);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return param;
        }

        public void SetAutoDefoamingSetting(string insID, DefoamingParam param)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWAutoDefoamingControl, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWAutoDefoamingControl_ReadWrite_PumpNo, param.PumpNo);
                sendCommand.SetValue(ParamId_5L.RWAutoDefoamingControl_ReadWrite_Enable, (param.SensorEnable ? 0x01 : 0x00));
                sendCommand.SetValue(ParamId_5L.RWAutoDefoamingControl_ReadWrite_CheckCycle, BitConverter.GetBytes(param.Cycle));
                sendCommand.SetValue(ParamId_5L.RWAutoDefoamingControl_ReadWrite_DutyCycle, param.TimeRatio);
                sendCommand.SetValue(ParamId_5L.RWAutoDefoamingControl_ReadWrite_FlowRate, param.FlowSpeed);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x05 读写MFC控制（气体）
        public GasParam GetGasSpeed(string insID, GasParam param)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMFCControl, CommandExtendId.Read);
            sendCommand.SetValue(ParamId_5L.RWMFCControl_Read_SerialNo, param.MFCNo);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.MFCNo = recvCommand.GetByte(ParamId_5L.RWMFCControl_ReadWrite_SerialNo);
                    param.FlowSpeed = recvCommand.GetSingle(ParamId_5L.RWMFCControl_ReadWrite_FlowSpeed);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return param;
        }

        public void SetGasSpeed(string insID, GasParam param)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMFCControl, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWMFCControl_ReadWrite_SerialNo, (byte)param.MFCNo);
                sendCommand.SetValue(ParamId_5L.RWMFCControl_ReadWrite_FlowSpeed, param.FlowSpeed);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x06 读写传感器型号
        public Tuple<int, int> GetMCUSensorTypeSetting(string insID, int sensorType)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSensorSetting, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    int kind = recvCommand.GetByte(ParamId_5L.RWSensorSetting_ReadWrite_Kind);
                    int type = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWSensorSetting_ReadWrite_Type));
                    return Tuple.Create(kind, type);
                }
                else
                {
                    return Tuple.Create(-1, -1);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return Tuple.Create(-1, -1);
        }

        public void SetMCUSensorTypeSetting(string insID, int sensorType, int signal)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSensorSetting, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWSensorSetting_ReadWrite_Kind, (byte)sensorType);
                sendCommand.SetValue(ParamId_5L.RWSensorSetting_ReadWrite_Type, BitConverter.GetBytes(signal));
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x08 读写主要功能控制开关  删除
        //public SwitchMode GetFunctionControlMode(string insID, ControlObject controlObject)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWFunctionControl, CommandExtendId.Read);
        //    sendCommand.SetValue(ParamId_5L.RWFunctionControl_Read_Object, (byte)controlObject);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            SwitchMode switchMode = (SwitchMode)recvCommand.GetByte(ParamId_5L.RWFunctionControl_ReadResponse_Mode);
        //            return switchMode;
        //        }
        //        else
        //        {
        //            return SwitchMode.Close;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");
        //    }
        //    return SwitchMode.Close;
        //}

        //public void SetFunctionControlMode(string insID, ControlObject controlObject, SwitchMode switchMode)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWFunctionControl, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWFunctionControl_Write_Object, (byte)controlObject);
        //        sendCommand.SetValue(ParamId_5L.RWFunctionControl_Write_Mode, (byte)switchMode);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");
        //    }
        //}
        #endregion

        #region 0x09 读实时信息
        public RealTimeParam GetRealTime(string insID)
        {
            RealTimeParam realTimeParam = new RealTimeParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RRealtimeParam, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand == null) return realTimeParam;

                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    realTimeParam.Temp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Temp), Const.NumericalPrecision);
                    realTimeParam.PH = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_PH), Const.NumericalPrecision);
                    realTimeParam.Agit = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RRealtimeParam_ReadResponse_Agit));
                    realTimeParam.DO = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_DO), Const.NumericalPrecision);
                    realTimeParam.StirringMotorTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_StirringMotorTemp), Const.NumericalPrecision);
                    realTimeParam.StirringMotorPower = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_StirringMotorPower), Const.NumericalPrecision);

                    realTimeParam.Pump1FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump1FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump1Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump1Flow), Const.NumericalPrecision);
                    realTimeParam.Pump1FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump1FlowTotal), Const.NumericalPrecision);
                    realTimeParam.Pump2FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump2FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump2Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump2Flow), Const.NumericalPrecision);
                    realTimeParam.Pump2FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump2FlowTotal), Const.NumericalPrecision);
                    realTimeParam.Pump3FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump3FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump3Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump3Flow), Const.NumericalPrecision);
                    realTimeParam.Pump3FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump3FlowTotal), Const.NumericalPrecision);
                    realTimeParam.Pump4FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump4FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump4Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump4Flow), Const.NumericalPrecision);
                    realTimeParam.Pump4FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump4FlowTotal), Const.NumericalPrecision);
                    realTimeParam.Pump5FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump5FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump5Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump5Flow), Const.NumericalPrecision);
                    realTimeParam.Pump5FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump5FlowTotal), Const.NumericalPrecision);
                    realTimeParam.Pump6FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump6FlowRate), Const.NumericalPrecision);
                    realTimeParam.Pump6Flow = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump6Flow), Const.NumericalPrecision);
                    realTimeParam.Pump6FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Pump6FlowTotal), Const.NumericalPrecision);

                    realTimeParam.MFC1FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC1FlowRate), Const.NumericalPrecision);
                    realTimeParam.MFC1FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC1FlowCapacity), Const.NumericalPrecision);
                    realTimeParam.MFC2FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC2FlowRate), Const.NumericalPrecision);
                    realTimeParam.MFC2FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC2FlowCapacity), Const.NumericalPrecision);
                    realTimeParam.MFC3FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC3FlowRate), Const.NumericalPrecision);
                    realTimeParam.MFC3FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC3FlowCapacity), Const.NumericalPrecision);
                    realTimeParam.MFC4FlowRate = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC4FlowRate), Const.NumericalPrecision);
                    realTimeParam.MFC4FlowCapacity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_MFC4FlowCapacity), Const.NumericalPrecision);

                    realTimeParam.JarWeight = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_JarWeight), Const.NumericalPrecision);
                    realTimeParam.ReserveWeight = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_ReserveWeight), Const.NumericalPrecision);
                    realTimeParam.Bottle1Weight = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Bottle1Weight), Const.NumericalPrecision);
                    realTimeParam.Bottle2Weight = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_Bottle2Weight), Const.NumericalPrecision);

                    realTimeParam.PHSensorTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_pHSensorTemp), Const.NumericalPrecision);
                    realTimeParam.DOSensorTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_DOSensorTemp), Const.NumericalPrecision);
                    realTimeParam.HeatingBlanketNTCTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_HeatingBlanketNTCTemp), Const.NumericalPrecision);
                    realTimeParam.RoomTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_RoomTemp), Const.NumericalPrecision);

                    realTimeParam.IntakeModuleCO2Concentration = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_IntakeModuleCO2Concentration), Const.NumericalPrecision);
                    realTimeParam.IntakeModuleO2Concentration = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_IntakeModuleO2Concentration), Const.NumericalPrecision);
                    realTimeParam.IntakeModuleGasTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_IntakeModuleGasTemp), Const.NumericalPrecision);
                    realTimeParam.IntakeModuleGasHumidity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_IntakeModuleGasHumidity), Const.NumericalPrecision);
                    realTimeParam.IntakeModuleGasPressure = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_IntakeModuleGasPressure), Const.NumericalPrecision);

                    realTimeParam.OffgasModuleCO2Concentration = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_OffgasModuleCO2Concentration), Const.NumericalPrecision);
                    realTimeParam.OffgasModuleO2Concentration = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_OffgasModuleO2Concentration), Const.NumericalPrecision);
                    realTimeParam.OffgasModuleGasTemp = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_OffgasModuleGasTemp), Const.NumericalPrecision);
                    realTimeParam.OffgasModuleGasHumidity = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_OffgasModuleGasHumidity), Const.NumericalPrecision);
                    realTimeParam.OffgasModuleGasPressure = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_OffgasModuleGasPressure), Const.NumericalPrecision);

                    realTimeParam.EPCPressure = MathF.Round(recvCommand.GetSingle(ParamId_5L.RRealtimeParam_ReadResponse_EPCPressure), Const.NumericalPrecision);

                    realTimeParam.HasFoam = recvCommand.GetByte(ParamId_5L.RRealtimeParam_ReadResponse_HasFoam) == 0x01;
                    realTimeParam.TempControling = recvCommand.GetByte(ParamId_5L.RRealtimeParam_ReadResponse_TempControlStatus) == 0x01;

                    realTimeParam.AlarmBytes = recvCommand.GetBytes(ParamId_5L.RRealtimeParam_ReadResponse_AlarmCodes);

                    #region 滤波开关 转速&DO
                    bool flag = Convert.ToBoolean(VarConfig.GetValue("IsFilterWave")?.ToString());
                    if (flag)
                    {
                        //float frequency = Convert.ToSingle(VarConfig.GetValue("Frequency")?.ToString());
                        //var temp = RCFilter.LowPass(realTimeParam.PH, old, AppSession.Interval, frequency);
                        //old = temp;
                        //realTimeParam.PH = (float)temp;

                        //var device = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == insID);
                        //realTimeParam.Agit = Convert.ToInt32(RCFilter.LowPass(realTimeParam.Agit, realTimeParam.LastAgit, device.AgitSampleCycle, device.AgitSampleFrequency));
                        //realTimeParam.LastAgit = realTimeParam.Agit;

                        if (!dicDOFilter.ContainsKey(insID))
                        {
                            KalmanFilter1D kalman = new KalmanFilter1D();
                            kalman.SetParameter(0.03, 0.1, 1.5, realTimeParam.DO);
                            dicDOFilter[insID] = kalman;
                        }
                        realTimeParam.RawDO = realTimeParam.DO;
                        realTimeParam.DO = Convert.ToSingle(dicDOFilter[insID].Update(realTimeParam.DO));
                        realTimeParam.DOPredict = Convert.ToSingle(dicDOFilter[insID].Predict());
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
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump1FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump1FlowCapacity;
                                }
                                else if (pumpIndex == 2)
                                {
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump2FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump2FlowCapacity;
                                }
                                else if (pumpIndex == 3)
                                {
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump3FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump3FlowCapacity;
                                }
                                else if (pumpIndex == 4)
                                {
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump4FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump4FlowCapacity;
                                }
                                else if (pumpIndex == 5)
                                {
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump5FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump5FlowCapacity;
                                }
                                else if (pumpIndex == 6)
                                {
                                    realTimeParam.AcidFlowSpeed = realTimeParam.Pump6FlowRate;
                                    realTimeParam.AcidFlowCapacity = realTimeParam.Pump6FlowCapacity;
                                }
                                break;
                            case PeristalticPump.BasePump:
                                if (pumpIndex == 1)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump1FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump1FlowCapacity;
                                }
                                else if (pumpIndex == 2)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump2FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump2FlowCapacity;
                                }
                                else if (pumpIndex == 3)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump3FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump3FlowCapacity;
                                }
                                else if (pumpIndex == 4)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump4FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump4FlowCapacity;
                                }
                                else if (pumpIndex == 5)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump5FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump5FlowCapacity;
                                }
                                else if (pumpIndex == 6)
                                {
                                    realTimeParam.BaseFlowSpeed = realTimeParam.Pump6FlowRate;
                                    realTimeParam.BaseFlowCapacity = realTimeParam.Pump6FlowCapacity;
                                }
                                break;
                            case PeristalticPump.FeedPump:
                                if (pumpIndex == 1)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump1FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump1FlowCapacity;
                                }
                                else if (pumpIndex == 2)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump2FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump2FlowCapacity;
                                }
                                else if (pumpIndex == 3)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump3FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump3FlowCapacity;
                                }
                                else if (pumpIndex == 4)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump4FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump4FlowCapacity;
                                }
                                else if (pumpIndex == 5)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump5FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump5FlowCapacity;
                                }
                                else if (pumpIndex == 6)
                                {
                                    realTimeParam.FeedFlowSpeed = realTimeParam.Pump6FlowRate;
                                    realTimeParam.FeedFlowCapacity = realTimeParam.Pump6FlowCapacity;
                                }
                                break;
                            case PeristalticPump.AFPump:
                                if (pumpIndex == 1)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump1FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump1FlowCapacity;
                                }
                                else if (pumpIndex == 2)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump2FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump2FlowCapacity;
                                }
                                else if (pumpIndex == 3)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump3FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump3FlowCapacity;
                                }
                                else if (pumpIndex == 4)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump4FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump4FlowCapacity;
                                }
                                else if (pumpIndex == 5)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump5FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump5FlowCapacity;
                                }
                                else if (pumpIndex == 6)
                                {
                                    realTimeParam.AFFlowSpeed = realTimeParam.Pump6FlowRate;
                                    realTimeParam.AFFlowCapacity = realTimeParam.Pump6FlowCapacity;
                                }
                                break;
                            case PeristalticPump.Feed2Pump:
                                if (pumpIndex == 1)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump1FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump1FlowCapacity;
                                }
                                else if (pumpIndex == 2)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump2FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump2FlowCapacity;
                                }
                                else if (pumpIndex == 3)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump3FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump3FlowCapacity;
                                }
                                else if (pumpIndex == 4)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump4FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump4FlowCapacity;
                                }
                                else if (pumpIndex == 5)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump5FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump5FlowCapacity;
                                }
                                else if (pumpIndex == 6)
                                {
                                    realTimeParam.Feed2FlowSpeed = realTimeParam.Pump6FlowRate;
                                    realTimeParam.Feed2FlowCapacity = realTimeParam.Pump6FlowCapacity;
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
                                    realTimeParam.AirFlowSpeed = realTimeParam.MFC1FlowRate;
                                    realTimeParam.AirFlowCapacity = realTimeParam.MFC1FlowCapacity;
                                }
                                else if (mfcIndex == 2)
                                {
                                    realTimeParam.AirFlowSpeed = realTimeParam.MFC2FlowRate;
                                    realTimeParam.AirFlowCapacity = realTimeParam.MFC2FlowCapacity;
                                }
                                else if (mfcIndex == 3)
                                {
                                    realTimeParam.AirFlowSpeed = realTimeParam.MFC3FlowRate;
                                    realTimeParam.AirFlowCapacity = realTimeParam.MFC3FlowCapacity;
                                }
                                else if (mfcIndex == 4)
                                {
                                    realTimeParam.AirFlowSpeed = realTimeParam.MFC4FlowRate;
                                    realTimeParam.AirFlowCapacity = realTimeParam.MFC4FlowCapacity;
                                }
                                else if (mfcIndex == 5)
                                {
                                    realTimeParam.AirFlowSpeed = realTimeParam.MFC5FlowRate;
                                    realTimeParam.AirFlowCapacity = realTimeParam.MFC5FlowCapacity;
                                }
                                break;
                            case GasType.O2:
                                if (mfcIndex == 1)
                                {
                                    realTimeParam.O2FlowSpeed = realTimeParam.MFC1FlowRate;
                                    realTimeParam.O2FlowCapacity = realTimeParam.MFC1FlowCapacity;
                                }
                                else if (mfcIndex == 2)
                                {
                                    realTimeParam.O2FlowSpeed = realTimeParam.MFC2FlowRate;
                                    realTimeParam.O2FlowCapacity = realTimeParam.MFC2FlowCapacity;
                                }
                                else if (mfcIndex == 3)
                                {
                                    realTimeParam.O2FlowSpeed = realTimeParam.MFC3FlowRate;
                                    realTimeParam.O2FlowCapacity = realTimeParam.MFC3FlowCapacity;
                                }
                                else if (mfcIndex == 4)
                                {
                                    realTimeParam.O2FlowSpeed = realTimeParam.MFC4FlowRate;
                                    realTimeParam.O2FlowCapacity = realTimeParam.MFC4FlowCapacity;
                                }
                                else if (mfcIndex == 5)
                                {
                                    realTimeParam.O2FlowSpeed = realTimeParam.MFC5FlowRate;
                                    realTimeParam.O2FlowCapacity = realTimeParam.MFC5FlowCapacity;
                                }
                                break;
                            case GasType.CO2:
                                if (mfcIndex == 1)
                                {
                                    realTimeParam.CO2FlowSpeed = realTimeParam.MFC1FlowRate;
                                    realTimeParam.CO2FlowCapacity = realTimeParam.MFC1FlowCapacity;
                                }
                                else if (mfcIndex == 2)
                                {
                                    realTimeParam.CO2FlowSpeed = realTimeParam.MFC2FlowRate;
                                    realTimeParam.CO2FlowCapacity = realTimeParam.MFC2FlowCapacity;
                                }
                                else if (mfcIndex == 3)
                                {
                                    realTimeParam.CO2FlowSpeed = realTimeParam.MFC3FlowRate;
                                    realTimeParam.CO2FlowCapacity = realTimeParam.MFC3FlowCapacity;
                                }
                                else if (mfcIndex == 4)
                                {
                                    realTimeParam.CO2FlowSpeed = realTimeParam.MFC4FlowRate;
                                    realTimeParam.CO2FlowCapacity = realTimeParam.MFC4FlowCapacity;
                                }
                                else if (mfcIndex == 5)
                                {
                                    realTimeParam.CO2FlowSpeed = realTimeParam.MFC5FlowRate;
                                    realTimeParam.CO2FlowCapacity = realTimeParam.MFC5FlowCapacity;
                                }
                                break;
                            case GasType.N2:
                                if (mfcIndex == 1)
                                {
                                    realTimeParam.N2FlowSpeed = realTimeParam.MFC1FlowRate;
                                    realTimeParam.N2FlowCapacity = realTimeParam.MFC1FlowCapacity;
                                }
                                else if (mfcIndex == 2)
                                {
                                    realTimeParam.N2FlowSpeed = realTimeParam.MFC2FlowRate;
                                    realTimeParam.N2FlowCapacity = realTimeParam.MFC2FlowCapacity;
                                }
                                else if (mfcIndex == 3)
                                {
                                    realTimeParam.N2FlowSpeed = realTimeParam.MFC3FlowRate;
                                    realTimeParam.N2FlowCapacity = realTimeParam.MFC3FlowCapacity;
                                }
                                else if (mfcIndex == 4)
                                {
                                    realTimeParam.N2FlowSpeed = realTimeParam.MFC4FlowRate;
                                    realTimeParam.N2FlowCapacity = realTimeParam.MFC4FlowCapacity;
                                }
                                else if (mfcIndex == 5)
                                {
                                    realTimeParam.N2FlowSpeed = realTimeParam.MFC5FlowRate;
                                    realTimeParam.N2FlowCapacity = realTimeParam.MFC5FlowCapacity;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex);

            }
            finally 
            {
                realTimeParam.ReactorName = insID;
            }
            return realTimeParam;
        }
        #endregion

        #region 0x0a 读MCU版本号
        public string GetMCUVersion(string insID, byte boardType)
        {
            string version = string.Empty;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RMCUVersion, CommandExtendId.Read);
            sendCommand.SetValue(ParamId_5L.RMCUVersion_Read_Type, boardType);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    version = recvCommand.GetString(ParamId_5L.RMCUVersion_ReadResponse_Version);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return version;
        }
        #endregion

        #region 0x0b 写电机调试 删除
        //public void SetMotorDebug(string insID, int motorNo, DebugMode debugMode)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.WMotorDebug, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.WMotorDebug_Write_MotorNo, (byte)motorNo);
        //        sendCommand.SetValue(ParamId_5L.WMotorDebug_Write_Mode, (byte)debugMode);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x0c 读写温控
        public TECParam GetTECPID(string insID)
        {
            TECParam param = new();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTempControlDebug, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.P = recvCommand.GetSingle(ParamId_5L.RWTempControlDebug_ReadWrite_P);
                    param.I = recvCommand.GetSingle(ParamId_5L.RWTempControlDebug_ReadWrite_I);
                    param.D = recvCommand.GetSingle(ParamId_5L.RWTempControlDebug_ReadWrite_D);
                    param.K = recvCommand.GetSingle(ParamId_5L.RWTempControlDebug_ReadWrite_K);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return param;
        }

        public void SetTECPID(string insID, TECParam tECParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTempControlDebug, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWTempControlDebug_ReadWrite_P, tECParam.P);
                sendCommand.SetValue(ParamId_5L.RWTempControlDebug_ReadWrite_I, tECParam.I);
                sendCommand.SetValue(ParamId_5L.RWTempControlDebug_ReadWrite_D, tECParam.D);
                sendCommand.SetValue(ParamId_5L.RWTempControlDebug_ReadWrite_K, tECParam.K);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x0d 读写设备参数
        DeviceParam ICommandWrapper.GetDeviceParam(string insID)
        {
            DeviceParam deviceParam = new DeviceParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWDeviceParam, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    deviceParam.MainIpAdress = recvCommand.GetBytes(ParamId_5L.RWDeviceParam_ReadWrite_MainIPAdress);
                    deviceParam.MainPort = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWDeviceParam_ReadWrite_MainPort));
                    deviceParam.MainGateway = recvCommand.GetBytes(ParamId_5L.RWDeviceParam_ReadWrite_MainGateway);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return deviceParam;
        }

        public void SetDeviceParam(string insID, DeviceParam deviceParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWDeviceParam, CommandExtendId.Write);
            try
            {
                byte[] bytes = new byte[4];
                var array = deviceParam.MainAdress.Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    bytes[i] = (byte)Convert.ToInt32(array[i]);
                }
                deviceParam.MainIpAdress = bytes;
                array = deviceParam.Gateway.Split('.');
                byte[] bytes1 = new byte[4];
                for (int i = 0; i < array.Length; i++)
                {
                    bytes1[i] = (byte)Convert.ToInt32(array[i]);
                }
                deviceParam.MainGateway = bytes1;
                array = deviceParam.WifiAdress.Split('.');
                byte[] bytes2 = new byte[4];
                for (int i = 0; i < array.Length; i++)
                {
                    bytes2[i] = (byte)Convert.ToInt32(array[i]);
                }
                deviceParam.WifiIpAdress = bytes2;
                array = deviceParam.Gateway1.Split('.');
                byte[] bytes3 = new byte[4];
                for (int i = 0; i < array.Length; i++)
                {
                    bytes3[i] = (byte)Convert.ToInt32(array[i]);
                }
                deviceParam.WifiGateway = bytes3;
                sendCommand.SetValue(ParamId_5L.RWDeviceParam_ReadWrite_MainIPAdress, deviceParam.MainIpAdress);
                sendCommand.SetValue(ParamId_5L.RWDeviceParam_ReadWrite_MainPort, BitConverter.GetBytes(deviceParam.MainPort));
                sendCommand.SetValue(ParamId_5L.RWDeviceParam_ReadWrite_MainGateway, deviceParam.MainGateway);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x0e 读写传感器校准
        public SensorCorrectParam GetSensorCorrect(string insID, byte sensorType)
        {
            SensorCorrectParam sensorCorrectParam = new();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSensorCorrect, CommandExtendId.Read);
            sendCommand.SetValue(ParamId_5L.RWSensorCorrect_Read_SensorType, sensorType);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    sensorCorrectParam.SensorType = (SensorType)recvCommand.GetByte(ParamId_5L.RWSensorCorrect_ReadResponse_SensorType);
                    sensorCorrectParam.Bias = recvCommand.GetSingle(ParamId_5L.RWSensorCorrect_ReadResponse_SensorBias);
                    sensorCorrectParam.Coefficient = recvCommand.GetSingle(ParamId_5L.RWSensorCorrect_ReadResponse_SensorCoefficient);
                    sensorCorrectParam.StatusCode = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWSensorCorrect_ReadResponse_StatusCode));
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return sensorCorrectParam;
        }

        public void SetSensorCorrect(string insID, SensorCorrectParam sensorCorrectParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSensorCorrect, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWSensorCorrect_Write_SensorType, (byte)sensorCorrectParam.SensorType_5L);
                sendCommand.SetValue(ParamId_5L.RWSensorCorrect_Write_CorrectMode, (byte)sensorCorrectParam.CorrectMode);
                sendCommand.SetValue(ParamId_5L.RWSensorCorrect_Write_CorrectValue, sensorCorrectParam.CorrectValue);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x0f 读写DO传感器校准  删除
        //public Tuple<int,float> GetDOSensorCorrect(string insID)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWDOSensorCorrect, CommandExtendId.Read);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            byte[] bytes = recvCommand.GetBytes(ParamId_5L.RWDOSensorCorrect_ReadResponse_Status);
        //            Array.Reverse(bytes);
        //            int status = BitConverter.ToInt32(bytes);
        //            float coefficient = recvCommand.GetSingle(ParamId_5L.RWDOSensorCorrect_ReadResponse_SensorCoefficient);
        //            return Tuple.Create(status, coefficient);
        //        }
        //        else
        //        {
        //            return Tuple.Create(0, 0f);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");
        //    }
        //    return Tuple.Create(0, 0f);
        //}

        //public void SetDOSensorCorrect(string insID, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWDOSensorCorrect, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWDOSensorCorrect_Write_CorrectMode, (byte)mode);
        //        sendCommand.SetValue(ParamId_5L.RWDOSensorCorrect_Write_CorrectParam, (byte)twoPointCorrectParam);
        //        sendCommand.SetValue(ParamId_5L.RWDOSensorCorrect_Write_CorrectValue, calitrationValue);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x10 读写称重传感器校准  删除
        //public Tuple<float, float> GetWeightSensorCorrect(string insID, int sensorNo)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWWeightSensorCorrect, CommandExtendId.Read);
        //    sendCommand.SetValue(ParamId_5L.RWWeightSensorCorrect_Read_SensorKind, (byte)sensorNo);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            float bias = recvCommand.GetSingle(ParamId_5L.RWWeightSensorCorrect_ReadResponse_SensorBias);
        //            float coefficient = recvCommand.GetSingle(ParamId_5L.RWWeightSensorCorrect_ReadResponse_SensorCoefficient);
        //            return Tuple.Create(bias, coefficient);
        //        }
        //        else
        //        {
        //            return Tuple.Create(0f, 0f);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}"); 
        //    }
        //    return default;
        //}

        //public void SetWeightSensorCorrect(string insID, int sensorNo, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWWeightSensorCorrect, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWWeightSensorCorrect_Write_SensorNo, (byte)sensorNo);
        //        sendCommand.SetValue(ParamId_5L.RWWeightSensorCorrect_Write_CorrectMode, (byte)mode);
        //        sendCommand.SetValue(ParamId_5L.RWWeightSensorCorrect_Write_CorrectParam, (byte)twoPointCorrectParam);
        //        sendCommand.SetValue(ParamId_5L.RWWeightSensorCorrect_Write_CorrectValue, calitrationValue);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x11 读写PT100校准  删除
        //public Tuple<float, float> GetPT100SensorCorrect(string insID)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWPT100SensorCorrect, CommandExtendId.Read);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            float bias = recvCommand.GetSingle(ParamId_5L.RWPT100SensorCorrect_ReadResponse_SensorBias);
        //            float coefficient = recvCommand.GetSingle(ParamId_5L.RWPT100SensorCorrect_ReadResponse_SensorCoefficient);
        //            return Tuple.Create(bias, coefficient);
        //        }
        //        else
        //        {
        //            return Tuple.Create(0f, 0f);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //    return default;
        //}

        //public void SetPT100SensorCorrect(string insID, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWPT100SensorCorrect, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWPT100SensorCorrect_Write_CorrectMode, (byte)mode);
        //        sendCommand.SetValue(ParamId_5L.RWPT100SensorCorrect_Write_CorrectParam, (byte)twoPointCorrectParam);
        //        sendCommand.SetValue(ParamId_5L.RWPT100SensorCorrect_Write_CorrectValue, calitrationValue);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x12 读写蠕动泵校准
        public float GetPeristalticPumpCorrect(string insID, int pumpNo)
        {
            float coefficient = -1;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpCorrect, CommandExtendId.Read);
            sendCommand.SetValue(ParamId_5L.RWPeristalticPumpCorrect_Read_PeristalticPumpNo, (byte)pumpNo);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    coefficient = recvCommand.GetSingle(ParamId_5L.RWPeristalticPumpCorrect_ReadResponse_PeristalticPumpCoefficient);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return coefficient;
        }

        public void SetPeristalticPumpCorrect(string insID, int pumpNo, int calitrationParam, float calitrationValue)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpCorrect, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpCorrect_Write_PeristalticPumpNo, (byte)pumpNo);
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpCorrect_Write_CorrectParam, (byte)calitrationParam);
                sendCommand.SetValue(ParamId_5L.RWPeristalticPumpCorrect_Write_CorrectValue, calitrationValue);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x13 读写蠕动泵对应功能配置  删除
        //public PeristalticPump[] GetPeristalticPumpSetting(string insID)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpSetting, CommandExtendId.Read);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            var pump1 = (PeristalticPump)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump1Setting);
        //            var pump2 = (PeristalticPump)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump2Setting);
        //            var pump3 = (PeristalticPump)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump3Setting);
        //            var pump4 = (PeristalticPump)recvCommand.GetByte(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump4Setting);
        //            return new PeristalticPump[] { pump1, pump2, pump3, pump4 };
        //        }
        //        else
        //        {
        //            return new PeristalticPump[] { PeristalticPump.None, PeristalticPump.None, PeristalticPump.None, PeristalticPump.None };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //    return default;
        //}

        //public void SetPeristalticSetting(string insID, PeristalticPump[] pumps)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWPeristalticPumpSetting, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump1Setting, (byte)pumps[0]);
        //        sendCommand.SetValue(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump2Setting, (byte)pumps[1]);
        //        sendCommand.SetValue(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump3Setting, (byte)pumps[2]);
        //        sendCommand.SetValue(ParamId_5L.RWPeristalticPumpSetting_ReadWrite_Pump4Setting, (byte)pumps[3]);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x14 读写MFC对应功能配置  删除
        //public GasType[] GetMFCSetting(string insID)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWMFCSetting, CommandExtendId.Read);
        //    try
        //    {
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
        //        {
        //            var item1 = (GasType)recvCommand.GetByte(ParamId_5L.RWMFCSetting_ReadWrite_MFC1Setting);
        //            var item2 = (GasType)recvCommand.GetByte(ParamId_5L.RWMFCSetting_ReadWrite_MFC2Setting);
        //            return new GasType[] { item1, item2 };
        //        }
        //        else
        //        {
        //            return new GasType[] { GasType.Air, GasType.Air };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //    return default;
        //}

        //public void SetMFCSetting(string insID, GasType[] gases)
        //{
        //    SendCommand sendCommand = new SendCommand(CommandId_5L.RWMFCSetting, CommandExtendId.Write);
        //    try
        //    {
        //        sendCommand.SetValue(ParamId_5L.RWMFCSetting_ReadWrite_MFC1Setting, (byte)gases[0]);
        //        sendCommand.SetValue(ParamId_5L.RWMFCSetting_ReadWrite_MFC2Setting, (byte)gases[1]);
        //        RecvCommand recvCommand = Send(insID, sendCommand);
        //        if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
        //        {
        //            throw new Exception("设置失败");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // 获取当前方法名并记录日志
        //        var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
        //        LogHelper.Debug($"Error in method {methodName}: {ex}");

        //    }
        //}
        #endregion

        #region 0x15 写流量清零
        public void SetResetFlowCapacity(string insID, ClearModule clearModule, int serialNo = 1)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.WCleanFlowCapacity, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.WCleanFlowCapacity_Write_Type, (byte)clearModule);
                sendCommand.SetValue(ParamId_5L.WCleanFlowCapacity_Write_No, (byte)serialNo);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x16 读写声光报警
        AlarmParam ICommandWrapper.GetSoundLightAlarm(string insID)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSoundLightAlarm, CommandExtendId.Read);
            AlarmParam param = new();
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.RedLightEnable = (SwitchMode)recvCommand.GetByte(ParamId_5L.RWSoundLightAlarm_ReadWrite_RedLightStatus);
                    param.GreenLightEnable = (SwitchMode)recvCommand.GetByte(ParamId_5L.RWSoundLightAlarm_ReadWrite_GreenLightStatus);
                    param.BlueLightEnable = (SwitchMode)recvCommand.GetByte(ParamId_5L.RWSoundLightAlarm_ReadWrite_BlueLightStatus);
                    param.BuzzerEnable = (SwitchMode)recvCommand.GetByte(ParamId_5L.RWSoundLightAlarm_ReadWrite_BuzzerStatus);
                    return param;
                }
                else
                {
                    return param;
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
            return param;
        }

        void ICommandWrapper.SetSoundLightAlarm(string insID, AlarmParam alarmParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWSoundLightAlarm, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWSoundLightAlarm_ReadWrite_RedLightStatus, (byte)alarmParam.RedLightEnable);
                sendCommand.SetValue(ParamId_5L.RWSoundLightAlarm_ReadWrite_GreenLightStatus, (byte)alarmParam.GreenLightEnable);
                sendCommand.SetValue(ParamId_5L.RWSoundLightAlarm_ReadWrite_BlueLightStatus, (byte)alarmParam.BlueLightEnable);
                sendCommand.SetValue(ParamId_5L.RWSoundLightAlarm_ReadWrite_BuzzerStatus, (byte)alarmParam.BuzzerEnable);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x17 读写时间同步

        public (System.DateTime, TimeSpan) GetTimeSync(string insID)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTimeSync, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    int year = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Year));
                    int month = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Month));
                    int day = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Day));
                    int hour = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Hour));
                    int minute = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Minute));
                    int second = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_Second));
                    int hour1 = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_RunningHour));
                    int minute1 = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_RunningMinute));
                    int second1 = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWTimeSync_ReadWrite_RunningSecond));
                    return (new DateTime(year, month, day, hour, minute, second), new TimeSpan(hour1, minute1, second1));
                }
                else
                {
                    return (DateTime.MinValue, TimeSpan.Zero);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
            return (DateTime.MinValue, TimeSpan.Zero);
        }

        public void SetTimeSync(string insID, System.DateTime dateTime, TimeSpan timeSpan)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWTimeSync, CommandExtendId.Write);
            try
            {
                byte[] array1 = BitConverter.GetBytes(dateTime.Year);
                //Array.Reverse(array1);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Year, array1);
                byte[] array2 = BitConverter.GetBytes(dateTime.Month);
                //Array.Reverse(array2);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Month, array2);
                byte[] array3 = BitConverter.GetBytes(dateTime.Day);
                //Array.Reverse(array3);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Day, array3);
                byte[] array4 = BitConverter.GetBytes(dateTime.Hour);
                //Array.Reverse(array4);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Hour, array4);
                byte[] array5 = BitConverter.GetBytes(dateTime.Minute);
                //Array.Reverse(array5);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Minute, array5);
                byte[] array6 = BitConverter.GetBytes(dateTime.Second);
                //Array.Reverse(array6);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_Second, array6);
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_RunningHour, BitConverter.GetBytes(timeSpan.Hours));
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_RunningMinute, BitConverter.GetBytes(timeSpan.Minutes));
                sendCommand.SetValue(ParamId_5L.RWTimeSync_ReadWrite_RunningSecond, BitConverter.GetBytes(timeSpan.Seconds));
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x18 写配置
        public void SetSettingSync(string insID, ScreenParam param)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.WSettingSync, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.WSettingSync_Write_PH, param.PH);
                sendCommand.SetValue(ParamId_5L.WSettingSync_Write_PHAuto, param.PhAuto == true ? 0x01 : 0x00);
                sendCommand.SetValue(ParamId_5L.WSettingSync_Write_DO, param.DO);
                sendCommand.SetValue(ParamId_5L.WSettingSync_Write_DOAuto, param.DOAuto == true ? 0x01 : 0x00);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x19 读写冷凝控制
        public CondensationParam GetCondensationControl(string insID)
        {
            CondensationParam condensationParam = new CondensationParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWCondensationControl, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    condensationParam.Enable = recvCommand.GetByte(ParamId_5L.RWCondensationControl_ReadWrite_Enable) == 0x01 ? true : false;
                    condensationParam.Temp = recvCommand.GetSingle(ParamId_5L.RWCondensationControl_ReadWrite_Temp);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
            return condensationParam;
        }

        public void SetCondensationControl(string insID, CondensationParam param)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWCondensationControl, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWCondensationControl_ReadWrite_Enable, param.Enable == true ? 0x01 : 0x00);
                sendCommand.SetValue(ParamId_5L.RWCondensationControl_ReadWrite_Temp, param.Temp);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x1a 读尾气信息
        public OffGasParam GetOffGas(string insID)
        {
            OffGasParam param = new OffGasParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.ROffgas, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    param.CO2 = recvCommand.GetSingle(ParamId_5L.ROffgas_ReadResponse_CO2);
                    param.O2 = recvCommand.GetSingle(ParamId_5L.ROffgas_ReadResponse_O2);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
            return param;
        }
        #endregion

        #region 0x1b 读写程序下载信息
        public MCUDownloadStatus GetMCUDownloadInfo(string insID)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMCUDownload, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    return (MCUDownloadStatus)recvCommand.GetByte(ParamId_5L.RWMCUDownload_ReadResponse_Status);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
            return MCUDownloadStatus.Idle;
        }

        public void SetMCUDownloadInfo(string insID, byte boardType)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMCUDownload, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWMCUDownload_Write_No, boardType);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
        }
        #endregion

        #region 0x1c 写部件恢复默认
        public void SetResetDefaultSetting(string insID, ClearModule clearModule, int serialNo = 1)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.WResetDefaultSetting, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.WResetDefaultSetting_Write_No, (byte)serialNo);
                sendCommand.SetValue(ParamId_5L.WResetDefaultSetting_Write_Type, (byte)clearModule);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");
            }
        }
        #endregion

        #region 0x1d 读写程序下载地址
        public DeviceParam GetMCUDownloadAdress(string insID)
        {
            DeviceParam deviceParam = new DeviceParam();
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMCUDownloadAdress, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    deviceParam.MainIpAdress = recvCommand.GetBytes(ParamId_5L.RWMCUDownloadAdress_ReadWrite_IPAdress);
                    deviceParam.MainPort = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWMCUDownloadAdress_ReadWrite_Port));
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return deviceParam;
        }

        public void SetMCUDownloadAdress(string insID, DeviceParam deviceParam)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMCUDownloadAdress, CommandExtendId.Write);
            try
            {
                byte[] bytes = new byte[4];
                var array = deviceParam.MainAdress.Split('.');
                for (int i = 0; i < array.Length; i++)
                {
                    bytes[i] = (byte)Convert.ToInt32(array[i]);
                }
                deviceParam.MainIpAdress = bytes;
                sendCommand.SetValue(ParamId_5L.RWMCUDownloadAdress_ReadWrite_IPAdress, deviceParam.MainIpAdress);
                sendCommand.SetValue(ParamId_5L.RWMCUDownloadAdress_ReadWrite_Port, BitConverter.GetBytes(deviceParam.MainPort));
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x1e 读写搅拌电机型号
        public int GetStirringMotorType(string insID)
        {
            int index = -1;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWStirringMotorType, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    index = BitConverter.ToInt32(recvCommand.GetBytes(ParamId_5L.RWStirringMotorType_ReadWrite_MotorType));
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return index;
        }

        public void SetStirringMotorType(string insID, int index)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWStirringMotorType, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWStirringMotorType_ReadWrite_MotorType, BitConverter.GetBytes(index));
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x1f 读写EPC压力
        public float GetEPCPressure(string insID)
        {
            float pressure = 0;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWEPCPressure, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    pressure = recvCommand.GetSingle(ParamId_5L.RWEPCPressure_ReadWrite_Pressure);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return pressure;
        }

        public void SetEPCPressure(string insID, float pressure)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWEPCPressure, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWEPCPressure_ReadWrite_Pressure, pressure);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x20 读写设置磁吸底座状态
        public byte GetMagneticBase(string insID)
        {
            byte status = 0xff;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMagneticBaseStatus, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    status = recvCommand.GetByte(ParamId_5L.RWMagneticBaseStatus_ReadWrite_ControlStatus);
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return status;
        }

        public void SetMagneticBase(string insID, byte status)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWMagneticBaseStatus, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWMagneticBaseStatus_ReadWrite_ControlStatus, status);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion

        #region 0x21 读写PT100位置检测配置

        public bool GetPT100LocationCheckSetting(string insID)
        {
            bool isEnabled = false;
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPT100LocationCheckSetting, CommandExtendId.Read);
            try
            {
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() == CommandExtendId.ReadResponse)
                {
                    isEnabled = recvCommand.GetByte(ParamId_5L.RWPT100LocationCheckSetting_ReadWrite_Enable) == 0x00 ? false : true;
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
            return isEnabled;
        }

        public void SetPT100LocationCheckSetting(string insID, bool isEnabled)
        {
            SendCommand sendCommand = new SendCommand(CommandId_5L.RWPT100LocationCheckSetting, CommandExtendId.Write);
            try
            {
                sendCommand.SetValue(ParamId_5L.RWPT100LocationCheckSetting_ReadWrite_Enable, isEnabled == true ? 0x01 : 0x00);
                RecvCommand recvCommand = Send(insID, sendCommand);
                if (recvCommand.GetExtCode() != CommandExtendId.WriteResponse)
                {
                    throw new Exception("设置失败");
                }
            }
            catch (Exception ex)
            {
                // 获取当前方法名并记录日志
                var methodName = new StackTrace().GetFrame(0).GetMethod().Name;
                LogHelper.Debug($"Error in method {methodName}: {ex}");

            }
        }
        #endregion
    }
}
