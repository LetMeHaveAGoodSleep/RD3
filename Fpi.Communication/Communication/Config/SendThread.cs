using System;
using System.Threading;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Manager
{
    /// <summary>
    /// ·¢ËÍÏß³Ì
    /// </summary>
    internal class SendThread
    {
        private string instrumentId;
        private IByteStream data;
        private IExceptionReceivable exceptionReceiver;


        public SendThread(string instrumentId, IByteStream data, IExceptionReceivable receiver)
        {
            this.instrumentId = instrumentId;
            this.data = data;
            this.exceptionReceiver = receiver;
        }

        public void Start()
        {
            Thread receiveThread = new Thread(new ThreadStart(SendThreadFunc));
            receiveThread.Priority = ThreadPriority.BelowNormal;
            receiveThread.Start();
        }

        private void SendThreadFunc()
        {
            try
            {
                object result = PortManager.GetInstance().Send(instrumentId, data);
                if (exceptionReceiver != null)
                {
                    exceptionReceiver.Receive(instrumentId, result, null);
                }
            }
            catch (Exception ex)
            {
                if (exceptionReceiver != null)
                {
                    exceptionReceiver.Receive(instrumentId, data, ex);
                }
            }
        }
    }
}