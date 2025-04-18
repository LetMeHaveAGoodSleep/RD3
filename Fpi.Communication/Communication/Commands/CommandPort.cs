using Fpi.Communication.Commands.Config;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Instruments;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;

namespace Fpi.Communication.Commands
{
    /// <summary>
    /// 命令端口。
    /// </summary>
    public class CommandPort : BasePort
    {
        private CommandManager manager;
        private IDataConvertable converter;

        public static readonly string PropertyName_Manager = "manager";
        public static readonly string PropertyName_Converter = "convert";

        public override void Init(BaseNode config)
        {
            base.Init(config);
            try
            {
                manager =
                    (CommandManager)
                    ReflectionHelper.CreateInstance(this.GetProperty(PropertyName_Manager, typeof(CommandManager).FullName));
            }
            catch { }

            converter =
                (IDataConvertable)
                ReflectionHelper.CreateInstance(this.GetProperty(PropertyName_Converter, typeof (DataConverter).FullName));
        }

        public CommandManager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        public override void Receive(object source, IByteStream data)
        {
            RecvCommand command = ParseRecvCommand(data);

            //上层端口接收数据
            if (PortOwner != null)
            {
                PortOwner.Receive(source, command);
            }
        }

        private RecvCommand ParseRecvCommand(IByteStream data)
        {
            byte[] commandData = data.GetBytes();
            RecvCommand command = new RecvCommand(commandData);
            //得到命令码
            int commandCode = command.GetCmdCode();
            //查找命令描述
            CommandDesc commandDesc = manager.FindCommandDesc(commandCode);
            //得到扩展码
            int extCode = command.GetExtCode();
            //查找扩展描述
            CommandExtend commandExtend = CommandManager.FindCommandExtend(commandDesc, extCode);

            //参数ID前缀
            int paramIdPrefix = (manager.commandDescs.IndexOf(commandDesc) << 8) + extCode;

            //设置参数描述,数据转换器
            command.Init(commandDesc.id, commandExtend.parameters, converter, paramIdPrefix);
            return command;
        }

        private RecvCommand ParseRecvCommand(CommandDesc commandDesc, IByteStream data)
        {
            byte[] commandData = data.GetBytes();
            RecvCommand command = new RecvCommand(commandData);
            //得到命令码
            //int commandCode = command.GetCmdCode();
            //查找命令描述
            // CommandDesc commandDesc = manager.FindCommandDesc(commandCode);
            //得到扩展码
            int extCode = command.GetExtCode();
            //查找扩展描述
            CommandExtend commandExtend = CommandManager.FindCommandExtend(commandDesc, extCode);

            //参数ID前缀
            int paramIdPrefix = (manager.commandDescs.IndexOf(commandDesc) << 8) + extCode;

            //设置参数描述,数据转换器
            command.Init(commandDesc.id, commandExtend.parameters, converter, paramIdPrefix);
            return command;
        }

        public override object Send(object dest, IByteStream data)
        {
            if (dest is string)
            {
                GetCommandManager((string) dest);
            }

            //组装数据
            SendCommand command = (SendCommand) data;
            //命令描述
            CommandDesc commandDesc = (CommandDesc) manager.commandDescs[command.GetCmdId()];

            int extCode = command.GetExtCode();
            //查找扩展描述
            CommandExtend commandExtend = CommandManager.FindCommandExtend(commandDesc, extCode);

            //参数ID前缀
            int paramIdPrefix = (manager.commandDescs.IndexOf(commandDesc) << 8) +
                                StringUtil.ParseByte(commandExtend.id);

            command.BuildData(commandDesc, commandExtend.parameters, converter, paramIdPrefix);

            IPort lowerPort = LowerPort;

            object res = lowerPort.Send(dest, command);

            if (res == null || !(res is IByteStream))
            {
                return res;
            }

            RecvCommand resCmd = ParseRecvCommand(commandDesc, (IByteStream) res);
            return resCmd;
        }

        private void GetCommandManager(string instrumentID)
        {
            InstrumentType insType = InstrumentManager.GetInstance().GetInstrumentType(instrumentID);

            if (insType != null)
            {
                string mngName = insType.GetPropertyValue(PropertyName_Manager);
                if (mngName != null && mngName != string.Empty)
                {
                    manager = (CommandManager)ReflectionHelper.CreateInstance(mngName);
                }
            }
        }
    }
}