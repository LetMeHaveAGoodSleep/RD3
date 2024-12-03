namespace Fpi.Communication.Protocols.Interfaces
{
    public delegate void RC_NotifyHandler(string mpId, string opId, string content, bool isDone);

    public interface IRC_Informer
    {
        event RC_NotifyHandler RC_NotifyEvent;
        void RC_StartOperation(string opId, string mpId, object opParam);

    }
}