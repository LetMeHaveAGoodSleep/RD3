using System;
using System.Collections;
using System.Threading;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// ResendThread 的摘要说明。
    /// </summary>
    public class ResendThread
    {
		private const int DEFAULT_INTERVAL = 500;
		private bool isAlive = false;
		private int interval = DEFAULT_INTERVAL;
	
		private ArrayList ports = new ArrayList();
		private static ResendThread instatnce = new ResendThread();
	
		private ResendThread()
		{
			isAlive = true;

            Thread th = new Thread(new ThreadStart(Run));
            th.Priority = ThreadPriority.BelowNormal;
            th.IsBackground = true;
            th.Name = "GroupingPort Resend Thread";
            th.Start();
		}
	
		public static ResendThread GetInstance()
		{
			return instatnce;
		}
	
		public void SetInterval(int interval) 
		{
			this.interval = interval;
		}
	
		public void AddPort(GroupingPort port)
		{
            lock (ports)
            {
                ports.Add(port);
            }
		}
	
		public void RemovePort(GroupingPort port)
		{
            lock (ports)
            {
                ports.Remove(port);
            }
		}
	
		public void Exit() 
		{
			isAlive = false;
		}
	

	
		//超时重发监控线程
		public void Run() 
		{
			while (isAlive)
			{
				foreach (GroupingPort port in ports) 
				{
                    if (port != null && port.Connected)
                    {
                        port.Resend();
                    }
				}

                Thread.Sleep(interval);				
			}
        }
    }
}