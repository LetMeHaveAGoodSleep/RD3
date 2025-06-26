using System;
using System.Collections.Generic;
using System.Text;

namespace RD3
{
    /// <summary>
    /// 通讯编号(ID)
    /// </summary>
    public class CommandId
    {
        /// <summary>
        /// 读写温度
        /// </summary>
        public const string RWTempParam = "RWTempParam";

        /// <summary>
        /// 读写转速
        /// </summary>
        public const string RWAgitParam = "RWAgitParam";

        /// <summary>
        /// 读写蠕动泵控制
        /// </summary>
        public const string RWPeristalticPumpControl = "RWPeristalticPumpControl";

        /// <summary>
        /// 读写自动消泡控制
        /// </summary>
        public const string RWAutoDefoamingControl = "RWAutoDefoamingControl";

        /// <summary>
        /// 读写MFC控制
        /// </summary>
        public const string RWMFCControl = "RWMFCControl";

        /// <summary>
        /// 读写传感器配置
        /// </summary>
        public const string RWSensorSetting = "RWSensorSetting";

        /// <summary>
        /// 读仪器实时信息
        /// </summary>
        public const string RRealtimeParam = "RRealtimeParam";

        /// <summary>
        /// 读MCU版本号
        /// </summary>
        public const string RMCUVersion = "RMCUVersion";

        /// <summary>
        /// 读写温控调试
        /// </summary>
        public const string RWTempControlDebug = "RWTempControlDebug";

        /// <summary>
        /// 读写设备参数
        /// </summary>
        public const string RWDeviceParam = "RWDeviceParam";

        /// <summary>
        /// 读写传感器校准
        /// </summary>
        public const string RWSensorCorrect = "RWSensorCorrect";

        /// <summary>
        /// 读写蠕动泵校准
        /// </summary>
        public const string RWPeristalticPumpCorrect = "RWPeristalticPumpCorrect";

        /// <summary>
        /// 写流量清零
        /// </summary>
        public const string WCleanFlowCapacity = "WCleanFlowCapacity";

        /// <summary>
        /// 读写声光报警
        /// </summary>
        public const string RWSoundLightAlarm = "RWSoundLightAlarm";

        /// <summary>
        /// 读写时间同步
        /// </summary>
        public const string RWTimeSync = "RWTimeSync";

        /// <summary>
        /// 写配置同步
        /// </summary>
        public const string WSettingSync = "WSettingSync";

        /// <summary>
        /// 读写冷凝控制
        /// </summary>
        public const string RWCondensationControl = "RWCondensationControl";

        /// <summary>
        /// 读尾气信息
        /// </summary>
        public const string ROffgas = "ROffgas";

        /// <summary>
        /// 读写MCU程序下载
        /// </summary>
        public const string RWMCUDownload = "RWMCUDownload";

        /// <summary>
        /// 写部件恢复默认设置
        /// </summary>
        public const string WResetDefaultSetting = "WResetDefaultSetting";

        /// <summary>
        /// 读写MCU程序下载地址
        /// </summary>
        public const string RWMCUDownloadAdress = "RWMCUDownloadAdress";

        /// <summary>
        /// 读写搅拌电机型号
        /// </summary>
        public const string RWStirringMotorType = "RWStirringMotorType";


    }


    /// <summary>
    /// 通讯参数编号(ID)
    /// </summary>
    public class ParamId
    {
        /// <summary>
        /// 使能
        /// </summary>
        public const int RWTempParam_ReadWrite_Enable = 0xFF00;

        /// <summary>
        /// 温度
        /// </summary>
        public const int RWTempParam_ReadWrite_Temp = 0xFF01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWTempParam_WriteResponse_ResponseValue = 0x8800;

        /// <summary>
        /// 转速
        /// </summary>
        public const int RWAgitParam_ReadWrite_Agit = 0x1FF00;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWAgitParam_WriteResponse_ResponseValue = 0x18800;

        /// <summary>
        /// 泵编号
        /// </summary>
        public const int RWPeristalticPumpControl_Read_SerialNo = 0x25500;

        /// <summary>
        /// 泵编号
        /// </summary>
        public const int RWPeristalticPumpControl_ReadWrite_SerialNo = 0x2FF00;

        /// <summary>
        /// 控制模式
        /// </summary>
        public const int RWPeristalticPumpControl_ReadWrite_ControlMode = 0x2FF01;

