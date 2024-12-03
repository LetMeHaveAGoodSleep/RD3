namespace Fpi.Communication.Commands
{
    public interface ICommandListener
    {
        void OnSendCommand(string instrumentId, Command command);
        void OnReceiveCommand(string instrumentId, Command sendCommand, Command receiveCommand);
    }
}