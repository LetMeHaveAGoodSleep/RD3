using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RD3.Shared
{
    public class CustomTcpClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected = false;
        private readonly object _lockObject = new object();
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly int _keepAliveTime = 60 * 1000; // 1分钟无活动后开始发送保活探测
        private readonly int _keepAliveInterval = 1000; // 每隔1秒发送一次保活探测
        private readonly int _keepAliveCount = 5; // 发送5次探测后关闭连接
        private readonly int _reconnectIntervalMilliseconds = 3000; // 重连间隔时间（毫秒）
        private byte[] _readBuffer = new byte[1024]; // 用于接收数据的缓冲区
        private CancellationTokenSource _cancellationTokenSource; // 取消标记
        private Thread _receiveThread; // 接收线程
        private readonly int _reconnectCount = 10;//总共重连10次
        private uint _retryTimes = 0;
        public bool IsConnected
        { 
            get { return _isConnected; }
        }

        public string Name
        {
            get;
            set;
        }

        public CustomTcpClient(string serverIp, int serverPort, int reconnectCount, int reconnectIntervalMilliseconds)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _reconnectCount = reconnectCount;
            _reconnectIntervalMilliseconds = reconnectIntervalMilliseconds;
        }

        public void Connect()
        {
            lock (_lockObject)
            {
                if (!_isConnected && _retryTimes <= _reconnectCount)
                {
                    try
                    {
                        _client = new TcpClient();
                        _client.Connect(_serverIp, _serverPort);
                        _stream = _client.GetStream();
                        _isConnected = true;
                        _retryTimes = 0;

                        // 设置TCP保活选项
                        Socket socket = _client.Client;
                        socket.ReceiveTimeout = 10000; // 接收超时时间
                        socket.SendTimeout = 10000; // 发送超时时间
                        socket.NoDelay = true; // 禁用Nagle算法
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        // 将整数数组转换为大端序字节数组
                        byte[] keepAliveValues = new byte[12];
                        BitConverter.GetBytes(_keepAliveTime).CopyTo(keepAliveValues, 0);
                        BitConverter.GetBytes(_keepAliveInterval).CopyTo(keepAliveValues, 4);
                        BitConverter.GetBytes(_keepAliveCount).CopyTo(keepAliveValues, 8);
                        socket.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);

                        // 开启接收线程
                        StartReceiveThread();
                    }
                    catch (Exception ex)
                    {
                        _retryTimes++;
                        LogHelper.Debug(ex.Message);
                        _isConnected = false;
                        Reconnect();
                    }
                }
            }
        }

        private void StartReceiveThread()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            _receiveThread = new Thread(() =>
            {
                while (_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        int bytesRead = _stream.Read(_readBuffer, 0, _readBuffer.Length);
                        if (bytesRead > 0)
                        {
                            byte[] receivedData = new byte[bytesRead];
                            Array.Copy(_readBuffer, receivedData, bytesRead);
                            OnDataReceived(receivedData);
                        }
                        //else
                        //{
                        //    Disconnect();
                        //}
                    }
                    catch (Exception ex)
                    {
                        //Disconnect();
                    }
                }
            });

            _receiveThread.Start();
        }

        private void Reconnect()
        {
            lock (_lockObject)
            {
                if (!_isConnected && _retryTimes <= _reconnectCount)
                {
                    // 使用一个标志来确保在重连过程中不会再次进入重连循环
                    bool shouldReconnect = true;

                    while (shouldReconnect)
                    {
                        Thread.Sleep(_reconnectIntervalMilliseconds);

                        lock (_lockObject)
                        {
                            if (!_isConnected)
                            {
                                Connect();
                                shouldReconnect = !_isConnected;
                            }
                            else
                            {
                                shouldReconnect = false;
                            }
                        }
                    }
                }
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                if (_isConnected)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource.Dispose();
                        _cancellationTokenSource = null;
                    }

                    if (_receiveThread != null)
                    {
                        _receiveThread.Join(); // 等待接收线程结束
                        _receiveThread = null;
                    }

                    _client.Close();

                    // 等待一段时间以确保端口被操作系统完全释放
                    Thread.Sleep(1000); // 1秒

                    _stream.Close();
                    _isConnected = false;
                }
            }
        }

        public void SendData(string data)
        {
            lock (_lockObject)
            {
                if (_isConnected)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(data);
                    try
                    {
                        _stream.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("Data sent: " + data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending data: {ex.Message}");
                        Disconnect();
                        Reconnect();
                    }
                }
            }
        }

        public void SendData(byte[] command, byte[] data)
        {
            lock (_lockObject)
            {
                if (_isConnected)
                {
                    byte[] buffer = FrameHelper.FrameData(command, data);
                    try
                    {
                        _stream.Write(buffer, 0, buffer.Length);
                        LogHelper.Info(BitConverter.ToString(buffer).Replace("-", " "));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                        Disconnect();
                        Reconnect();
                    }
                }
            }
        }

        public void SendData(byte[] command)
        {
            lock (_lockObject)
            {
                if (_isConnected)
                {
                    byte[] emptyArray = new byte[0];
                    byte[] buffer = FrameHelper.FrameData(command, emptyArray);
                    try
                    {
                        _stream.Write(buffer, 0, buffer.Length);
                        LogHelper.Info(BitConverter.ToString(buffer).Replace("-", " "));
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message);
                        Disconnect();
                        Reconnect();
                    }
                }
            }
        }

        protected virtual void OnDataReceived(byte[] data)
        {
            var tuple = FrameHelper.UnframeData(data);
            short result = BitConverter.ToInt16(tuple.Item1, 0);
            var command = CommandManager.GetInstance().Commands.Find(t => t.ID == result);
            byte[] bytes =
                [tuple.Item2[0], tuple.Item2[1], tuple.Item2[2], tuple.Item2[3]];
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes); // 确保大端字节序
            var a = BitConverter.ToSingle(bytes, 0);
            // 这里可以处理接收到的数据，例如输出到控制台
            LogHelper.Debug("Received data: " + BitConverter.ToString(data));
        }
    }
}