        /// <summary>
        /// 流速
        /// </summary>
        public const int RWPeristalticPumpControl_ReadWrite_FlowSpeed = 0x2FF02;

        /// <summary>
        /// 流量
        /// </summary>
        public const int RWPeristalticPumpControl_ReadWrite_FlowCapacity = 0x2FF03;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWPeristalticPumpControl_WriteResponse_ResponseValue = 0x28800;

        /// <summary>
        /// 使能
        /// </summary>
        public const int RWAutoDefoamingControl_ReadWrite_Enable = 0x3FF00;

        /// <summary>
        /// 泵编号
        /// </summary>
        public const int RWAutoDefoamingControl_ReadWrite_PumpNo = 0x3FF01;

        /// <summary>
        /// 泵流速
        /// </summary>
        public const int RWAutoDefoamingControl_ReadWrite_FlowRate = 0x3FF02;

        /// <summary>
        /// 检测周期
        /// </summary>
        public const int RWAutoDefoamingControl_ReadWrite_CheckCycle = 0x3FF03;

        /// <summary>
        /// 占空比
        /// </summary>
        public const int RWAutoDefoamingControl_ReadWrite_DutyCycle = 0x3FF04;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWAutoDefoamingControl_WriteResponse_ResponseValue = 0x38800;

        /// <summary>
        /// MFC编号
        /// </summary>
        public const int RWMFCControl_Read_SerialNo = 0x45500;

        /// <summary>
        /// MFC编号
        /// </summary>
        public const int RWMFCControl_ReadWrite_SerialNo = 0x4FF00;

        /// <summary>
        /// 流速
        /// </summary>
        public const int RWMFCControl_ReadWrite_FlowSpeed = 0x4FF01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWMFCControl_WriteResponse_ResponseValue = 0x48800;

        /// <summary>
        /// 传感器种类
        /// </summary>
        public const int RWSensorSetting_ReadWrite_Kind = 0x5FF00;

        /// <summary>
        /// 传感器类型
        /// </summary>
        public const int RWSensorSetting_ReadWrite_Type = 0x5FF01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWSensorSetting_WriteResponse_ResponseValue = 0x58800;

        /// <summary>
        /// 温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Temp = 0x6AA00;

        /// <summary>
        /// PH
        /// </summary>
        public const int RRealtimeParam_ReadResponse_PH = 0x6AA01;

        /// <summary>
        /// 溶氧
        /// </summary>
        public const int RRealtimeParam_ReadResponse_DO = 0x6AA02;

        /// <summary>
        /// 转速
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Agit = 0x6AA03;

        /// <summary>
        /// 泵1流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump1FlowRate = 0x6AA04;

        /// <summary>
        /// 泵1流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump1Flow = 0x6AA05;

        /// <summary>
        /// 泵1总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump1FlowTotal = 0x6AA06;

        /// <summary>
        /// 泵2流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump2FlowRate = 0x6AA07;

        /// <summary>
        /// 泵2流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump2Flow = 0x6AA08;

        /// <summary>
        /// 泵2总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump2FlowTotal = 0x6AA09;

        /// <summary>
        /// 泵3流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump3FlowRate = 0x6AA0A;

        /// <summary>
        /// 泵3流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump3Flow = 0x6AA0B;

        /// <summary>
        /// 泵3总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump3FlowTotal = 0x6AA0C;

        /// <summary>
        /// 泵4流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump4FlowRate = 0x6AA0D;

        /// <summary>
        /// 泵4流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump4Flow = 0x6AA0E;

        /// <summary>
        /// 泵4总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump4FlowTotal = 0x6AA0F;

        /// <summary>
        /// 泵5流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump5FlowRate = 0x6AA10;

        /// <summary>
        /// 泵5流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump5Flow = 0x6AA11;

        /// <summary>
        /// 泵5总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Pump5FlowTotal = 0x6AA12;

        /// <summary>
        /// MFC1流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC1FlowRate = 0x6AA13;

        /// <summary>
        /// MFC1总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC1FlowCapacity = 0x6AA14;

        /// <summary>
        /// MFC2流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC2FlowRate = 0x6AA15;

        /// <summary>
        /// MFC2总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC2FlowCapacity = 0x6AA16;

        /// <summary>
        /// MFC3流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC3FlowRate = 0x6AA17;

        /// <summary>
        /// MFC3总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC3FlowCapacity = 0x6AA18;

