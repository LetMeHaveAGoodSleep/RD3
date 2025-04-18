using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Util.Sundry;
using Fpi.Communication.Exceptions;
using Fpi.Xml;
using Fpi.Communication.Manager;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;
using Fpi.Properties;

namespace Fpi.Communication.Buses
{
    public class BusHelper : IInitialization
    {
        private static Hashtable busTable = new Hashtable();
        static BusHelper()
        { 
        }

        static public BaseBus CreateBus(string busTypeName, Property busProperty)
        {
            try
            {
                object ob = ReflectionHelper.CreateInstance(busTypeName);
                BaseBus bus = ob as BaseBus;
                bus.Init(busProperty);
                return bus;
            }
            catch
            {
                throw new CommunicationException(string.Format(Resources.CreateBusFailed,busTypeName));
            }
        }

        static public ICollection GetBusCollection()
        {
            return busTable.Values;
        }
        public static BaseBus GetBus(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
                return null;
            foreach (Type type in busTable.Keys)
            {
                if (type.FullName == typeFullName)
                {
                    return busTable[type] as BaseBus;
                }
            }
            return null;
        }

        #region IInitialization ≥…‘±

        public void Initialize()
        {
            InitBusTable();
        }

        public static void InitBusTable()
        {
            Type[] types = ReflectionHelper.GetChildTypes(typeof(BaseBus));
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
                    BaseBus bus = obj as BaseBus;

                    if (busTable.ContainsKey(type))
                    {
                        throw new CommunicationException(string.Format(Resources.RouterConflict, type.FullName));
                    }
                    else
                    {
                        busTable.Add(type, bus);
                    }
                }
                catch (CommunicationException ex)
                {
                    BusLogHelper.TraceBusMsg(ex.Message);
                }
                catch (Exception ex)
                {
                    BusLogHelper.TraceBusMsg(string.Format(Resources.BuildRouterExeption, ex.Message, type.FullName));
                }
            }
        }

        #endregion

    }
}
