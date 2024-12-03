using System;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Util.Reflection;

namespace Fpi.Communication.Commands.Config
{
    /// <summary>
    /// CommandManager 的摘要说明。
    /// </summary>
    public class CommandManager : IdNameNode
    {
        public int timeOut;
        public int tryTimes;
        public string commandListener;
        public string converter;

        public NodeList commandDescs = new NodeList();

        private ICommandListener iCommandListener;
        public CommandManager(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                this.loadXml();
                if (commandListener != null)
                {
                    iCommandListener = (ICommandListener)ReflectionHelper.CreateInstance(commandListener);
                }
            }
            else
            {
                LoadXml(path);
                if (commandListener != null)
                {
                    iCommandListener = (ICommandListener)ReflectionHelper.CreateInstance(commandListener);
                }
            }
        }
        //protected CommandManager()
        //{
        //    this.loadXml();
        //    if (commandListener != null)
        //    {
        //        iCommandListener = (ICommandListener) ReflectionHelper.CreateInstance(commandListener);
        //    }
        //}

        public ICommandListener GetCommandListener()
        {
            return iCommandListener;
        }

        private static object syncObj = new object();
        private static CommandManager instance = null;

        public static CommandManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new CommandManager();
                }
            }
            return instance;
        }
        public string DisplayName
        {
            get
            {
                return this.GetType().Name;
            }
            set { }
        }

        public virtual IdNameNode[] GetAllCommand()
        {
            if (this.commandDescs == null || this.commandDescs.GetCount() == 0)
            {
                return new IdNameNode[0];
            }
            IdNameNode[] result = new IdNameNode[this.commandDescs.GetCount()];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (IdNameNode) this.commandDescs[i];
            }

            return result;
        }

        public CommandDesc FindCommandDesc(string cmdID)
        {
            foreach (CommandDesc cd in commandDescs)
            {
                if (cd.id == cmdID)
                {
                    return cd;
                }
            }
            throw new CommandException("no command code find: " + cmdID);
        }

        public CommandDesc FindCommandDesc(int commandCode)
        {
            foreach (CommandDesc cd in commandDescs)
            {
                if (cd.commandCode == commandCode)
                {
                    return cd;
                }
            }
            throw new CommandException("no command code find: " + commandCode);
        }

        public static CommandExtend FindCommandExtend(CommandDesc commandDesc, int extCode)
        {
            CommandExtend commandExtend = (CommandExtend) commandDesc.commandExtends.FindNode(extCode);
            if ((commandExtend == null) &&
                ((extCode == CommandExtendId.ReadResponse) || (extCode == CommandExtendId.Write)))
                commandExtend = (CommandExtend) commandDesc.commandExtends.FindNode((int) CommandExtendId.ReadWrite);
            if (commandExtend == null)
            {
                throw new CommandException("no command extend code find: " + extCode);
            }
            return commandExtend;
        }

        public static CommandManager GetCommandManager(string managerType)
        {
            Type manager = typeof (CommandManager);
            if (manager.FullName.ToLower().EndsWith(managerType.ToLower()))
            {
                return GetInstance();
            }

            Type[] types = ReflectionHelper.GetChildTypes(manager.GetType());
            foreach (Type type in types)
            {
                if (type.FullName.ToLower().EndsWith(managerType.ToLower()))
                {
                    return ReflectionHelper.CreateInstance(type) as CommandManager;
                }
            }

            return null;
        }
    }
}