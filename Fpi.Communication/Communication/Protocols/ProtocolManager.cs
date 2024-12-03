using System;
using System.Collections;
using System.Reflection;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Communication.Exceptions;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Util.Reflection;
using Fpi.Properties;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// 通讯协议管理器
    /// </summary>
    public class ProtocolManager : IInitialization
    {
        private static Hashtable protocolTable = new Hashtable();

        public static Protocol CreateProtocol(string protocolId)
        {
            try
            {
                object ob = ReflectionHelper.CreateInstance(protocolId);
                Protocol p = ob as Protocol;
                return p;
            }
            catch
            {
                throw new ProtocolException(string.Format(Resources.CreateProtocolFailed, protocolId));
            }
        }
        public static Protocol GetProtocol(string protocolId)
        {
            if (string.IsNullOrEmpty(protocolId))
                return null;
            if (protocolTable.ContainsKey(protocolId))
            {
                return protocolTable[protocolId] as Protocol;
            }
            return null;
        }

        public static Protocol GetProtocolByFriendlyName(string friendlyName)
        {
            foreach (Protocol p in protocolTable.Values)
            {
                if (p.FriendlyName.Equals(friendlyName))
                {
                    return p;
                }
            }
            return null;
        }

        public static ArrayList GetProtocolList()
        {
            ArrayList list = new ArrayList(protocolTable.Values);
            list.Sort((IComparer) ProtocolComparer.GetInstance());
            return list;
        }

        public static void InitProtocolTable()
        {
            Type[] types = ReflectionHelper.GetChildTypes(typeof(Protocol));
            if (types == null)
            {
                return;
            }

            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsNotPublic)
                {
                    continue;
                }
                try
                {
                    object obj = ReflectionHelper.CreateInstance(type);
                    Protocol p = obj as Protocol;

                    string key = p.GetType().FullName;

                    if (protocolTable.ContainsKey(key))
                    {
                        //throw new ProtocolException(string.Format(Resources.ProtocolConflict, key));
                    }
                    else
                    {
                        protocolTable.Add(key, p);
                    }
                }
                catch (ProtocolException ex)
                {
                    ProtocolLogHelper.TraceMsg(ex.Message);
                }
                catch (Exception ex)
                {
                    ProtocolLogHelper.TraceMsg(string.Format(Resources.BuildProtocolTypeException, ex.Message, type.FullName));
                }
            }
        }

        #region IInitialization 成员

        public void Initialize()
        {
            InitProtocolTable();
        }

        #endregion
    }
}