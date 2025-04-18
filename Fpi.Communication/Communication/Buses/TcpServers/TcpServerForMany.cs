using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Communication.Exceptions;
using Fpi.Util.WinApiUtil;
using Fpi.Properties;
using System.Collections.Generic;
using System.Collections;


namespace Fpi.Communication.Buses
{
    public class TcpServerForMany : BaseBus
    {
        public static readonly string PropertyName_Port = "port";

        private int port;
        private TcpListener listener;

        private Hashtable _tcpClientTable = new Hashtable();

        private int _maxConnections = 100;

        private Hashtable _tcpClientReportTimeTable = new Hashtable();

        private Timer _timer = null;

        private void StartListening()
        {
            connected = true;
            listener = new TcpListener(IPAddress.Any, port);

            bool listening = false;

            _timer = new Timer(new TimerCallback(this.TimerFunc), null, 10000, 5000);

            while (connected)
            {
                try
                {
                    if (!listening)
                    {
                        listener.Start();
                        listening = true;
                    }

                    TcpClient tcpClient = listener.AcceptTcpClient();
                    if (_tcpClientTable.Count > _maxConnections)
                    {
                        throw new Exception("超过最大连接数");
                    }

                    int hashcode = tcpClient.GetHashCode();

                    lock (_tcpClientTable)
                    {
                        _tcpClientTable.Add(hashcode, tcpClient);
                    }

                    lock (_tcpClientReportTimeTable)
                    {
                        _tcpClientReportTimeTable[tcpClient.GetHashCode()] = DateTime.Now;
                    }

                }
                catch (Exception ex)
                {
                    BusLogHelper.TraceBusMsg(string.Format(Resources.TcpListenError, ex.Message));
                }
            }
        }

        public override string FriendlyName
        {
            get { return "TCP 服务端（多客户端）";  }
        }

        #region IConnector 成员

        public override void Init(Fpi.Xml.BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.TcpLocalPortNotConfig);

            base.Init(config);
            port = Int32.Parse(config.GetPropertyValue(PropertyName_Port));
        }

        public override bool Write(byte[] buf)
        {
            return true;
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            if (_tcpClientTable.Count <= 0)
                return true;
            List<TcpClient> tcpClients = new List<TcpClient>();
            lock (_tcpClientTable)
            {
                foreach (int key in _tcpClientTable.Keys)
                {
                    tcpClients.Add((TcpClient)(_tcpClientTable[key]));
                }
            }

            foreach (TcpClient tcpClient in tcpClients)
            {
                try
                {
                    NetworkStream netStream = tcpClient.GetStream();
                    if (netStream == null || !netStream.DataAvailable)
                    {
                        Thread.Sleep(10);  //避免频繁耗费CPU
                        continue;
                    }
                    bytesread = netStream.Read(buf, 0, buf.Length);

                    lock (_tcpClientReportTimeTable)
                    {
                        _tcpClientReportTimeTable[tcpClient.GetHashCode()] = DateTime.Now;
                    }

                    return true;
                }
                catch (System.IO.IOException ioEx)
                {
                    //注释日志调试输出，避免频繁文件写操作
                    //BusLogHelper.TraceBusMsg(string.Format(Resources.TcpListenError, ioEx.Message));
                    continue;
                }
                catch (Exception ex)
                {
                    //注释日志调试输出，避免频繁文件写操作
                    //BusLogHelper.TraceBusMsg(string.Format(Resources.TcpListenError, ex.Message));
                    continue;
                }
            }

            return false;
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            bool flag = _Open();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpPortSucceed, port));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpPortFailed, port));
            }

            connected = flag;
            return flag;
        }

        private bool _Open()
        {
            (new Thread(new ThreadStart(StartListening))).Start();
            return true;
        }

        public override bool Close()
        {
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpPortSucceed, port));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpPortFailed, port));
            }

            connected = !flag;
            return flag;
        }

        private bool _Close()
        {
            connected = false;

            if (listener != null)
            {
                listener.Stop();
            }

            List<TcpClient> tcpClients = new List<TcpClient>();

            try
            {
                lock (_tcpClientTable)
                {
                    foreach (int key in _tcpClientTable.Keys)
                    {
                        tcpClients.Add((TcpClient)(_tcpClientTable[key]));
                    }
                }
            }
            catch
            {
            }

            foreach (TcpClient tcpClient in tcpClients)
            {
                try
                {
                    NetworkStream netStream = tcpClient.GetStream();
                    if (netStream == null)
                        continue;
                    netStream.Close();
                    netStream = null;

                    tcpClient.Close();
                }
                catch
                {
                    continue;
                }

            }

            _tcpClientTable.Clear();
            _tcpClientReportTimeTable.Clear();

            return true;
        }

        #endregion

        private void TimerFunc(object obj)
        {
            if (_tcpClientReportTimeTable.Count <= 0)
                return;
            List<int> corrupt = new List<int>();

            lock (_tcpClientReportTimeTable)
            {
                foreach (int key in _tcpClientReportTimeTable.Keys)
                {
                    try
                    {
                        DateTime time = (DateTime)(_tcpClientReportTimeTable[key]);
                        TimeSpan ts = DateTime.Now - time;
                        if (ts.Minutes > 15)
                        {
                            if (!_tcpClientTable.Contains(key))
                                continue;
                            TcpClient tcpClient = (TcpClient)(_tcpClientTable[key]);


                            NetworkStream netStream = tcpClient.GetStream();
                            if (netStream == null)
                                continue;
                            netStream.Close();
                            netStream = null;

                            tcpClient.Close();

                        }
                    }
                    catch (System.ObjectDisposedException dex)
                    {
                        corrupt.Add(key);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        corrupt.Add(key);
                        continue;
                    }
                }
            }

            try
            {
                lock (_tcpClientReportTimeTable)
                {
                    foreach (int tcpClient in corrupt)
                    {
                        if (_tcpClientReportTimeTable.Contains(tcpClient))
                            _tcpClientReportTimeTable.Remove(tcpClient);
                    }
                }
            }
            catch
            {

            }

            try
            {
                lock (_tcpClientTable)
                {
                    foreach (int tcpClient in corrupt)
                    {
                        if (_tcpClientTable.Contains(tcpClient))
                            _tcpClientTable.Remove(tcpClient);
                    }
                }
            }
            catch
            {

            }
        }
    }
}