        /// <summary>
        /// MFC4流量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC4FlowRate = 0x6AA19;

        /// <summary>
        /// MFC4总量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_MFC4FlowCapacity = 0x6AA1A;

        /// <summary>
        /// 罐体重量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_JarWeight = 0x6AA1B;

        /// <summary>
        /// 瓶子1重量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Bottle1Weight = 0x6AA1C;

        /// <summary>
        /// 瓶子2重量
        /// </summary>
        public const int RRealtimeParam_ReadResponse_Bottle2Weight = 0x6AA1D;

        /// <summary>
        /// pH传感器温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_pHSensorTemp = 0x6AA1E;

        /// <summary>
        /// DO传感器温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_DOSensorTemp = 0x6AA1F;

        /// <summary>
        /// 加热座冷面NTC温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_HeatingBaseCoolingNTCTemp = 0x6AA20;

        /// <summary>
        /// 加热座热面NTC温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_HeatingBaseHeatingNTCTemp = 0x6AA21;

        /// <summary>
        /// 冷凝模块冷面NTC温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_CoolingModuleCoolingNTCTemp = 0x6AA22;

        /// <summary>
        /// 冷凝模块热面NTC温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_CoolingModuleHeatingNTCTemp = 0x6AA23;

        /// <summary>
        /// 冷凝模块室内NTC温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_CoolingModuleRoomNTCTemp = 0x6AA24;

        /// <summary>
        /// 进气模块CO2浓度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_IntakeModuleCO2Concentration = 0x6AA25;

        /// <summary>
        /// 进气模块O2浓度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_IntakeModuleO2Concentration = 0x6AA26;

        /// <summary>
        /// 进气模块气体温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_IntakeModuleGasTemp = 0x6AA27;

        /// <summary>
        /// 进气模块气体湿度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_IntakeModuleGasHumidity = 0x6AA28;

        /// <summary>
        /// 进气模块气体压力
        /// </summary>
        public const int RRealtimeParam_ReadResponse_IntakeModuleGasPressure = 0x6AA29;

        /// <summary>
        /// 尾气模块CO2浓度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_OffgasModuleCO2Concentration = 0x6AA2A;

        /// <summary>
        /// 尾气模块O2浓度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_OffgasModuleO2Concentration = 0x6AA2B;

        /// <summary>
        /// 尾气模块气体温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_OffgasModuleGasTemp = 0x6AA2C;

        /// <summary>
        /// 尾气模块气体湿度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_OffgasModuleGasHumidity = 0x6AA2D;

        /// <summary>
        /// 尾气模块气体压力
        /// </summary>
        public const int RRealtimeParam_ReadResponse_OffgasModuleGasPressure = 0x6AA2E;

        /// <summary>
        /// 搅拌电机温度
        /// </summary>
        public const int RRealtimeParam_ReadResponse_StirringMotorTemp = 0x6AA2F;

        /// <summary>
        /// 搅拌电机功率
        /// </summary>
        public const int RRealtimeParam_ReadResponse_StirringMotorPower = 0x6AA30;

        /// <summary>
        /// 有无泡沫
        /// </summary>
        public const int RRealtimeParam_ReadResponse_HasFoam = 0x6AA31;

        /// <summary>
        /// 报警码
        /// </summary>
        public const int RRealtimeParam_ReadResponse_AlarmCodes = 0x6AA32;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RRealtimeParam_WriteResponse_ResponseValue = 0x68800;

        /// <summary>
        /// 板类型
        /// </summary>
        public const int RMCUVersion_Read_Type = 0x75500;

        /// <summary>
        /// 板类型
        /// </summary>
        public const int RMCUVersion_ReadResponse_Type = 0x7AA00;

        /// <summary>
        /// 版本号
        /// </summary>
        public const int RMCUVersion_ReadResponse_Version = 0x7AA01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RMCUVersion_WriteResponse_ResponseValue = 0x78800;

        /// <summary>
        /// P参数
        /// </summary>
        public const int RWTempControlDebug_ReadWrite_P = 0x8FF00;

        /// <summary>
        /// I参数
        /// </summary>
        public const int RWTempControlDebug_ReadWrite_I = 0x8FF01;

        /// <summary>
        /// D参数
        /// </summary>
        public const int RWTempControlDebug_ReadWrite_D = 0x8FF02;

