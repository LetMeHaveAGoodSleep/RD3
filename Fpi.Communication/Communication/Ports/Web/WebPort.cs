using Fpi.Communication.Commands.Config;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Communication.Ports.NumberPorts;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Commands;
using Fpi.Util.Reflection;

namespace Fpi.Communication.Ports.Web
{
    /// <summary>
    /// ����˿ڡ�
    /// </summary>
    public class WebPort : BasePort
    {
        private CommandManager manager;
        private IDataConvertable converter;

        public static readonly string PropertyName_Manager = CommandPort.PropertyName_Manager;
        public static readonly string PropertyName_Converter = CommandPort.PropertyName_Converter;

        public override void Init(BaseNode config)
        {
            base.Init(config);
            manager =
                (CommandManager)
                ReflectionHelper.CreateInstance(this.GetProperty(PropertyName_Manager, typeof (WebCommandManager).FullName));
            converter =
                (IDataConvertable)
                ReflectionHelper.CreateInstance(this.GetProperty(PropertyName_Converter, typeof (DataConverter).FullName));
        }

        public override void Receive(object source, IByteStream data)
        {
            NumberData numberData = (NumberData) data;

            WebRecvCommand command = new WebRecvCommand(numberData.data.GetBytes());

            InitCommand(command);

            //�ϲ�˿ڽ�������
            IPortOwner portOwner = PortOwner;
            portOwner.Receive(numberData.number, command);
        }

        internal void InitCommand(WebRecvCommand command)
        {
            string cmdId = command.GetCmdId();

            //������������
            CommandDesc commandDesc = (CommandDesc) manager.commandDescs[cmdId];

            //�õ���չ��
            int extCode = command.GetExtCode();

            //������չ����
            CommandExtend commandExtend = (CommandExtend) CommandManager.FindCommandExtend(commandDesc, extCode);

            //����IDǰ׺
            int paramIdPrefix = (manager.commandDescs.IndexOf(commandDesc) << 8) +
                                StringUtil.ParseByte(commandExtend.id);

            //���ò�������,����ת����
            command.Init(commandExtend.parameters, converter, paramIdPrefix);
        }

        public override object Send(object dest, IByteStream data)
        {
            if (!(data is WebSendCommand))
            {
                //throw new ArgumentException("����������� WebSendCommand ���͡�");
                return null;
            }
            //��װ����
            WebSendCommand command = (WebSendCommand) data;
            //��������
            CommandDesc commandDesc = (CommandDesc) manager.commandDescs[command.GetCmdId()];

            int extCode = command.GetExtCode();
            //������չ����
            CommandExtend commandExtend = (CommandExtend) CommandManager.FindCommandExtend(commandDesc, extCode);

            //����IDǰ׺
            int paramIdPrefix = (manager.commandDescs.IndexOf(commandDesc) << 8) +
                                StringUtil.ParseByte(commandExtend.id);

            command.BuildData(commandDesc, commandExtend.parameters, converter, paramIdPrefix);


            IPort lowerPort = LowerPort;
            object res = null;

            if ((dest == null) || !(dest is byte))
            {
                res = lowerPort.Send(dest, command);
            }
            else
            {
                res = lowerPort.Send(dest, new NumberData((byte) dest, command));
            }


            if (res == null || !(res is NumberData))
            {
                return res;
            }

            NumberData numberData = (NumberData) res;
            WebRecvCommand recvCmd = new WebRecvCommand(numberData.data.GetBytes());
            InitCommand(recvCmd);
            return recvCmd;
        }
    }
}