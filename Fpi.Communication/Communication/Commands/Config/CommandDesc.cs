using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;

namespace Fpi.Communication.Commands.Config
{
    /// <summary>
    /// CmdDescItem 的摘要说明。
    /// </summary>
    public class CommandDesc : IdNameNode
    {
        public int commandCode;
        public int timeOut;
        public int tryTimes;
        public string receiver;
        public string timeOutReceiver;
        public NodeList commandExtends;
        public string destInstrument;
        public string converter;
        public new string tag;


        public CommandDesc()
        {
            tryTimes = 1;
        }

        public CommandExtend GetCommandExtend(int commandExtendId)
        {
            //for (int i=0; i<commandExtends.GetCount(); i++)
            //{
            //    CommandExtend commandExtend = (CommandExtend)commandExtends[i];
            //    if (commandExtend.GetCommandExtendId() == commandExtendId)
            //        return commandExtend;
            //}
            //return null;
            CommandExtend commandExtend = (CommandExtend) commandExtends.FindNode(commandExtendId);
            if ((commandExtend == null) &&
                ((commandExtendId == CommandExtendId.ReadResponse) || (commandExtendId == CommandExtendId.Write)))
                commandExtend = (CommandExtend) commandExtends.FindNode((int) CommandExtendId.ReadWrite);
            return commandExtend;
        }

        public int GetCommandId()
        {
            return StringUtil.ParseInt(id);
        }

        public bool CanWrite()
        {
            for (int i = 0; i < commandExtends.GetCount(); i++)
            {
                CommandExtend commandExtend = (CommandExtend) commandExtends[i];
                int ceId = commandExtend.GetCommandExtendId();
                if ((ceId == CommandExtendId.Write) || (ceId == CommandExtendId.ReadWrite))
                    return true;
            }
            return false;
        }

        public bool CanRead()
        {
            for (int i = 0; i < commandExtends.GetCount(); i++)
            {
                CommandExtend commandExtend = (CommandExtend) commandExtends[i];
                int ceId = commandExtend.GetCommandExtendId();
                if ((ceId == CommandExtendId.Read) || (ceId == CommandExtendId.ReadWrite))
                    return true;
            }
            return false;
        }

        public bool CanReadWrite()
        {
            return CanWrite() && CanRead();
        }


        private static IReceivable commandReceiver = null;
        private static object syncObj = new object();

        public IReceivable GetReceiver()
        {
            lock (syncObj)
            {
                if (commandReceiver == null)
                {
                    if ((receiver != null) && (receiver.Trim().Length > 0))
                    {
                        commandReceiver = (IReceivable) ReflectionHelper.CreateInstance(receiver);
                    }
                }
            }
            return commandReceiver;
        }
    }
}