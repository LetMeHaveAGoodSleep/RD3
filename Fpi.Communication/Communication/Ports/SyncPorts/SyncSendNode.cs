using System;
using Fpi.Util.Sundry;

namespace Fpi.Communication.Ports.SyncPorts
{
    /// <summary>
    /// 7B7D开始和结束的帧。
    /// </summary>
    public class SyncSendNode
    {
        private bool isSyncSending;
        private object result;


        private System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);


        public SyncSendNode()
        {
            Init();
        }

        ~SyncSendNode()
        { 

            are.Close();
            are = null;

        }

        public void Init()
        {
            this.isSyncSending = false;
            this.result = null;
        }

        public bool IsSyncSending()
        {
            return isSyncSending;
        }

        public void SetSyncSending(bool isSyncSending)
        {
            this.isSyncSending = isSyncSending;
        }

        public object GetResult()
        {
            return result;
        }

        public void SetResult(object result)
        {
            this.result = result;
        }

        public bool WaitOne(int time)
        {

            return are.WaitOne(time, true);
           
        }

        public bool Set()
        {

            return are.Set();

        }
    }
}