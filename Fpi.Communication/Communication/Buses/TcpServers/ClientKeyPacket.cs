using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// 处理"获取与客户端绑定的唯一标识的包"的类
    /// 创建人：张永强
    /// 创建时间：2011-6-29
    /// </summary>
    public abstract class ClientKeyPacket
    {
        private const int BUFFERSIZE = 10240;
        private bool keyGetted;
        protected object clientKey;
        private byte[] sendCmd;

        protected byte[] dataBuffer = new byte[BUFFERSIZE];
        protected int bufferPos;

        public void Init()
        {
            this.sendCmd = ConstructSendCmd();
        }
        
        /// <summary>
        /// 是否已得到唯一标识
        /// </summary>
        public bool KeyGetted
        {
            get { return this.keyGetted; }
        }
        public object ClientKey
        {
            get { return this.clientKey; }
        }
        /// <summary>
        /// 获取唯一标识的发送命令
        /// </summary>
        public byte[] SendCmd
        {
            get { return this.sendCmd; }
        }
        /// <summary>
        /// 构造获取唯一标识的发送命令
        /// </summary>
        protected virtual byte[] ConstructSendCmd()
        {
            return new byte[0];
        }
        /// <summary>
        /// 接收数据,存放到缓冲区
        /// </summary>
        /// <param name="data"></param>
        public void PutData(byte[] data)
        {
            lock (dataBuffer)
            {
                if (bufferPos + data.Length >= BUFFERSIZE)
                {
                    bufferPos = 0;
                }

                Buffer.BlockCopy(data, 0, dataBuffer, bufferPos, data.Length);

                bufferPos += data.Length;
            }
            if (ParseData())
            {
                keyGetted = true;
            }
        }

        /// <summary>
        /// 解析收到的数据，以得到Key
        /// </summary>
        /// <returns>true:得到key,false:还未得到key</returns>
        protected virtual bool ParseData()
        {
            return false;
        }
        /// <summary>
        /// 解析完整的数据包，从中得到Key
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual object GetKeyfromData(byte[] data)
        {
            return null;
        }

    }
}
