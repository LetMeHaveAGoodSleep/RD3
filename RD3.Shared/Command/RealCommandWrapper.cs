using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class RealCommandWrapper : ICommandWrapper
    {
        private CustomTcpClient _tcpClient;

        public RealCommandWrapper()
        {
        }

        public RealCommandWrapper(CustomTcpClient tcpClient):base()
        {
            _tcpClient = tcpClient;
        }

        public float GetAcidSpeed()
        {
            return 1.5f;
        }

        public float GetAFSpeed()
        {
            return 1.5f;
        }

        public float GetAgitSpeed()
        {
            return 1.5f;
        }

        public float GetBaseSpeed()
        {
            return 1.5f;
        }

        public byte[] GetData()
        {
            return [0x01];
        }

        public byte[] GetDeviceParam()
        {
            return [0x01];
        }

        public DOParam GetDO()
        {
            return new DOParam();
        }

        public float GetFeedSpeed()
        {
            return 1.5f;
        }

        public float GetGasSpeed()
        {
            return 1.5f;
        }

        public byte[] GetHistoryData()
        {
            return [0x01];
        }

        public float GetInoculate()
        {
            return 1.5f;
        }

        public byte[] GetMCUVersion(byte adress)
        {
            return [0x01];
        }

        public byte[] GetMonitorInfo(byte index)
        {
            return [0x01];
        }

        public float GetORP()
        {
            return 1.5f;
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
            return [0x01];
        }

        public TempParam GetTemp()
        {
            return new TempParam();
        }

        public void SetAcidSpeed(float speed)
        {
            
        }

        public void SetAFSpeed(float speed)
        {
            
        }

        public void SetAgitSpeed(float speed)
        {
            
        }

        public void SetBaseSpeed(float speed)
        {
            
        }

        public void SetDeviceParam(byte[] ipAdress, int port, byte[] NFCParam, byte[] wifiIpAdress, int wifiport, byte[] screenParam)
        {
            
        }

        public void SetDO(DOParam dOParam)
        {
            
        }

        public void SetFeedSpeed(float speed)
        {
            
        }

        public void SetGasSpeed(float speed, GasType gasType)
        {
            
        }

        public void SetInoculate(float inoculate)
        {
            
        }

        public void SetMonitorInfo(float speed, byte direction)
        {
            
        }

        public void SetORP(float orp)
        {
            
        }

        public void SetPH(PHParam pHParam)
        {
            
        }

        public void SetSenorParamCorrect(SensorCorrectionMode correctionMode, SensorType sensorType, byte index, float value)
        {
            
        }

        public void SetTECInfo(byte index, float temp, byte p, byte i, byte d)
        {
            
        }

        public void SetTemp(TempParam tempParam)
        {
            
        }
    }
}
