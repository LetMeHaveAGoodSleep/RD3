using Fpi.Communication.Interfaces;
namespace Fpi.Communication.Ports.SyncPorts.ResendKeys
{
    /// <summary>
    /// 根据帧数据获取判断重发命令的帧。
    /// </summary>
    public interface IResendKey
    {
        //发送帧中的重发键
        object GetSendKey(IByteStream data);
        //接收帧中的重发键
        object GetReceiveKey(IByteStream data);
    }
}