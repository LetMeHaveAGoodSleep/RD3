using System;
using Fpi.Communication.Commands;
using Fpi.Communication.Converter;

namespace Fpi.Communication.Ports.Web
{
    public class WebSendCommand : SendCommand
    {
        public WebSendCommand(string cmdId, int extCode)
            : base(cmdId, extCode)
        {
        }

        public override byte[] GetBytes()
        {
            byte[] paramData = parametersData.GetData();
            byte[] commandIdBytes = DataConverter.GetInstance().GetBytes(cmdId);
            byte[] result = new byte[commandIdBytes.Length + 2 + paramData.Length];
            result[0] = (byte) commandIdBytes.Length;
            Buffer.BlockCopy(commandIdBytes, 0, result, 1, commandIdBytes.Length);
            result[commandIdBytes.Length + 1] = (byte) extCode;
            Buffer.BlockCopy(paramData, 0, result, commandIdBytes.Length + 2, paramData.Length);
            return result;
        }
    }
}