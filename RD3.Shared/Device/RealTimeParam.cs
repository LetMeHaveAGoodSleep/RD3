using Fpi.Util.Sundry;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class RealTimeParam : BindableBase
    {
        public RealTimeParam()
        {
            AlarmBytes = [];
            SampleTime = DateTime.Now;
        }

        private string _reactorName;
        [JsonIgnore]
        public string ReactorName
        {
            get { return _reactorName; }
            set { SetProperty(ref _reactorName, value); }
        }

        private DateTime _sampleTime;
        [JsonIgnore]
        public DateTime SampleTime
        {
            get { return _sampleTime; }
            set { SetProperty(ref _sampleTime, value); }
        }

        private float _temp = 0f;
        [JsonIgnore]
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
        }

        private float _pH = 0f;
        [JsonIgnore]
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private float _rawDO = 0f;
        [JsonIgnore]
        public float RawDO
        {
            get { return _rawDO; }
            set { SetProperty(ref _rawDO, value); }
        }

        private float _dO = 0f;
        [JsonIgnore]
        public float DO
        {
            get { return _dO; }
            set { SetProperty(ref _dO, value); }
        }

        private float _dOPredict = 0f;
        [JsonIgnore]
        public float DOPredict
        {
            get { return _dOPredict; }
            set { SetProperty(ref _dOPredict, value); }
        }

        private int _lastAgit = 0;
        [JsonIgnore]
        public int LastAgit
        {
            get { return _lastAgit; }
            set { SetProperty(ref _lastAgit, value); }
        }

        private int _agit = 0;
        [JsonIgnore]
        public int Agit
        {
            get { return _agit; }
            set { SetProperty(ref _agit, value); }
        }

        private float _pump1FlowRate = 0f;
        [JsonIgnore]
        public float Pump1FlowRate
        {
            get { return _pump1FlowRate; }
            set { SetProperty(ref _pump1FlowRate, value); }
        }

        private float _pump1Flow = 0f;
        [JsonIgnore]
        public float Pump1Flow
        {
            get { return _pump1Flow; }
            set { SetProperty(ref _pump1Flow, value); }
        }

        private float _pump1FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump1FlowCapacity
        {
            get { return _pump1FlowCapacity; }
            set { SetProperty(ref _pump1FlowCapacity, value); }
        }

        private float _pump2FlowRate = 0f;
        [JsonIgnore]
        public float Pump2FlowRate
        {
            get { return _pump2FlowRate; }
            set { SetProperty(ref _pump2FlowRate, value); }
        }

        private float _pump2Flow = 0f;
        [JsonIgnore]
        public float Pump2Flow
        {
            get { return _pump2Flow; }
            set { SetProperty(ref _pump2Flow, value); }
        }

        private float _pump2FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump2FlowCapacity
        {
            get { return _pump2FlowCapacity; }
            set { SetProperty(ref _pump2FlowCapacity, value); }
        }

        private float _pump3FlowRate = 0f;
        [JsonIgnore]
        public float Pump3FlowRate
        {
            get { return _pump3FlowRate; }
            set { SetProperty(ref _pump3FlowRate, value); }
        }

        private float _pump3Flow = 0f;
        [JsonIgnore]
        public float Pump3Flow
        {
            get { return _pump3Flow; }
            set { SetProperty(ref _pump3Flow, value); }
        }

        private float _pump3FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump3FlowCapacity
        {
            get { return _pump3FlowCapacity; }
            set { SetProperty(ref _pump3FlowCapacity, value); }
        }

        private float _pump4FlowRate = 0f;
        [JsonIgnore]
        public float Pump4FlowRate
        {
            get { return _pump4FlowRate; }
            set { SetProperty(ref _pump4FlowRate, value); }
        }

        private float _pump4Flow = 0f;
        [JsonIgnore]
        public float Pump4Flow
        {
            get { return _pump4Flow; }
            set { SetProperty(ref _pump4Flow, value); }
        }

        private float _pump4FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump4FlowCapacity
        {
            get { return _pump4FlowCapacity; }
            set { SetProperty(ref _pump4FlowCapacity, value); }
        }

        private float _pump5FlowRate = 0f;
        [JsonIgnore]
        public float Pump5FlowRate
        {
            get { return _pump5FlowRate; }
            set { SetProperty(ref _pump5FlowRate, value); }
        }

        private float _pump5Flow = 0f;
        [JsonIgnore]
        public float Pump5Flow
        {
            get { return _pump5Flow; }
            set { SetProperty(ref _pump5Flow, value); }
        }

        private float _pump5FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump5FlowCapacity
        {
            get { return _pump5FlowCapacity; }
            set { SetProperty(ref _pump5FlowCapacity, value); }
        }

        private float _pump6FlowRate = 0f;
        [JsonIgnore]
        public float Pump6FlowRate
        {
            get { return _pump6FlowRate; }
            set { SetProperty(ref _pump6FlowRate, value); }
        }

        private float _pump6Flow = 0f;
        [JsonIgnore]
        public float Pump6Flow
        {
            get { return _pump6Flow; }
            set { SetProperty(ref _pump6Flow, value); }
        }

        private float _pump6FlowCapacity = 0f;
        [JsonIgnore]
        public float Pump6FlowCapacity
        {
            get { return _pump6FlowCapacity; }
            set { SetProperty(ref _pump6FlowCapacity, value); }
        }

        private float _mfc1FlowRate = 0f;
        [JsonIgnore]
        public float MFC1FlowRate
        {
            get { return _mfc1FlowRate; }
            set { SetProperty(ref _mfc1FlowRate, value); }
        }

        private float _mfc1FlowCapacity = 0f;
        [JsonIgnore]
        public float MFC1FlowCapacity
        {
            get { return _mfc1FlowCapacity; }
            set { SetProperty(ref _mfc1FlowCapacity, value); }
        }

        private float _mfc2FlowRate = 0f;
        [JsonIgnore]
        public float MFC2FlowRate
        {
            get { return _mfc2FlowRate; }
            set { SetProperty(ref _mfc2FlowRate, value); }
        }

        private float _mfc2FlowCapacity = 0f;
        [JsonIgnore]
        public float MFC2FlowCapacity
        {
            get { return _mfc2FlowCapacity; }
            set { SetProperty(ref _mfc2FlowCapacity, value); }
        }

        private float _mfc3FlowRate = 0f;
        [JsonIgnore]
        public float MFC3FlowRate
        {
            get { return _mfc3FlowRate; }
            set { SetProperty(ref _mfc3FlowRate, value); }
        }

        private float _mfc3FlowCapacity = 0f;
        [JsonIgnore]
        public float MFC3FlowCapacity
        {
            get { return _mfc3FlowCapacity; }
            set { SetProperty(ref _mfc3FlowCapacity, value); }
        }

        private float _mfc4FlowRate = 0f;
        [JsonIgnore]
        public float MFC4FlowRate
        {
            get { return _mfc4FlowRate; }
            set { SetProperty(ref _mfc4FlowRate, value); }
        }

        private float _mfc4FlowCapacity = 0f;
        [JsonIgnore]
        public float MFC4FlowCapacity
        {
            get { return _mfc4FlowCapacity; }
            set { SetProperty(ref _mfc4FlowCapacity, value); }
        }

        private float _mfc5FlowRate = 0f;
        [JsonIgnore]
        public float MFC5FlowRate
        {
            get { return _mfc5FlowRate; }
            set { SetProperty(ref _mfc5FlowRate, value); }
        }

        private float _mfc5FlowCapacity = 0f;
        [JsonIgnore]
        public float MFC5FlowCapacity
        {
            get { return _mfc5FlowCapacity; }
            set { SetProperty(ref _mfc5FlowCapacity, value); }
        }

        private float _acidFlowSpeed = 0f;
        [JsonIgnore]
        public float AcidFlowSpeed
        {
            get { return _acidFlowSpeed; }
            set { SetProperty(ref _acidFlowSpeed, value); }
        }

        private float _acidFlowCapacity = 0f;
        [JsonIgnore]
        public float AcidFlowCapacity
        {
            get { return _acidFlowCapacity; }
            set { SetProperty(ref _acidFlowCapacity, value); }
        }

        private float _baseFlowSpeed = 0f;
        [JsonIgnore]
        public float BaseFlowSpeed
        {
            get { return _baseFlowSpeed; }
            set { SetProperty(ref _baseFlowSpeed, value); }
        }

        private float _baseFlowCapacity = 0f;
        [JsonIgnore]
        public float BaseFlowCapacity
        {
            get { return _baseFlowCapacity; }
            set { SetProperty(ref _baseFlowCapacity, value); }
        }

        private float _feedFlowSpeed = 0f;
        [JsonIgnore]
        public float FeedFlowSpeed
        {
            get { return _feedFlowSpeed; }
            set { SetProperty(ref _feedFlowSpeed, value); }
        }

        private float _feedFlowCapacity = 0f;
        [JsonIgnore]
        public float FeedFlowCapacity
        {
            get { return _feedFlowCapacity; }
            set { SetProperty(ref _feedFlowCapacity, value); }
        }

        private float _feed2FlowSpeed = 0f;
        [JsonIgnore]
        public float Feed2FlowSpeed
        {
            get { return _feed2FlowSpeed; }
            set { SetProperty(ref _feed2FlowSpeed, value); }
        }

        private float _feed2FlowCapacity = 0f;
        [JsonIgnore]
        public float Feed2FlowCapacity
        {
            get { return _feed2FlowCapacity; }
            set { SetProperty(ref _feed2FlowCapacity, value); }
        }

        private float _aFFlowSpeed = 0f;
        [JsonIgnore]
        public float AFFlowSpeed
        {
            get { return _aFFlowSpeed; }
            set { SetProperty(ref _aFFlowSpeed, value); }
        }

        private float _aFFlowCapacity = 0f;
        [JsonIgnore]
        public float AFFlowCapacity
        {
            get { return _aFFlowCapacity; }
            set { SetProperty(ref _aFFlowCapacity, value); }
        }

        private float _airFlowSpeed = 0f;
        [JsonIgnore]
        public float AirFlowSpeed
        {
            get { return _airFlowSpeed; }
            set { SetProperty(ref _airFlowSpeed, value); }
        }

        private float _airFlowCapacity = 0f;
        [JsonIgnore]
        public float AirFlowCapacity
        {
            get { return _airFlowCapacity; }
            set { SetProperty(ref _airFlowCapacity, value); }
        }

        private float _o2FlowSpeed = 0f;
        [JsonIgnore]
        public float O2FlowSpeed
        {
            get { return _o2FlowSpeed; }
            set { SetProperty(ref _o2FlowSpeed, value); }
        }

        private float _o2FlowCapacity = 0f;
        [JsonIgnore]
        public float O2FlowCapacity
        {
            get { return _o2FlowCapacity; }
            set { SetProperty(ref _o2FlowCapacity, value); }
        }

        private float _cO2FlowSpeed = 0f;
        [JsonIgnore]
        public float CO2FlowSpeed
        {
            get { return _cO2FlowSpeed; }
            set { SetProperty(ref _cO2FlowSpeed, value); }
        }

        private float _cO2FlowCapacity = 0f;
        [JsonIgnore]
        public float CO2FlowCapacity
        {
            get { return _cO2FlowCapacity; }
            set { SetProperty(ref _cO2FlowCapacity, value); }
        }

        private float _n2FlowSpeed = 0f;
        [JsonIgnore]
        public float N2FlowSpeed
        {
            get { return _n2FlowSpeed; }
            set { SetProperty(ref _n2FlowSpeed, value); }
        }

        private float _n2FlowCapacity = 0f;
        [JsonIgnore]
        public float N2FlowCapacity
        {
            get { return _n2FlowCapacity; }
            set { SetProperty(ref _n2FlowCapacity, value); }
        }

        private float _jarWeight = 0f;
        [JsonIgnore]
        public float JarWeight
        {
            get { return _jarWeight; }
            set { SetProperty(ref _jarWeight, value); }
        }

        private float _bottle1Weight = 0f;
        [JsonIgnore]
        public float Bottle1Weight
        {
            get { return _bottle1Weight; }
            set { SetProperty(ref _bottle1Weight, value); }
        }

        private float _bottle2Weight = 0f;
        [JsonIgnore]
        public float Bottle2Weight
        {
            get { return _bottle2Weight; }
            set { SetProperty(ref _bottle2Weight, value); }
        }

        private float _pHSensorTemp = 0f;
        [JsonIgnore]
        public float PHSensorTemp
        {
            get { return _pHSensorTemp; }
            set { SetProperty(ref _pHSensorTemp, value); }
        }

        private float _doSensorTemp = 0f;
        [JsonIgnore]
        public float DOSensorTemp
        {
            get { return _doSensorTemp; }
            set { SetProperty(ref _doSensorTemp, value); }
        }

        private float _heatingBaseCoolingNTCTemp = 0f;
        /// <summary>
        /// 加热座冷面NTC温度
        /// </summary>
        [JsonIgnore]
        public float HeatingBaseCoolingNTCTemp
        {
            get { return _heatingBaseCoolingNTCTemp; }
            set { SetProperty(ref _heatingBaseCoolingNTCTemp, value); }
        }

        private float _heatingBaseHeatingNTCTemp = 0f;
        /// <summary>
        /// 加热座热面NTC温度
        /// </summary>
        [JsonIgnore]
        public float HeatingBaseHeatingNTCTemp
        {
            get { return _heatingBaseHeatingNTCTemp; }
            set { SetProperty(ref _heatingBaseHeatingNTCTemp, value); }
        }

        private float _coolingModuleCoolingNTCTemp = 0f;
        /// <summary>
        /// 冷凝模块冷面NTC温度
        /// </summary>
        [JsonIgnore]
        public float CoolingModuleCoolingNTCTemp
        {
            get { return _coolingModuleCoolingNTCTemp; }
            set { SetProperty(ref _coolingModuleCoolingNTCTemp, value); }
        }

        private float _coolingModuleHeatingNTCTemp = 0f;
        /// <summary>
        /// 冷凝模块热面NTC温度
        /// </summary>
        [JsonIgnore]
        public float CoolingModuleHeatingNTCTemp
        {
            get { return _coolingModuleHeatingNTCTemp; }
            set { SetProperty(ref _coolingModuleHeatingNTCTemp, value); }
        }

        private float _coolingModuleRoomNTCTemp = 0f;
        /// <summary>
        /// 冷凝模块室温NTC温度
        /// </summary>
        [JsonIgnore]
        public float CoolingModuleRoomNTCTemp
        {
            get { return _coolingModuleRoomNTCTemp; }
            set { SetProperty(ref _coolingModuleRoomNTCTemp, value); }
        }

        private float _intakeModuleCO2Concentration = 0f;
        /// <summary>
        /// 进气模块-CO2浓度
        /// </summary>
        [JsonIgnore]
        public float IntakeModuleCO2Concentration
        {
            get { return _intakeModuleCO2Concentration; }
            set { SetProperty(ref _intakeModuleCO2Concentration, value); }
        }

        private float _intakeModuleO2Concentration = 0f;
        /// <summary>
        /// 进气模块-O2浓度
        /// </summary>
        [JsonIgnore]
        public float IntakeModuleO2Concentration
        {
            get { return _intakeModuleO2Concentration; }
            set { SetProperty(ref _intakeModuleO2Concentration, value); }
        }

        private float _intakeModuleGasTemp = 0f;
        /// <summary>
        /// 进气模块-气体温度
        /// </summary>
        [JsonIgnore]
        public float IntakeModuleGasTemp
        {
            get { return _intakeModuleGasTemp; }
            set { SetProperty(ref _intakeModuleGasTemp, value); }
        }

        private float _intakeModuleGasHumidity = 0f;
        /// <summary>
        /// 进气模块-气体湿度
        /// </summary>
        [JsonIgnore]
        public float IntakeModuleGasHumidity
        {
            get { return _intakeModuleGasHumidity; }
            set { SetProperty(ref _intakeModuleGasHumidity, value); }
        }

        private float _intakeModuleGasPressure = 0f;
        /// <summary>
        /// 进气模块-气体压力
        /// </summary>
        [JsonIgnore]
        public float IntakeModuleGasPressure
        {
            get { return _intakeModuleGasPressure; }
            set { SetProperty(ref _intakeModuleGasPressure, value); }
        }

        private float _offgasModuleCO2Concentration = 0f;
        /// <summary>
        /// 尾气模块-CO2浓度
        /// </summary>
        [JsonIgnore]
        public float OffgasModuleCO2Concentration
        {
            get { return _offgasModuleCO2Concentration; }
            set { SetProperty(ref _offgasModuleCO2Concentration, value); }
        }

        private float _offgasModuleO2Concentration = 0f;
        /// <summary>
        /// 尾气模块-O2浓度
        /// </summary>
        [JsonIgnore]
        public float OffgasModuleO2Concentration
        {
            get { return _offgasModuleO2Concentration; }
            set { SetProperty(ref _offgasModuleO2Concentration, value); }
        }

        private float _offgasModuleGasTemp = 0f;
        /// <summary>
        /// 尾气模块-气体温度
        /// </summary>
        [JsonIgnore]
        public float OffgasModuleGasTemp
        {
            get { return _offgasModuleGasTemp; }
            set { SetProperty(ref _offgasModuleGasTemp, value); }
        }

        private float _offgasModuleGasHumidity = 0f;
        /// <summary>
        /// 尾气模块-气体湿度
        /// </summary>
        [JsonIgnore]
        public float OffgasModuleGasHumidity
        {
            get { return _offgasModuleGasHumidity; }
            set { SetProperty(ref _offgasModuleGasHumidity, value); }
        }

        private float _offgasModuleGasPressure = 0f;
        /// <summary>
        /// 尾气模块-气体压力
        /// </summary>
        [JsonIgnore]
        public float OffgasModuleGasPressure
        {
            get { return _offgasModuleGasPressure; }
            set { SetProperty(ref _offgasModuleGasPressure, value); }
        }

        private float _stirringMotorTemp = 0f;
        /// <summary>
        /// 搅拌电机温度
        /// </summary>
        [JsonIgnore]
        public float StirringMotorTemp
        {
            get { return _stirringMotorTemp; }
            set { SetProperty(ref _stirringMotorTemp, value); }
        }

        private float _stirringMotorPower = 0f;
        /// <summary>
        /// 搅拌电机功率
        /// </summary>
        [JsonIgnore]
        public float StirringMotorPower
        {
            get { return _stirringMotorPower; }
            set { SetProperty(ref _stirringMotorPower, value); }
        }

        private bool _hasFoam = false;
        [JsonIgnore]
        public bool HasFoam
        {
            get { return _hasFoam; }
            set { SetProperty(ref _hasFoam, value); }
        }

        private WorkStatus  _workStatus;
        [JsonIgnore]
        public WorkStatus WorkStatus
        {
            get { return _workStatus; }
            set { SetProperty(ref _workStatus, value); }
        }

        //private byte[] _alarmBytes;
        //public byte[] AlarmBytes
        //{
        //    get { return _alarmBytes; }
        //    set
        //    {
        //        if (value != null && value != _alarmBytes && value.Length > 0)
        //        {
        //            List<string> array = [];
        //            foreach (var item in value)
        //            {
        //                string hexCode = "0x" + item.ToString("X2");
        //                array.Add(hexCode);
        //            }
        //            AlarmCodes = string.Join("-", array);
        //        }
        //        SetProperty(ref _alarmBytes, value);
        //    }
        //}

        //private string _alarmCodes;
        //public string AlarmCodes
        //{
        //    get => _alarmCodes;
        //    set { SetProperty(ref _alarmCodes, value); }
        //}


        private byte[] _alarmBytes;
        [JsonIgnore]
        public byte[] AlarmBytes
        {
            get { return _alarmBytes; }
            set
            {
                if (value != null && value != _alarmBytes)
                {
                    AlarmCodes = StringUtil.BytesToString(value);
                }
                SetProperty(ref _alarmBytes, value);
            }
        }

        private string _alarmCodes;
        [JsonIgnore]
        public string AlarmCodes
        {
            get => _alarmCodes;
            set { SetProperty(ref _alarmCodes, value); }
        }
    }
}
