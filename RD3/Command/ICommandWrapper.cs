using Fpi.Communication.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public interface ICommandWrapper
    {
        #region 0x01
        /// <summary>
        /// 读取温度设置
        /// </summary>
        TempParam GetTempSetting(string insID);

        /// <summary>
        /// 写入温度设置
        /// </summary>
        void SetTempSetting(string insID, TempParam tempParam);
        #endregion

        #region 0x02
        /// <summary>
        /// 读取搅拌速度
        /// </summary>
        int GetAgitSpeed(string insID);

        /// <summary>
        /// 写入搅拌速度
        /// </summary>
        void SetAgitSpeed(string insID, int speed);
        #endregion

        #region 0x03
        /// <summary>
        /// 读取蠕动泵控制参数
        /// </summary>
        PeristalticPumpControlParam GetPeristalticPumpControlParam(string insID, int pumpNo);

        /// <summary>
        /// 写入蠕动泵控制参数
        /// </summary>
        void SetPeristalticPumpControlParam(string insID, PeristalticPumpControlParam peristalticPumpControlParam);
        #endregion

        #region 0x04
        /// <summary>
        /// 读取自动除泡设置
        /// </summary>
        DefoamingParam GetAutoDefoamingSetting(string insID);

        /// <summary>
        /// 写入自动除泡设置
        /// </summary>
        void SetAutoDefoamingSetting(string insID, DefoamingParam param);
        #endregion

        #region 0x05
        /// <summary>
        /// 读取气体速度设置
        /// </summary>
        GasParam GetGasSpeed(string insID, GasParam param);

        /// <summary>
        /// 写入气体速度设置
        /// </summary>
        void SetGasSpeed(string insID, GasParam param);
        #endregion

        #region 0x06
        /// <summary>
        /// 读取MCU传感器类型设置
        /// </summary>
        Tuple<int, int> GetMCUSensorTypeSetting(string insID, int sensorType);

        /// <summary>
        /// 写入MCU传感器类型设置
        /// </summary>
        void SetMCUSensorTypeSetting(string insID, int sensorType, int signal);
        #endregion

        #region 0x08  删除
        /// <summary>
        /// 读取功能控制模式
        /// </summary>
        //SwitchMode GetFunctionControlMode(string insID, ControlObject controlObject);

        /// <summary>
        /// 写入功能控制模式
        /// </summary>
        //void SetFunctionControlMode(string insID, ControlObject controlObject, SwitchMode controlMode);
        #endregion

        #region 0x09
        /// <summary>
        /// 读取实时参数
        /// </summary>
        RealTimeParam GetRealTime(string insID);
        #endregion

        #region 0x0a
        /// <summary>
        /// 读取MCU版本信息
        /// </summary>
        string GetMCUVersion(string insID, byte boardType);
        #endregion

        #region 0x0b  删除
        /// <summary>
        /// 写入电机调试设置
        /// </summary>
        //void SetMotorDebug(string insID, int motorNo, DebugMode debugMode);
        #endregion

        #region 0x0c
        /// <summary>
        /// 读取TEC PID设置
        /// </summary>
        TECParam GetTECPID(string insID);

        /// <summary>
        /// 写入TEC PID设置
        /// </summary>
        void SetTECPID(string insID, TECParam tECParam);
        #endregion

        #region 0x0d
        /// <summary>
        /// 读取设备参数
        /// </summary>
        DeviceParam GetDeviceParam(string insID);

        /// <summary>
        /// 写入设备参数
        /// </summary>
        void SetDeviceParam(string insID, DeviceParam deviceParam);
        #endregion

        #region 0x0e
        /// <summary>
        /// 读取传感器校准设置
        /// </summary>
        SensorCorrectParam GetSensorCorrect(string insID, byte sensorType);

        /// <summary>
        /// 写入PH传感器校准设置
        /// </summary>
        void SetSensorCorrect(string insID, SensorCorrectParam sensorCorrectParam);
        #endregion

        #region 0x0f  删除
        /// <summary>
        /// 读取DO传感器校准设置
        /// </summary>
        //Tuple<int,float> GetDOSensorCorrect(string insID);

        ///// <summary>
        ///// 写入DO传感器校准设置
        ///// </summary>
        //void SetDOSensorCorrect(string insID, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue);
        #endregion

        #region 0x10  删除
        ///// <summary>
        ///// 读取重量传感器校准设置
        ///// </summary>
        //Tuple<float, float> GetWeightSensorCorrect(string insID,int sensorNo);

        ///// <summary>
        ///// 写入重量传感器校准设置
        ///// </summary>
        //void SetWeightSensorCorrect(string insID, int sensorNo, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue);
        #endregion

        #region 0x11  删除
        ///// <summary>
        ///// 读取PT100传感器校准设置
        ///// </summary>
        //Tuple<float, float> GetPT100SensorCorrect(string insID);

        ///// <summary>
        ///// 写入PT100传感器校准设置
        ///// </summary>
        //void SetPT100SensorCorrect(string insID, SensorCorrectMode mode, TwoPointCorrectParam twoPointCorrectParam, float calitrationValue);
        #endregion

        #region 0x12
        /// <summary>
        /// 读取蠕动泵校准参数
        /// </summary>
        float GetPeristalticPumpCorrect(string insID, int pumpNo);

        /// <summary>
        /// 写入蠕动泵校准参数
        /// </summary>
        void SetPeristalticPumpCorrect(string insID, int pumpNo, int calitrationParam, float calitrationValue);
        #endregion

        #region 0x13  删除
        ///// <summary>
        ///// 读取蠕动泵设置
        ///// </summary>
        //PeristalticPump[] GetPeristalticPumpSetting(string insID);

        ///// <summary>
        ///// 写入蠕动泵设置
        ///// </summary>
        //void SetPeristalticSetting(string insID, PeristalticPump[] pumps);
        #endregion

        #region 0x14  删除
        ///// <summary>
        ///// 读取MFC气体流量计设置
        ///// </summary>
        //GasType[] GetMFCSetting(string insID);

        ///// <summary>
        ///// 写入MFC气体流量计设置
        ///// </summary>
        //void SetMFCSetting(string insID, GasType[] gases);
        #endregion

        #region 0x15
        /// <summary>
        /// 写入重置流量容量
        /// </summary>
        void SetResetFlowCapacity(string insID, ClearModule clearModule, int serialNo = 1);
        #endregion

        #region 0x16

        /// <summary>
        /// 读取声光报警
        /// </summary>
        AlarmParam GetSoundLightAlarm(string insID);

        /// <summary>
        /// 写入声光报警
        /// </summary>
        void SetSoundLightAlarm(string insID, AlarmParam alarmParam);
        #endregion

        #region 0x17

        /// <summary>
        /// 读取时间同步
        /// </summary>
        (DateTime,TimeSpan) GetTimeSync(string insID);

        /// <summary>
        /// 写入时间同步
        /// </summary>
        void SetTimeSync(string insID, DateTime dateTime, TimeSpan timeSpan);
        #endregion

        #region 0x18
        /// <summary>
        /// 写入配置同步
        /// </summary>
        void SetSettingSync(string insID, ScreenParam param);
        #endregion

        #region 0x19 读写冷凝控制
        CondensationParam GetCondensationControl(string insID);

        void SetCondensationControl(string insID, CondensationParam param);
        #endregion

        #region 0x1a 读尾气信息
        OffGasParam GetOffGas(string insID);
        #endregion

        #region 0x1b 读写程序下载信息
        MCUDownloadStatus GetMCUDownloadInfo(string insID);

        void SetMCUDownloadInfo(string insID, byte boardType);
        #endregion

        #region 0x1c 写部件恢复默认
        void SetResetDefaultSetting(string insID, ClearModule clearModule, int serialNo = 1);
        #endregion

        #region 0x1d 读写程序下载地址
        DeviceParam  GetMCUDownloadAdress(string insID);

        void SetMCUDownloadAdress(string insID, DeviceParam deviceParam);
        #endregion

        #region 0x1e 读写搅拌电机型号
        int GetStirringMotorType(string insID);

        void SetStirringMotorType(string insID, int index);
        #endregion

        #region 0x1e 读写EPC压力
        float GetEPCPressure(string insID);

        void SetEPCPressure(string insID, float pressure);
        #endregion

        #region 0x20 读写设置磁吸底座状态
        byte GetMagneticBase(string insID);

        void SetMagneticBase(string insID, byte status);
        #endregion

        #region 0x21 读写PT100位置检测配置
        bool GetPT100LocationCheckSetting(string insID);

        void SetPT100LocationCheckSetting(string insID, bool isEnabled);
        #endregion
    }
}
