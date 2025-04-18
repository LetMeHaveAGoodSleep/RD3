using System;
using Fpi.Communication.Commands;
using Fpi.Communication.Converter;
using Fpi.Xml;

namespace Fpi.Communication.Ports.Web
{
    public class WebRecvCommand : RecvCommand
    {
        public WebRecvCommand(byte[] data) : base(data)
        {
        }

        protected override void Init(byte[] data)
        {
            //命令数据
            int commandIdLength = (int) data[0];
            cmdId = DataConverter.GetInstance().ToString(data, 1, commandIdLength);
            extCode = data[commandIdLength + 1] & 0xFF;

            cmdData = new byte[data.Length - commandIdLength - 2];
            Buffer.BlockCopy(data, commandIdLength + 2, cmdData, 0, cmdData.Length);
        }


        //设置参数描述，接收到命令后调用
        internal void Init(NodeList parameters, IDataConvertable converter, int paramIdPrefix)
        {
            this.cmdId = cmdId;
            this.paramIdPrefix = paramIdPrefix;
            parametersData = new ParametersData(parameters, converter, paramIdPrefix, cmdData);
        }
    }
}