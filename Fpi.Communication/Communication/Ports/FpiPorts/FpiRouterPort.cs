using System;
using Fpi.Communication.Manager;
using Fpi.Instruments;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Properties;
using Fpi.Util;

namespace Fpi.Communication.Ports.FpiPorts
{
    public class FpiRouterPort : BasePort
    {
        public static readonly string PropertyName_Address = "address";


        public FpiRouterPort()
        {
        }

        //端口地址
        private byte address;

        public override void Init(BaseNode config)
        {
            base.Init(config);
            //获取端口地址
            address = StringUtil.ParseByte(GetProperty(PropertyName_Address, "0"));
        }

        protected override IByteStream PackData(object dest, IByteStream data)
        {
            byte[] destAddress = GetDestAddress((string) dest);
            byte[] sendData = data.GetBytes();

            sendData = FillDstAddress(destAddress, sendData);

            return new ByteArrayWrap(sendData);            
        }

        protected override IByteStream ParseData(object recvSource, ref object newSource, IByteStream data)
        {
            newSource = this;
            byte[] recvData = data.GetBytes();

            //目标路径长度
            int targetAddrLength = (int) recvData[0];
            //目标地址长度小于等于0
            if (targetAddrLength <= 0)
            {
                Log(string.Format(Resources.DestAddressLength, targetAddrLength));
                return null;
            }

            //目标路径地址
            byte[] targetAddr = new byte[targetAddrLength];
            Buffer.BlockCopy(recvData, 1, targetAddr, 0, targetAddrLength);

            //目标地址与当前地址不匹配
            if (targetAddr[0] != address)
            {
                Log(string.Format(Resources.DestAddressError, targetAddr[0]));
                return null;
            }

            //源路径长度
            int sourceAddrLength = recvData[targetAddrLength + 1];
            if (sourceAddrLength <= 0)
            {
                Log(string.Format(Resources.SourceAddressLength,sourceAddrLength));
                return null;
            }

            //源路径地址
            byte[] sourceAddr = new byte[sourceAddrLength];
            Buffer.BlockCopy(recvData, targetAddrLength + 2, sourceAddr, 0, sourceAddrLength);
            byte sourceAddress = sourceAddr[sourceAddr.Length - 1];

            //数据到达目标地址
            if (targetAddrLength == 1)
            {
                int startIndex = sourceAddrLength + targetAddrLength + 2;
                byte[] newData = new byte[recvData.Length - startIndex];
                Buffer.BlockCopy(recvData, startIndex, newData, 0, newData.Length);
                Instrument ins = InstrumentManager.GetInstance().GetInstrument(sourceAddress);
                newSource = ins.id;
                return new ByteArrayWrap(newData);
            }

                //数据路由
            else if (targetAddrLength > 1)
            {
                /*  pc f3 pda f2 , virtual address com1, com2 对应fc, fd(这里从f5开始)
                          1、把目标路径长度减1做为新的目标路径长度。

                          2、把目标地址区域第一目标去掉，其他目标向前顺次移动一个位置。

                          3、把源路径长度加1做为新的源路径长度。

                          4、把源地址区域的源地址顺次向后移动一个位置。

                          5、把本机在第一目标局域网内的地址作为第一源地址。

                          */

                //1、目标路径长度减1
                recvData[0] = (byte) (targetAddrLength - 1);
                //5、把本机在第一目标局域网内的地址作为第一源地址。

                recvData[targetAddrLength + 1] = recvData[1];
                //2、把目标地址区域第一目标去掉，其他目标向前顺次移动一个位置。

                Buffer.BlockCopy(recvData, 2, recvData, 1, targetAddrLength - 1);
                //3、把源路径长度加1做为新的源路径长度。

                recvData[targetAddrLength] = (byte) (sourceAddrLength + 1);

                byte routerAddress = recvData[1];
                Instrument ins = InstrumentManager.GetInstance().GetInstrument(routerAddress);

                //查找到路由设备对应的端口
                IPort port = FindRouterPort(ins.id);
                //路由数据发送
                //注意，这里对485等半双工的总线，没有进行同步处理，可能导致数据冲突，可以在485bus的上层某个port里面处理同步功能
                if (port != null && port.LowerPort != null)
                {
                    TimeoutByteStream tbs = new TimeoutByteStream(new ByteArrayWrap(recvData), 0, 0);
                    port.LowerPort.Send(ins.id, tbs);
                }
                return null;
            }

            return null;
        }

        IPort FindRouterPort(string insId)
        {
            Pipe pipe = PortManager.GetInstance().FindSendPipe(insId);
            if (pipe != null)
            {
                return pipe.FindPort(typeof(FpiRouterPort).FullName);
            }
            return null;
        }


        private const string MessageType = "FpiRouterPort";

        /// <summary>
        /// 485协议路由数据处理
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>返回： True，已经完成路由转发 False，表示没有完成路由转发，data为本地接收数据。</returns>
        private void Log(string msg)
        {
            LogHelper.Debug(msg);
        }

        //根据设备id获取目标地址
        private byte[] GetDestAddress(string instrumentId)
        {
            byte[] destAddress;
            destAddress = new byte[32]; //最多32层路由
            int addressCount = 0;
            string tempInstrumentId = instrumentId;
            while (true)
            {
                Instrument instr = (Instrument) InstrumentManager.GetInstance().instruments[tempInstrumentId];
                if (instr == null)
                {
                    throw new InstrumentException("instrument not found: " + tempInstrumentId);
                }
                destAddress[addressCount++] = (byte)instr.address;
                if ((instr.router != null) && (instr.router.Trim().Length > 0))
                {
                    tempInstrumentId = instr.router;
                }
                else
                {
                    break;
                }
            }

            /*
             * pan_xu 治理设施，路由信息保存在属性routers中，更加明显直观，而且支持单点设备多路由,比如2-6-5,3-6-4
             * 
             */
            Instrument finalInstr = (Instrument)InstrumentManager.GetInstance().instruments[tempInstrumentId];
            string routers = finalInstr.GetPropertyValue("routers");
            if (!string.IsNullOrEmpty(routers))
            {
                string[] routerArr = routers.Split(',');  //地址序列
                foreach (string i in routerArr)
                {
                    destAddress[addressCount++] = byte.Parse(i);
                }
            }
             

            byte[] result = new byte[addressCount];
            //目标地址反转
            for (int i = 0; i < addressCount; i++)
            {
                result[i] = destAddress[addressCount - i - 1];
            }
            return result;
        }

        /// <summary>填充目标地址</summary>
        private byte[] FillDstAddress(byte[] destAddress, byte[] sendData)
        {
            byte[] data = new byte[3 + destAddress.Length + sendData.Length];
            data[0] = (byte) destAddress.Length;
            Buffer.BlockCopy(destAddress, 0, data, 1, destAddress.Length);
            data[destAddress.Length + 1] = (byte) 1;
            data[destAddress.Length + 2] = address;
            Buffer.BlockCopy(sendData, 0, data, destAddress.Length + 3, sendData.Length);
            return data;
        }
    }
}