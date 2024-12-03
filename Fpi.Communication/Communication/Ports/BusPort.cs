using System;
using System.Threading;
using Fpi.Communication.Buses;
using Fpi.Util.Exeptions;
using Fpi.Communication.Interfaces;
using Fpi.Properties;
using Fpi.Communication.Exceptions;
using System.IO;
using Fpi.Communication.Manager;
using Fpi.Util;

namespace Fpi.Communication.Ports
{
    public class BusPort : BasePort
    {
        private const int BUFFER_SIZE = 1088;
        private byte[] buffer = new byte[BUFFER_SIZE];
        private int errorCount = 0;
        private int MAX_ERROR_COUNT = 30;  //pan_xu modified 2014.6.6 change 0 to 30
        private IBus bus;
        private Thread thread;
        private Pipe pipe;  //add 2019.1.2

        public BusPort(IBus bus, Pipe pipe)
        {
            this.bus = bus;
            this.pipe = pipe;
            if ((bus != null) && (bus is BaseBus))
            {
                try
                {
                    MAX_ERROR_COUNT = Int32.Parse((bus as BaseBus).GetProperty("errorCount", "30")); // pan_xu modified 2014.6.6 change 0 to 30
                }
                catch (Exception ex)
                {
                    LogHelper.Debug(ex.Message + "：" + ex.StackTrace);
                }
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            try
            {
                if (connected)
                {
                    byte[] bytes = data.GetBytes();
                    if (bus.Write(bytes))
                    {
                        if (bus is BaseBus && pipe != null)
                        {
                            BusLogHelper.TraceBusSendMsg(pipe.id, bytes);
                        }
                        else
                        {
                            BusLogHelper.TraceBusSendMsg(bytes);
                        }
                        errorCount = 0;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }
            catch (Exception e)
            {
                errorCount++;
                CheckErrorCount();
                LogHelper.Debug("bus send exception: " + e.Message);
            }
            return null;
        }

        public override bool Open()
        {
            if (bus == null)
            {
                throw new PlatformException("bus is null !");
            }

            bool result = true;
            if (!connected)
            {
                result = bus.Open();
                connected = result;

                if (result)
                {
                    IPortOwner portOwner = PortOwner;
                    if (portOwner != null)
                    {
                        thread = new Thread(new ThreadStart(run));

                        thread.Name = "Bus ReadData Thread";

                        thread.Priority = ThreadPriority.BelowNormal;
                        thread.Start();
                    }
                }
            }

            return result;
        }

        public override bool Close()
        {
            if (bus == null)
            {
                throw new PlatformException("bus is null !");
            }

            bool result = true;
            if (connected)
            {
                connected = false;
                result = bus.Close();
            }

            return result;
        }

        private void run()
        {
            errorCount = 0;
            while (connected)
            {
                try
                {
                    int count = 0;
                    bus.Read(buffer, BUFFER_SIZE, ref count);
                    if (count > 0)
                    {
                        byte[] tempData = new byte[count];
                        Buffer.BlockCopy(buffer, 0, tempData, 0, count);
                        if (bus is BaseBus && pipe != null)
                        {
                            BusLogHelper.TraceBusRecvMsg(pipe.id, tempData);
                        }
                        else
                        {
                            BusLogHelper.TraceBusRecvMsg(tempData);
                        }

                        IPortOwner portOwner = PortOwner;
                        portOwner.Receive(bus, new ByteArrayWrap(tempData));
                        errorCount = 0;
                    }
                }
                //区分各类异常 pan_xu
                catch (CommunicationException cex)
                {
                    string error = string.Format(string.Format(Resources.BusReadException, (bus as BaseBus).FriendlyName, cex.Message));
                    LogHelper.Debug(error + "     " + cex.StackTrace);

                    errorCount++;
                    CheckErrorCount();
                }
                catch (IOException ioex)
                {
                    string error = string.Format(string.Format(Resources.BusReadException, (bus as BaseBus).FriendlyName, ioex.Message));
                    LogHelper.Debug(error + "     " + ioex.StackTrace);

                    errorCount++;
                    CheckErrorCount();
                }
                catch (ArithmeticException aex)
                {
                    string error = string.Format(string.Format(Resources.BusReadException, (bus as BaseBus).FriendlyName, aex.Message));
                    LogHelper.Debug(error + "     " + aex.StackTrace);

                    errorCount = 0;
                }
                catch (Exception e)
                {
                    string error = string.Format(string.Format(Resources.BusReadException, (bus as BaseBus).FriendlyName, e.Message));
                    LogHelper.Debug(error + "     " + e.StackTrace);

                    errorCount = 0;
                }
            }
        }

        private void CheckErrorCount()
        {
            if ((MAX_ERROR_COUNT > 0) && (errorCount >= MAX_ERROR_COUNT))
            {
                Close();
                IPortOwner portOwner = PortOwner;
                portOwner.OnDisconnecting(this);
            }
        }
    }
}