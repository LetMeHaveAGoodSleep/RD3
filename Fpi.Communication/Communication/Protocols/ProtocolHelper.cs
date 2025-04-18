using System;
using System.Runtime.InteropServices;
using Fpi.Communication;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Communication.Exceptions;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Protocols.Interfaces;

namespace Fpi.Communication.Protocols
{
    public class ProtocolHelper
    {
        #region Call Win Api

        [DllImport("Kernel32.dll")]
        public static extern uint SetLocalTime(ref SYSTEMTIME lpSystemTime);

        public static void CorrectSystemTime(DateTime dateTime)
        {
            TimeSpan ts = dateTime - DateTime.Now;
            if (Math.Abs(ts.TotalMinutes) < 0.5)
            {
                return;
            }
            SYSTEMTIME sysTime = new SYSTEMTIME();
            sysTime.wYear = (ushort)dateTime.Year;
            sysTime.wMonth = (ushort)dateTime.Month;
            sysTime.wDay = (ushort)dateTime.Day;
            sysTime.wHour = (ushort)dateTime.Hour;
            sysTime.wMinute = (ushort)dateTime.Minute;
            sysTime.wSecond = (ushort)dateTime.Second;
            sysTime.wMilliseconds = (ushort)dateTime.Millisecond;
            sysTime.wDayOfWeek = (ushort)dateTime.DayOfWeek;
            SetLocalTime(ref sysTime);
        }

        #endregion

        #region Send Method

        public static bool SendData(Pipe pipe, IByteStream bs)
        {
            pipe.Send(null, bs);
            ProtocolLogHelper.TraceSendMsg(bs);
            return true;
        }

        #endregion

        #region Sender Timer Control

        //public static void StartSenderTimer(Protocol owner)
        //{
        //    Sender sender = owner.Sender;
        //    if (sender != null && sender.Timer != null)
        //    {
        //        sender.Timer.Start();
        //    }
        //}

        //public static void StopSenderTimer(Protocol owner)
        //{
        //    Sender sender = owner.Sender;
        //    if (sender != null && sender.Timer != null)
        //    {
        //        sender.Timer.Stop();
        //    }
        //}

        #endregion


        //#region Get Param

        //public static string GetDbField(VarNode var)
        //{
        //    return GetDbField(var.id);
        //}

        //public static string GetDbField(string varId)
        //{
        //    return "F" + varId;
        //}

        //#endregion

        #region Get protocol property

        /// <summary>
        /// 获取该通道所配置的某测量点所有输出项
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="mpId"></param>
        /// <returns></returns>
        public static NodeList GetOutputVarList(Pipe pipe, string mpId)
        {
            Property singleMpOutVar = GetProperty_OutputByMp(pipe, mpId);
            if (singleMpOutVar != null)
            {
                if (singleMpOutVar.propertys == null)
                {
                    singleMpOutVar.propertys = new NodeList();
                }
                return singleMpOutVar.propertys;
            }
            return null;
        }

        /// <summary>
        /// 获取指定输出通道在一个指定测量点上所有输出项的属性
        /// </summary>
        /// <param name="pipe"></param>
        /// <param name="mpId"></param>
        /// <returns></returns>
        public static Property GetProperty_OutputByMp(Pipe pipe, string mpId)
        {
            NodeList list = GetOutputList(pipe);
            if (list.Contains(mpId))
            {
                return list[mpId] as Property;
            }
            else
            {
                Property p = new Property(mpId, mpId);
                p.propertys = new NodeList();
                list.Add(p);
                return p;
            }
        }

        /// <summary>
        /// 获取该通道所配置的所有测量点所有输出项
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static NodeList GetOutputList(Pipe pipe)
        {
            Property prop = GetProperty_Output(pipe);
            if (prop.propertys == null)
            {
                prop.propertys = new NodeList();
            }
            return prop.propertys;
        }

        /// <summary>
        /// 获取指定输出通道在所有输出项的属性
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static Property GetProperty_Output(Pipe pipe)
        {
            Property prop = GetConfig(pipe, Protocol.PropertyName_OutputList);
            return prop;
        }