        /// <summary>
        /// 系数
        /// </summary>
        public const int RWTempControlDebug_ReadWrite_K = 0x8FF03;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWTempControlDebug_WriteResponse_ResponseValue = 0x88800;

        /// <summary>
        /// 主板IP地址
        /// </summary>
        public const int RWDeviceParam_ReadWrite_MainIPAdress = 0x9FF00;

        /// <summary>
        /// 主板端口号
        /// </summary>
        public const int RWDeviceParam_ReadWrite_MainPort = 0x9FF01;

        /// <summary>
        /// 主板网关
        /// </summary>
        public const int RWDeviceParam_ReadWrite_MainGateway = 0x9FF02;

        /// <summary>
        /// Wifi模组IP地址
        /// </summary>
        public const int RWDeviceParam_ReadWrite_WifiIPAdress = 0x9FF03;

        /// <summary>
        /// Wifi端口号
        /// </summary>
        public const int RWDeviceParam_ReadWrite_WifiPort = 0x9FF04;

        /// <summary>
        /// Wifi网关
        /// </summary>
        public const int RWDeviceParam_ReadWrite_WifiGateway = 0x9FF05;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWDeviceParam_WriteResponse_ResponseValue = 0x98800;

        /// <summary>
        /// 传感器种类
        /// </summary>
        public const int RWSensorCorrect_Read_SensorType = 0xA5500;

        /// <summary>
        /// 传感器种类
        /// </summary>
        public const int RWSensorCorrect_ReadResponse_SensorType = 0xAAA00;

        /// <summary>
        /// 传感器系数
        /// </summary>
        public const int RWSensorCorrect_ReadResponse_SensorCoefficient = 0xAAA01;

        /// <summary>
        /// 传感器偏置
        /// </summary>
        public const int RWSensorCorrect_ReadResponse_SensorBias = 0xAAA02;

        /// <summary>
        /// 状态码
        /// </summary>
        public const int RWSensorCorrect_ReadResponse_StatusCode = 0xAAA03;

        /// <summary>
        /// 传感器种类
        /// </summary>
        public const int RWSensorCorrect_Write_SensorType = 0xA6600;

        /// <summary>
        /// 校准模式
        /// </summary>
        public const int RWSensorCorrect_Write_CorrectMode = 0xA6601;

        /// <summary>
        /// 校准数据
        /// </summary>
        public const int RWSensorCorrect_Write_CorrectValue = 0xA6602;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWSensorCorrect_WriteResponse_ResponseValue = 0xA8800;

        /// <summary>
        /// 蠕动泵编号
        /// </summary>
        public const int RWPeristalticPumpCorrect_Read_PeristalticPumpNo = 0xB5500;

        /// <summary>
        /// 蠕动泵编号
        /// </summary>
        public const int RWPeristalticPumpCorrect_ReadResponse_PeristalticPumpNo = 0xBAA00;

        /// <summary>
        /// 蠕动泵系数
        /// </summary>
        public const int RWPeristalticPumpCorrect_ReadResponse_PeristalticPumpCoefficient = 0xBAA01;

        /// <summary>
        /// 蠕动泵编号
        /// </summary>
        public const int RWPeristalticPumpCorrect_Write_PeristalticPumpNo = 0xB6600;

        /// <summary>
        /// 校准参数
        /// </summary>
        public const int RWPeristalticPumpCorrect_Write_CorrectParam = 0xB6601;

        /// <summary>
        /// 校准数据
        /// </summary>
        public const int RWPeristalticPumpCorrect_Write_CorrectValue = 0xB6602;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWPeristalticPumpCorrect_WriteResponse_ResponseValue = 0xB8800;

        /// <summary>
        /// 流量类型
        /// </summary>
        public const int WCleanFlowCapacity_Write_Type = 0xC6600;

        /// <summary>
        /// 编号
        /// </summary>
        public const int WCleanFlowCapacity_Write_No = 0xC6601;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int WCleanFlowCapacity_WriteResponse_ResponseValue = 0xC8800;

        /// <summary>
        /// 红灯状态
        /// </summary>
        public const int RWSoundLightAlarm_ReadWrite_RedLightStatus = 0xDFF00;

        /// <summary>
        /// 绿灯状态
        /// </summary>
        public const int RWSoundLightAlarm_ReadWrite_GreenLightStatus = 0xDFF01;

        /// <summary>
        /// 蓝灯状态
        /// </summary>
        public const int RWSoundLightAlarm_ReadWrite_BlueLightStatus = 0xDFF02;

