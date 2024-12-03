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
    /// Э���е�����������
    /// ����������չ�Ķ�ʱ������ִ����չ���񣬷���ִ��Ĭ�϶�ʱ����
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

        /// <summary>��չ�Ķ�ʱ����</summary>
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
        /// ������������Э��Ĭ�ϵķ��Ͷ�ʱ��
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
            //timer.description = string.Format("{0}_������ʱ��[{1}]", pipe.name,owner.FriendlyName);
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
                    //������չ����
                    extendSender.SendData(pipe);
                }
                else
                {
                    //Ĭ�ϵĶ�ʱ����
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
        /// ������Э�����չ��ʱ����
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
        
        #region IDisposable ��Ա

        public virtual void Dispose()
        {
            DisposeTimer();
            if (pipe != null)
            {
                pipe.PipeValidChangedEvent -= new PipeValidChangedHandler(pipe_PipeValidChangedEvent);
            }
        }

        #endregion

        #region �������Լ�����

        /// <summary>�Ƿ���չ��ʱ����</summary>
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

        #region ISender ��Ա

        abstract public void SendData(Pipe pipe);

        #endregion
    }
}