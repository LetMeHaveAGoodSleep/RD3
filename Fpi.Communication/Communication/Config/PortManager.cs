using System;
using System.Collections;
using Fpi.Communication.Exceptions;
using Fpi.Communication.Ports;
using Fpi.Util.Interfaces.Initialize;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using System.Threading;
using Fpi.Properties;
using Fpi.Util;

namespace Fpi.Communication.Manager
{
    public class PortManager : BaseNode, IInitialization,IDisposable
    {
        #region Fields ＆ Construction method

        public NodeList pipes = new NodeList();

        private Hashtable pipeTable = new Hashtable();
        private Hashtable instrumentTable = new Hashtable();

        private PortManager()
        {
            loadXml();
        }
        ~PortManager()
        {
            Dispose();
        }
        private static object syncObj = new object();
        private static PortManager instance = null;
        public static PortManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new PortManager();
                }
            }
            return instance;
        }
        public static void ReLoad()
        {
            lock (syncObj)
            {
                //光把实例instance置为空,则实例释放工作不彻底,故调用Dispose. modified by zhangyq.2010.4.8.
                //instance = null;
                GetInstance().Dispose();
                GetInstance().Initialize();
            }
        }
       

        #endregion

        #region Open\Close\Reset\Send\Add\Remove\Clear

        public void Open()
        {
            foreach (Pipe pipe in pipes)
            {
                Open(pipe);            
            }
        }
        public void Close()
        {
            foreach (Pipe pipe in pipes)
            {
                pipe.Close();
            }
        }

        public bool Open(string pipeId)
        {
           Pipe pipe = GetPipe(pipeId);
           if (pipe != null)
           {
               return Open(pipe);
           }
           return false;
        }
        public bool Open(Pipe pipe)
        {
            if (!pipeTable.ContainsKey(pipe.id))
            {
                //未创建过链路的要构造链路
                this.BuildPipe(pipe);
            }
            return pipe.Open();
        }

        public bool Close(string pipeId)
        {
            Pipe pipe = GetPipe(pipeId);
            if (pipe != null)
            {
                return Close(pipe);
            }
            return false;
        }
        public bool Close(Pipe pipe)
        {
            return pipe.Close();
        }

        ////add by caoxu 2010.11.19

        private object syncPipesEditObject = new object();

        ///// <summary>
        ///// 增加一个pipe
        ///// </summary>
        ///// <param name="pipe"></param>
        //public void AddPipe(Pipe pipe)
        //{
        //    lock (syncPipesEditObject)
        //    {
        //        this.pipes.Add(pipe);
        //    }
        //    this.Save();
        //}

        ///// <summary>
        ///// 删除一个pipe
        ///// </summary>
        ///// <param name="pipe"></param>
        //public void RemovePipe(Pipe pipe)
        //{
        //    this.Close(pipe);
        //    lock (syncPipesEditObject)
        //    {
        //        this.pipes.Remove(pipe);
        //    }
        //    this.Save();
        //}

        ///// <summary>
        ///// 清空pipes列表
        ///// </summary>
        ///// <param name="pipe"></param>
        //public void ClearPipes()
        //{
        //    this.Close();
        //    lock (syncPipesEditObject)
        //    {
        //        this.pipes.Clear();
        //    }
        //    this.Save();
        //}

        //add end

        public void Reset()
        {
            Close();
            Initialize();
        }

        public object Send(string instrumentId, IByteStream data)
        {
            Pipe pipe = FindSendPipe(instrumentId);
            if (pipe != null)
            {
                return pipe.Send(instrumentId, data);
            }
            return null;
        }
        public void Send(string instrumentId, IByteStream data, IExceptionReceivable receiver)
        {
            new SendThread(instrumentId, data, receiver).Start();
        }

        #endregion

        #region GetPipe\FindSendPipe

        public Pipe GetPipe(string pipeId)
        {
            if (pipes != null && pipes[pipeId] != null)
            {
                return pipes[pipeId] as Pipe;
            }
            return null;
        }

        public Pipe FindSendPipe(string instrumentId)
        {
            if (instrumentTable.ContainsKey(instrumentId))
            {
                return instrumentTable[instrumentId] as Pipe;
            }
            return null;
        }

        ////根据设备id，端口类名查找相应的port
        //public IPort FindPort(string instrumentId, string className)
        //{
        //    IPort port = (IPort) instrumentTable[instrumentId];
        //    while (port != null)
        //    {
        //        //find class name
        //        if (port.GetType().FullName == className)
        //        {
        //            return port;
        //        }

        //        port = port.LowerPort;
        //    }

        //    throw new CommunicationException(className + " not found in " + instrumentId);
        //}

        //public IPort FindPort(Pipe pipe, string className)
        //{
        //    IPort port = GetPort(pipe.id);
        //    while (port != null)
        //    {
        //        if (port.GetType().FullName == className)
        //        {
        //            return port;
        //        }

        //        port = port.LowerPort;
        //    }

        //    throw new CommunicationException(className + " not found in " + pipe.name);
        //}

        #endregion

        #region IInitialization 成员

        Timer timerReConnect;

        public void Initialize()
        {
            lock (this)
            {
                BuildPipes();

                foreach (Pipe pipe in pipeTable.Values)
                {
                    //added by zf 是否自动启动
                    if (pipe.autoConnect == true)
                        pipe.Open();
                }

                timerReConnect = new Timer(new TimerCallback(ReConnectFunc), null, Timeout.Infinite, Timeout.Infinite);
                timerReConnect.Change(5000, 1000 * 30);
            }
        }

        private void BuildPipes()
        {
            instrumentTable.Clear();
            pipeTable.Clear();
            foreach (Pipe pipe in pipes)
            {
                BuildPipe(pipe);
            }
        }

        private bool BuildPipe(Pipe pipe)
        {
            try
            {
                lock (pipe)
                {
                    bool res = pipe.BuildLinkLayer();
                    if (res)
                    {
                        foreach (string insId in pipe.targets)
                        {
                            instrumentTable.Add(insId, pipe);
                        }
                        pipeTable.Add(pipe.id, pipe);
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }

        bool isReConnentting = false;
        private void ReConnectFunc(object obj)
        {
            if (isReConnentting)
                return;

            isReConnentting = true;
            try
            {
                lock (syncPipesEditObject)
                {
                    foreach (Pipe pipe in pipes)
                    {
                        //若通道已经启用且已配置为自动重连
                        if (pipe.autoReConnect && pipe.valid)
                        {
                            //若通道断开，则重新连接
                            if (!pipe.Connected)
                            {
                                pipe.Open();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
            isReConnentting = false;
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            lock (syncObj)
            {
                if (timerReConnect != null)
                {
                    timerReConnect.Change(Timeout.Infinite, Timeout.Infinite);
                    timerReConnect = null;
                }

                Close();
                instance = null;
            }
        }

        #endregion

        #region Dynamic add by zf
        /// <summary>
        /// 将仪器注册到pipe上
        /// </summary>
        /// <param name="pipeId"></param>
        /// <param name="instrumentId"></param>
        public bool RegisterInstrument(string pipeId, string instrumentId, bool save)
        {
            try
            {
                Pipe pipe = (Pipe)pipes.FindNode(pipeId);

                lock (pipe)
                {
                    if (!pipe.targets.Contains(instrumentId))
                    { pipe.targets.Add(instrumentId); }
                    else
                    {
                        pipe.targets = new ArrayList();
                        pipe.targets.Add(instrumentId);
                    }
                }
                if (!instrumentTable.ContainsKey(instrumentId))
                {
                    instrumentTable.Add(instrumentId, pipe);
                }
                if (save)
                {
                    this.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 将仪器从pipe注销
        /// </summary>
        /// <param name="pipeId"></param>
        /// <param name="instrumentId"></param>
        public bool UnregisterInstrument(string pipeId, string instrumentId, bool save)
        {
            try
            {
                Pipe pipe = (Pipe)pipes.FindNode(pipeId);

                lock (pipe)
                {
                    if (pipe.targets.Contains(instrumentId))
                    {
                        pipe.targets.Remove(instrumentId);
                    }
                }

                instrumentTable.Remove(instrumentId);
                if (save)
                {
                    this.Save();
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 动态增加Pipe
        /// </summary>
        public bool AddPipe(Pipe pipe)
        {
            return AddPipe(pipe, true);
        }
        /// <summary>
        /// 动态增加Pipe
        /// </summary>
        /// <param name="pipeId"></param>
        public bool AddPipe(Pipe pipe, bool save)
        {
            try
            {
                if (!pipes.Contains(pipe.id))
                {
                    pipes.Add(pipe);
                    lock (pipeTable)
                    {
                        if (!pipeTable.Contains(pipe.id))
                        {
                            bool res = pipe.BuildLinkLayer();

                            if (res)
                            {
                                foreach (string insId in pipe.targets)
                                {
                                    if (!instrumentTable.Contains(insId))
                                        instrumentTable.Add(insId, pipe);
                                }
                                pipeTable.Add(pipe.id, pipe);
                            }

                            if (save)
                            {
                                this.Save();
                            }
                            return res;
                        }
                    }
                }
                //pipe已经存在
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        public bool RemovePipe(Pipe pipe)
        {
            return RemovePipe(pipe,true);
        }
        /// <summary>
        /// 动态删除Pipe 
        /// </summary>
        /// <param name="pipeId"></param> 
        public bool RemovePipe(Pipe pipe, bool save)
        {
            try
            {
                if (pipes.Contains(pipe.id))
                {
                    lock (pipeTable)
                    {
                        if (pipeTable.Contains(pipe.id))
                        {
                            foreach (string instrumentId in pipe.targets)
                            {
                                if (instrumentTable.Contains(instrumentId))
                                    instrumentTable.Remove(instrumentId);
                            }
                            pipeTable.Remove(pipe.id);
                        }
                    }
                    pipes.Remove(pipe);
                    if (save)
                    {
                        this.Save();
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }

        #endregion

    }
}