using System;
using System.Text;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Protocols.Interfaces;
using Fpi.Util.Reflection;
using System.Threading;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// 协议中的主动发送器
    /// 若配置了扩展的定时任务，则执行扩展任务，否则执行默认定时任务
    /// </summary>
    public abstract class Sender : ProtocolComponent, ISender,IDisposable
    {
        public Sender()
        {
        }
        ~Sender()
        {
            Dispose();
        }

        /// <summary>扩展的定时任务</summary>
        protected ISender extendSender = null;
        protected Timer timerSender = null;
        protected bool canExtended = false;
        bool isRun = false;

        protected override void ActionPipe(Pipe pipe)
        {
            if (pipe != null)
            {
                extendSender = CreateExtendSender(pipe);
                pipe.PipeValidChangedEvent += new PipeValidChangedHandler(pipe_PipeValidChangedEvent);
            }
            else
            {
                extendSender = null;
                DisposeTimer();
            }
        }

        protected virtual void pipe_PipeValidChangedEvent(bool valid)
        {
            if (valid)
            {
                StartTimer(pipe);
            }
            else
            {
                DisposeTimer();
            }
        }

        /// <summary>
        /// 创建并启动该协议默认的发送定时器
        /// </summary>
        private void StartTimer(Pipe pipe)
        {
            if (timerSender == null)
            {
                timerSender = new Timer(new TimerCallback(SendFunc), pipe, Timeout.Infinite, Timeout.Infinite);
            }
            StartTimer();

            //if (timer == null)
            //{
            //    timer = new Timer();
            //    timer.timespan = 60000;
            //    timer.autoStart = true;
            //    timer.runImmediately = true;
            //    timer.SetTimerAction(this);
            //}
            //timer.description = string.Format("{0}_主发定时器[{1}]", pipe.name,owner.FriendlyName);
            //ProtocolDesc desc = owner.ProtocolDesc;
            //if (desc != null)
            //{
            //    timer.timespan = desc.SenderInterval;
            //    timer.Start();
            //}

            //TimerManager.GetInstance().RegisterTimer(timer);
        }

        private void SendFunc(object obj)
        {
            Pipe pipe = obj as Pipe;
            if (isRun || pipe == null || !pipe.Connected)
            {
                return;
            }

            isRun = true;

            try
            {
                if (extendSender != null)
                {
                    //若有扩展任务
                    extendSender.SendData(pipe);
                }
                else
                {
                    //默认的定时任务
                    SendData(pipe);
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("{0} sender exception:{1}", owner, ex.Message);
                throw new Exception(error);
            }
            finally
            {
                isRun = false;
            }
        }

        private void DisposeTimer()
        {
            if (timerSender != null)
            {
                StopTimer();
                timerSender = null;
            }
            //if (timer != null)
            //{
            //    TimerManager.GetInstance().StopTimer(timer);
            //    TimerManager.GetInstance().UnRegisterTimer(timer);
            //    timer = null;
            //}
        }

        /// <summary>
        /// 创建该协议的扩展定时任务
        /// </summary>
        /// <returns></returns>
        private ISender CreateExtendSender(Pipe pipe)
        {
            if (!canExtended)
                return null;

            string typeName = pipe.GetCustomSender();
            if (typeName != null)
            {
                return ReflectionHelper.CreateInstance(typeName) as ISender;
            }
            return null;
        }
        
        #region IDisposable 成员

        public virtual void Dispose()
        {
            DisposeTimer();
            if (pipe != null)
            {
                pipe.PipeValidChangedEvent -= new PipeValidChangedHandler(pipe_PipeValidChangedEvent);
            }
        }

        #endregion

        #region 公开属性及方法

        /// <summary>是否扩展定时任务</summary>
        public bool CanExtended
        {
            get { return canExtended; }
        }

        public void StartTimer()
        {
            if (timerSender != null)
            {
                if (owner.ProtocolDesc != null && owner.ProtocolDesc.SenderInterval > 0)
                {
                    timerSender.Change(1000, owner.ProtocolDesc.SenderInterval);
                }
            }
        }

        public void StopTimer()
        {
            if (timerSender != null)
            {
                timerSender.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        #endregion       

        #region ISender 成员

        abstract public void SendData(Pipe pipe);

        #endregion
    }
}