        /// <summary>
        /// 蜂鸣器状态
        /// </summary>
        public const int RWSoundLightAlarm_ReadWrite_BuzzerStatus = 0xDFF03;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWSoundLightAlarm_WriteResponse_ResponseValue = 0xD8800;

        /// <summary>
        /// 年
        /// </summary>
        public const int RWTimeSync_ReadWrite_Year = 0xEFF00;

        /// <summary>
        /// 月
        /// </summary>
        public const int RWTimeSync_ReadWrite_Month = 0xEFF01;

        /// <summary>
        /// 日
        /// </summary>
        public const int RWTimeSync_ReadWrite_Day = 0xEFF02;

        /// <summary>
        /// 时
        /// </summary>
        public const int RWTimeSync_ReadWrite_Hour = 0xEFF03;

        /// <summary>
        /// 分
        /// </summary>
        public const int RWTimeSync_ReadWrite_Minute = 0xEFF04;

        /// <summary>
        /// 秒
        /// </summary>
        public const int RWTimeSync_ReadWrite_Second = 0xEFF05;

        /// <summary>
        /// 时
        /// </summary>
        public const int RWTimeSync_ReadWrite_RunningHour = 0xEFF06;

        /// <summary>
        /// 分
        /// </summary>
        public const int RWTimeSync_ReadWrite_RunningMinute = 0xEFF07;

        /// <summary>
        /// 秒
        /// </summary>
        public const int RWTimeSync_ReadWrite_RunningSecond = 0xEFF08;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWTimeSync_WriteResponse_ResponseValue = 0xE8800;

        /// <summary>
        /// PH自动控制
        /// </summary>
        public const int WSettingSync_Write_PHAuto = 0xF6600;

        /// <summary>
        /// PH目标值
        /// </summary>
        public const int WSettingSync_Write_PH = 0xF6601;

        /// <summary>
        /// 溶氧自动控制
        /// </summary>
        public const int WSettingSync_Write_DOAuto = 0xF6602;

        /// <summary>
        /// 溶氧目标值
        /// </summary>
        public const int WSettingSync_Write_DO = 0xF6603;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int WSettingSync_WriteResponse_ResponseValue = 0xF8800;

        /// <summary>
        /// 使能
        /// </summary>
        public const int RWCondensationControl_ReadWrite_Enable = 0x10FF00;

        /// <summary>
        /// 设定温度
        /// </summary>
        public const int RWCondensationControl_ReadWrite_Temp = 0x10FF01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWCondensationControl_WriteResponse_ResponseValue = 0x108800;

        /// <summary>
        /// 氧气浓度
        /// </summary>
        public const int ROffgas_ReadResponse_O2 = 0x11AA00;

        /// <summary>
        /// 二氧化碳浓度
        /// </summary>
        public const int ROffgas_ReadResponse_CO2 = 0x11AA01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int ROffgas_WriteResponse_ResponseValue = 0x118800;

        /// <summary>
        /// 下载程序状态
        /// </summary>
        public const int RWMCUDownload_ReadResponse_Status = 0x12AA00;

        /// <summary>
        /// 板子编号
        /// </summary>
        public const int RWMCUDownload_Write_No = 0x126600;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWMCUDownload_WriteResponse_ResponseValue = 0x128800;

        /// <summary>
        /// 部件类型
        /// </summary>
        public const int WResetDefaultSetting_Write_Type = 0x136600;

        /// <summary>
        /// 编号
        /// </summary>
        public const int WResetDefaultSetting_Write_No = 0x136601;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int WResetDefaultSetting_WriteResponse_ResponseValue = 0x138800;

        /// <summary>
        /// 主板IP地址
        /// </summary>
        public const int RWMCUDownloadAdress_ReadWrite_IPAdress = 0x14FF00;

        /// <summary>
        /// 主板端口号
        /// </summary>
        public const int RWMCUDownloadAdress_ReadWrite_Port = 0x14FF01;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWMCUDownloadAdress_WriteResponse_ResponseValue = 0x148800;

        /// <summary>
        /// 电机型号
        /// </summary>
        public const int RWStirringMotorType_ReadWrite_MotorType = 0x15FF00;

        /// <summary>
        /// 写回应
        /// </summary>
        public const int RWStirringMotorType_WriteResponse_ResponseValue = 0x158800;

    }
}