        public static string[] GetSelectMpArray(Pipe pipe)
        {
            string mpList = GetSelectMpList(pipe);
            if (string.IsNullOrEmpty(mpList))
            {
                return new string[0];
            }

            string[] rv = mpList.Split(',');
            return rv;
        }

        public static string GetSelectMpList(Pipe pipe)
        {
            string mpList = GetConfigValue(pipe, Protocol.PropertyName_SelectMp);
            //if (string.IsNullOrEmpty(mpList))
            //{ 
            //}
            return mpList;
        }

        public static void SetSelectMpList(Pipe pipe, string mpList)
        {
            Property prop = GetConfig(pipe, Protocol.PropertyName_SelectMp);
            prop.value = mpList;
        }

        public static int GetSenderInterval(Pipe pipe)
        {
            string str = GetConfigValue(pipe, Protocol.PropertyName_SenderInterval);
            int rv = 0;
            int.TryParse(str, out rv);
            rv /= 1000;
            return rv;
        }

        public static void SetSenderInterval(Pipe pipe, int interval)
        {
            Property prop = GetConfig(pipe, Protocol.PropertyName_SenderInterval);
            prop.value = (interval*1000).ToString();
            PortManager.GetInstance().Save();
        }

        public static string GetConfigValue(Pipe pipe, string propertyId, string defaultValue)
        {
            Property prop = GetConfig(pipe, propertyId);
            if (string.IsNullOrEmpty(prop.value))
            {
                prop.value = defaultValue;
            }
            return prop.value;
        }

        public static string GetConfigValue(Pipe pipe, string propertyId)
        {
            Property prop = GetConfig(pipe, propertyId);
            return prop.value;
        }

        public static void SetConfigValue(Pipe pipe, string propertyId, string value)
        {
            Property prop = GetConfig(pipe, propertyId);
            prop.value = value;
        }

        public static Property GetConfig(Pipe pipe, string propertyId)
        {
            Property protocolConfig = GetProtoclConfig(pipe);
            Property prop = protocolConfig.GetProperty(propertyId);
            if (prop == null)
            {
                prop = new Property(propertyId, propertyId);
                protocolConfig.AddProperty(prop);
            }
            return prop;
        }

        public static Property GetProtoclConfig(Pipe pipe)
        {
            if (pipe == null)
            {
                throw new ProtocolException("pipe is null:" + pipe.name);
            }
            Property prop = pipe.GetProtocolProperty();
            //if (prop == null)
            //{
            //    prop = new Property(Protocol.PropertyName_Protocol, Protocol.PropertyName_Protocol);
            //    pipe.AddProperty(prop);
            //}
            return prop;
        }

        #endregion

        #region 反控

        /*

        /// <summary>自动标定操作编号</summary>
        public const string AutoAdjustOpId = "OneGasCali";
        private static long opSerialNumber = 101;
        public static IRC_Informer GetOmaInformer(string mpId)
        {
            Pipe pipe = GetOmaPipe(mpId);
            if (pipe == null)
            {
                return null;
            }

            IPortOwner omaWebReceiver = pipe.PortOwner;
            if (omaWebReceiver == null)
            {
                return null;
            }

            if (omaWebReceiver is IRC_Informer)
            {
                return omaWebReceiver as IRC_Informer;
            }
            else
            {
                return null;
            }
        }

        /// <summary>查找该测量点对应OMA表的 pipe</summary>
        public static Pipe GetOmaPipe(string mpId)
        {
            NodeList pipes = PortManager.GetInstance().pipes;
            foreach (Pipe pipe in pipes)
            {
                if (pipe.id.StartsWith(mpId)
                    && pipe.id.ToLower().Contains("oma")
                    && pipe.target != null
                    && pipe.target.Contains("oma"))
                {
                    return pipe;
                }
            }

            return null;
        }

        */

        #endregion
    }

    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }
}