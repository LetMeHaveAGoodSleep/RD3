using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class VirtualCommandWrapper : ICommandWrapper
    {
        public float GetAcidSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public float GetAFSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public float GetAgitSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public float GetBaseSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public byte[] GetData()
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public byte[] GetDeviceParam()
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public DOParam GetDO()
        {
            return new DOParam();
        }

        public float GetFeedSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public float GetGasSpeed()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public byte[] GetHistoryData()
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public float GetInoculate()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public byte[] GetMCUVersion(byte adress)
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public byte[] GetMonitorInfo(byte index)
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public float GetORP()
        {
            return RandomNumberUtil.GetRandomSingle();
        }

        public PHParam GetPH()
        {
            return new PHParam();
        }

        public RealTimeParam GetRealTime()
        {
            return new RealTimeParam();
        }

        public byte[] GetTECInfo(byte index)
        {
            return BitConverter.GetBytes(RandomNumberUtil.GetRandomSingle());
        }

        public TempParam GetTemp()
        {
            return new TempParam();
        }

        public void SetAcidSpeed(float speed)
        {
            return;
        }

        public void SetAFSpeed(float speed)
        {
            return;
        }

        public void SetAgitSpeed(float speed)
        {
            return;
        }

        public void SetBaseSpeed(float speed)
        {
            return;
        }

        public void SetDeviceParam(byte[] ipAdress, int port, byte[] NFCParam, byte[] wifiIpAdress, int wifiport, byte[] screenParam)
        {
            return;
        }

        public void SetDO(DOParam dOParam)
        {
            return;
        }

        public void SetFeedSpeed(float speed)
        {
            return;
        }

        public void SetGasSpeed(float speed, GasType gasType)
        {
            return;
        }

        public void SetInoculate(float inoculate)
        {
            return;
        }

        public void SetMonitorInfo(float speed, byte direction)
        {
            return;
        }

        public void SetORP(float orp)
        {
            return;
        }

        public void SetPH(PHParam pHParam)
        {
            return;
        }

        public void SetSenorParamCorrect(SensorCorrectionMode correctionMode, SensorType sensorType, byte index, float value)
        {
            return;
        }

        public void SetTECInfo(byte index, float temp, byte p, byte i, byte d)
        {
            return;
        }

        public void SetTemp(TempParam tempParam)
        {
            return;
        }
    }
}
