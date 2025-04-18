namespace Fpi.Communication.Ports.Grouping
{
    public class SendState
    {
        //待发送
        public const int ToSend = 0;
        //已发送
        public const int Sended = 1;
        //已应答
        public const int Replayed = 2;
    }
}