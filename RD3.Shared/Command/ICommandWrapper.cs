using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public interface ICommandWrapper
    {
        TempParam GetTemp();

        void SetTemp(TempParam tempParam);

        PHParam GetPH();

        void SetPH(PHParam pHParam);

        DOParam GetDO();

        void SetDO(DOParam dOParam);

        float GetAgitSpeed();

        void SetAgitSpeed(float speed);

        float GetAcidSpeed();

        void SetAcidSpeed(float speed);

        float GetBaseSpeed();

        void SetBaseSpeed(float speed);

        float GetFeedSpeed();

        void SetFeedSpeed(float speed);

        float GetInoculate();

        void SetInoculate(float inoculate);

        float GetAFSpeed();

        void SetAFSpeed(float speed);

        float GetGasSpeed();

        void SetGasSpeed(float speed, GasType gasType);

        RealTimeParam GetRealTime();

        byte[] GetMCUVersion(byte adress);

        byte[] GetHistoryData();

         byte[] GetData();

        float GetORP();

        void SetORP(float orp);

        void SetSenorParamCorrect(SensorCorrectionMode correctionMode, SensorType sensorType, byte index, float value);

        byte[] GetMonitorInfo(byte index);

        void SetMonitorInfo(float speed,byte direction);

        byte[] GetTECInfo(byte index);

        void SetTECInfo(byte index,float temp,byte p,byte i,byte d);

        byte[] GetDeviceParam();

        void SetDeviceParam(byte[] ipAdress, int port, byte[] NFCParam, byte[] wifiIpAdress, int wifiport, byte[] screenParam);
    }
}
