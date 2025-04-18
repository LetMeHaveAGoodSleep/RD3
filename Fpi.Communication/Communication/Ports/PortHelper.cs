using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Util.Sundry;
using Fpi.Communication.Exceptions;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;
using Fpi.Properties;

namespace Fpi.Communication.Ports
{
    public class PortHelper //: IInitialization
    {
        private static Hashtable portTable = new Hashtable();
        static PortHelper()
        { 
        }

        static public IPort CreatePort(string portTypeName, Property portProperty)
        {
            try
            {
                object ob = ReflectionHelper.CreateInstance(portTypeName);
                IPort port = ob as IPort;
                port.Init(portProperty);
                return port;
            }
            catch
            {
                throw new CommunicationException(string.Format(Resources.CreatePortFailed, portTypeName));
            }
        }


        //static public ArrayList GetPortList()
        //{
        //    ArrayList list = new ArrayList(portTable.Values);
        //    return list;
        //}
        //public static BasePort GetPort(string typeFullName)
        //{
        //    foreach (Type type in portTable.Keys)
        //    {
        //        if (type.FullName == typeFullName)
        //        {
        //            return portTable[type] as BasePort;
        //        }
        //    }
        //    return null;
        //}


        #region IInitialization ≥…‘±

        public void Initialize()
        {
            Type[] types = ReflectionHelper.GetChildTypes(typeof(BasePort));
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
                    BasePort port = obj as BasePort;

                    if (portTable.ContainsKey(type))
                    {
                        throw new CommunicationException(string.Format(Resources.PortTypeConflict,type.FullName));
                    }
                    else
                    {
                        portTable.Add(type, port);
                    }
                }
                catch (CommunicationException ex)
                {
                    PortLogHelper.TracePortMsg(ex.Message);
                }
                catch (Exception ex)
                {
                    PortLogHelper.TracePortMsg(string.Format(Resources.BuildPortTypeException, ex.Message, type.FullName));
                }
            }
        }

        #endregion

    }
}
