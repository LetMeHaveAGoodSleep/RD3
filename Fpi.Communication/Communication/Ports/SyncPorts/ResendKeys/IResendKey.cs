using Fpi.Communication.Interfaces;
namespace Fpi.Communication.Ports.SyncPorts.ResendKeys
{
    /// <summary>
    /// ����֡���ݻ�ȡ�ж��ط������֡��
    /// </summary>
    public interface IResendKey
    {
        //����֡�е��ط���
        object GetSendKey(IByteStream data);
        //����֡�е��ط���
        object GetReceiveKey(IByteStream data);
    }
}