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

        //�˿ڵ�ַ
        private byte address;

        public override void Init(BaseNode config)
        {
            base.Init(config);
            //��ȡ�˿ڵ�ַ
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

            //Ŀ��·������
            int targetAddrLength = (int) recvData[0];
            //Ŀ���ַ����С�ڵ���0
            if (targetAddrLength <= 0)
            {
                Log(string.Format(Resources.DestAddressLength, targetAddrLength));
                return null;
            }

            //Ŀ��·����ַ
            byte[] targetAddr = new byte[targetAddrLength];
            Buffer.BlockCopy(recvData, 1, targetAddr, 0, targetAddrLength);

            //Ŀ���ַ�뵱ǰ��ַ��ƥ��
            if (targetAddr[0] != address)
            {
                Log(string.Format(Resources.DestAddressError, targetAddr[0]));
                return null;
            }

            //Դ·������
            int sourceAddrLength = recvData[targetAddrLength + 1];
            if (sourceAddrLength <= 0)
            {
                Log(string.Format(Resources.SourceAddressLength,sourceAddrLength));
                return null;
            }

            //Դ·����ַ
            byte[] sourceAddr = new byte[sourceAddrLength];
            Buffer.BlockCopy(recvData, targetAddrLength + 2, sourceAddr, 0, sourceAddrLength);
            byte sourceAddress = sourceAddr[sourceAddr.Length - 1];

            //���ݵ���Ŀ���ַ
            if (targetAddrLength == 1)
            {
                int startIndex = sourceAddrLength + targetAddrLength + 2;
                byte[] newData = new byte[recvData.Length - startIndex];
                Buffer.BlockCopy(recvData, startIndex, newData, 0, newData.Length);
                Instrument ins = InstrumentManager.GetInstance().GetInstrument(sourceAddress);
                newSource = ins.id;
                return new ByteArrayWrap(newData);
            }

                //����·��
            else if (targetAddrLength > 1)
            {
                /*  pc f3 pda f2 , virtual address com1, com2 ��Ӧfc, fd(�����f5��ʼ)
                          1����Ŀ��·�����ȼ�1��Ϊ�µ�Ŀ��·�����ȡ�

                          2����Ŀ���ַ�����һĿ��ȥ��������Ŀ����ǰ˳���ƶ�һ��λ�á�

                          3����Դ·�����ȼ�1��Ϊ�µ�Դ·�����ȡ�

                          4����Դ��ַ�����Դ��ַ˳������ƶ�һ��λ�á�

                          5���ѱ����ڵ�һĿ��������ڵĵ�ַ��Ϊ��һԴ��ַ��

                          */

                //1��Ŀ��·�����ȼ�1
                recvData[0] = (byte) (targetAddrLength - 1);
                //5���ѱ����ڵ�һĿ��������ڵĵ�ַ��Ϊ��һԴ��ַ��

                recvData[targetAddrLength + 1] = recvData[1];
                //2����Ŀ���ַ�����һĿ��ȥ��������Ŀ����ǰ˳���ƶ�һ��λ�á�

                Buffer.BlockCopy(recvData, 2, recvData, 1, targetAddrLength - 1);
                //3����Դ·�����ȼ�1��Ϊ�µ�Դ·�����ȡ�

                recvData[targetAddrLength] = (byte) (sourceAddrLength + 1);

                byte routerAddress = recvData[1];
                Instrument ins = InstrumentManager.GetInstance().GetInstrument(routerAddress);

                //���ҵ�·���豸��Ӧ�Ķ˿�
                IPort port = FindRouterPort(ins.id);
                //·�����ݷ���
                //ע�⣬�����485�Ȱ�˫�������ߣ�û�н���ͬ���������ܵ������ݳ�ͻ��������485bus���ϲ�ĳ��port���洦��ͬ������
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
        /// 485Э��·�����ݴ���
        /// </summary>
        /// <param name="data">����</param>
        /// <returns>���أ� True���Ѿ����·��ת�� False����ʾû�����·��ת����dataΪ���ؽ������ݡ�</returns>
        private void Log(string msg)
        {
            LogHelper.Debug(msg);
        }

        //�����豸id��ȡĿ���ַ
        private byte[] GetDestAddress(string instrumentId)
        {
            byte[] destAddress;
            destAddress = new byte[32]; //���32��·��
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
             * pan_xu ������ʩ��·����Ϣ����������routers�У���������ֱ�ۣ�����֧�ֵ����豸��·��,����2-6-5,3-6-4
             * 
             */
            Instrument finalInstr = (Instrument)InstrumentManager.GetInstance().instruments[tempInstrumentId];
            string routers = finalInstr.GetPropertyValue("routers");
            if (!string.IsNullOrEmpty(routers))
            {
                string[] routerArr = routers.Split(',');  //��ַ����
                foreach (string i in routerArr)
                {
                    destAddress[addressCount++] = byte.Parse(i);
                }
            }
             

            byte[] result = new byte[addressCount];
            //Ŀ���ַ��ת
            for (int i = 0; i < addressCount; i++)
            {
                result[i] = destAddress[addressCount - i - 1];
            }
            return result;
        }

        /// <summary>���Ŀ���ַ</summary>
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